using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly.Caching;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// This contains the pipeline execution context and other data
    /// needed to execute a pipeline and cache it's results.
    /// </summary>
    internal class PipelinePhase : IDisposable
    {
        private readonly IList<IModule> _modules;
        private readonly ILogger _logger;
        private bool _disposed;

        public PipelinePhase(IPipeline pipeline, string pipelineName, Phase phase, IList<IModule> modules, ILogger logger, params PipelinePhase[] dependencies)
        {
            Pipeline = pipeline;
            PipelineName = pipelineName;
            Phase = phase;
            _modules = modules ?? new List<IModule>();
            _logger = logger.ThrowIfNull(nameof(logger));
            Dependencies = dependencies ?? Array.Empty<PipelinePhase>();
        }

        public IPipeline Pipeline { get; }

        public string PipelineName { get; }

        public Phase Phase { get; }

        /// <summary>
        /// Contains direct dependencies for this pipeline and phase.
        /// The first dependency should contain the input documents for this phase.
        /// </summary>
        public PipelinePhase[] Dependencies { get; set; }

        /// <summary>
        /// Holds the output documents from the previous execution of this phase.
        /// </summary>
        public ImmutableArray<IDocument> Outputs { get; private set; } = ImmutableArray<IDocument>.Empty;

        /// <summary>
        /// The first dependency always holds the input documents for this phase.
        /// </summary>
        /// <remarks>
        /// Deployment pipeline input phases have a dependency on non-deployment pipeline output phases for ordering,
        /// but we don't want to bring those in to deployment input phases so always provide an empty set of inputs to input phases.
        /// </remarks>
        /// <returns>The input documents for this phase.</returns>
        private ImmutableArray<IDocument> GetInputs() => Phase == Phase.Input || Dependencies.Length == 0 ? ImmutableArray<IDocument>.Empty : Dependencies[0].Outputs;

        // This is the main execute method called by the engine
        public async Task ExecuteAsync(
            Engine engine,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            ConcurrentDictionary<PipelinePhase, ConcurrentBag<AnalyzerResult>> analyzerResults)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(PipelinePhase));
            }

            // Raise the before event
            await engine.Events.RaiseAsync(new BeforePipelinePhaseExecution(engine.ExecutionId, PipelineName, Phase));

            // Skip the phase if there are no modules
            DateTimeOffset startTime = DateTimeOffset.Now;
            long elapsedMilliseconds = -1;
            if (_modules.Count == 0)
            {
                Outputs = GetInputs();
                _logger.LogDebug($"{PipelineName}/{Phase} » Pipeline contains no modules, skipping");
            }
            else
            {
                // Execute the phase
                ImmutableArray<IDocument> inputs = GetInputs();
                System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                _logger.LogInformation($"-> {PipelineName}/{Phase} » Starting {PipelineName} {Phase} phase execution... ({inputs.Length} input document(s), {_modules.Count} module(s))");
                try
                {
                    // Execute all modules in the pipeline with a new DI scope per phase
                    IServiceScopeFactory serviceScopeFactory = engine.Services.GetRequiredService<IServiceScopeFactory>();
                    using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
                    {
                        ExecutionContextData contextData = new ExecutionContextData(
                            this,
                            engine,
                            phaseResults,
                            serviceScope.ServiceProvider);
                        Outputs = await Engine.ExecuteModulesAsync(contextData, null, _modules, inputs, _logger);
                        stopwatch.Stop();
                        elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
                        _logger.LogInformation($"<- {PipelineName}/{Phase} » Finished {PipelineName} {Phase} phase execution ({Outputs.Length} output document(s), {elapsedMilliseconds} ms)");
                    }
                }
                catch (Exception ex)
                {
                    Outputs = ImmutableArray<IDocument>.Empty;

                    // Report on the exception if it's not a "true" cancellation of the engine (I.e. a timeout or internal cancellation)
                    if (!engine.CancellationToken.IsCancellationRequested)
                    {
                        string exceptionType = "Exception";
                        Exception exceptionToLog = ex;
                        LoggedException executeModulesException = ex as LoggedException;

                        // Was this a timeout (already tested that IsCancellationRequested is false)
                        if (ex is OperationCanceledException)
                        {
                            exceptionType = "Timeout/Cancellation";
                            exceptionToLog = ex.InnerException ?? ex;
                        }
                        else if (executeModulesException is object)
                        {
                            // ...or was it a logged exception
                            exceptionToLog = executeModulesException.InnerException;
                        }

                        // Log the exception (or inner exception)
                        _logger.LogDebug($"{exceptionType} while executing pipeline {PipelineName}/{Phase}: {exceptionToLog}");
                        if (executeModulesException is object && exceptionToLog is object)
                        {
                            throw exceptionToLog;
                        }
                    }

                    // Always rethrow the exception
                    throw;
                }
                finally
                {
                    stopwatch.Stop();
                }
            }

            // Raise the after event
            await engine.Events.RaiseAsync(new AfterPipelinePhaseExecution(engine.ExecutionId, PipelineName, Phase, Outputs, elapsedMilliseconds < 0 ? 0 : elapsedMilliseconds));

            // Run analyzers
            await RunAnalyzersAsync(engine, phaseResults, analyzerResults);

            // Record the results if execution actually ran
            if (elapsedMilliseconds >= 0)
            {
                PhaseResult phaseResult = new PhaseResult(PipelineName, Phase, Outputs, startTime, elapsedMilliseconds);
                phaseResults.AddOrUpdate(
                    phaseResult.PipelineName,
                    _ =>
                    {
                        PhaseResult[] results = new PhaseResult[4];
                        results[(int)phaseResult.Phase] = phaseResult;
                        return results;
                    },
                    (_, results) =>
                    {
                        if (results[(int)phaseResult.Phase] is object)
                        {
                            // Sanity check, we should never hit this
                            throw new InvalidOperationException($"Results for phase {phaseResult.Phase} have already been added");
                        }
                        results[(int)phaseResult.Phase] = phaseResult;
                        return results;
                    });
            }
        }

        // Outputs should be set before making this call
        private async Task RunAnalyzersAsync(
            Engine engine,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            ConcurrentDictionary<PipelinePhase, ConcurrentBag<AnalyzerResult>> analyzerResults)
        {
            // We need to create an execution context so the async static instance is set for analyzers
            IServiceScopeFactory serviceScopeFactory = engine.Services.GetRequiredService<IServiceScopeFactory>();
            using (IServiceScope serviceScope = serviceScopeFactory.CreateScope())
            {
                ExecutionContextData contextData = new ExecutionContextData(
                    this,
                    engine,
                    phaseResults,
                    serviceScope.ServiceProvider);
                ExecutionContext executionContext = new ExecutionContext(contextData, null, null, Outputs);

                // Run analyzers for this pipeline and phase, don't check log level here since each document could override it
                ConcurrentBag<AnalyzerResult> results = new ConcurrentBag<AnalyzerResult>();
                KeyValuePair<string, IAnalyzer>[] analyzerItems = engine.AnalyzerCollection
                    .Where(analyzerItem => analyzerItem.Value.PipelinePhases?.Any(pipelinePhase => pipelinePhase.Key.Equals(PipelineName, StringComparison.OrdinalIgnoreCase) && pipelinePhase.Value == Phase) == true)
                    .ToArray();
                if (analyzerItems.Length > 0)
                {
                    _logger.LogInformation($"{PipelineName}/{Phase} » Running {analyzerItems.Length} analyzers ({string.Join(", ", analyzerItems.Select(x => x.Key))})");
                    await analyzerItems
                        .ParallelForEachAsync(async analyzerItem =>
                        {
                            AnalyzerContext analyzerContext = new AnalyzerContext(contextData, Outputs, analyzerItem, results);
                            await analyzerItem.Value.AnalyzeAsync(analyzerContext);
                        });
                }

                // Add the results before the error check so the exact results can be reported later
                if (!analyzerResults.TryAdd(this, results))
                {
                    // Sanity check, should never get here
                    throw new InvalidOperationException($"Analyzer results for pipeline {PipelineName} already added");
                }

                // Throw if any results are above error
                if (results.Any(x => x.LogLevel != LogLevel.None && x.LogLevel >= LogLevel.Error))
                {
                    const string message = "One or more analyzers produced error results, see analyzer report following execution";
                    _logger.Log(LogLevel.Error, new StatiqLogState { LogToBuildServer = false }, message);
                    throw new LoggedException(new ExecutionException(message));
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Pipeline));
            }
            _disposed = true;

            DisposeModules(_modules);
        }

        private static void DisposeModules(IEnumerable<IModule> modules)
        {
            foreach (IModule module in modules)
            {
                (module as IDisposable)?.Dispose();
                if (module is IEnumerable<IModule> childModules)
                {
                    DisposeModules(childModules);
                }
            }
        }
    }
}