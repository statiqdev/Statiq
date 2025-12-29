using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.Common;

namespace Statiq.App
{
    [Description("Executes the specified pipelines and provides an interactive prompt when complete.")]
    public class InteractiveCommand<TSettings> : PipelinesCommand<TSettings>
        where TSettings : PipelinesCommandSettings
    {
        private readonly AutoResetEvent _triggerExecutionEvent = new AutoResetEvent(false);
        private readonly InterlockedBool _exit = new InterlockedBool(false);

        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private ScriptOptions _scriptOptions;
        private ScriptState<object> _scriptState;

        public InteractiveCommand(
            IConfiguratorCollection configurators,
            Settings settings,
            IServiceCollection serviceCollection,
            IFileSystem fileSystem,
            Bootstrapper bootstrapper)
            : base(
                  configurators,
                  settings,
                  serviceCollection,
                  fileSystem,
                  bootstrapper)
        {
        }

        protected override async Task<int> ExecuteEngineAsync(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager)
        {
            ILogger logger = engineManager.Engine.Services.GetRequiredService<ILogger<Bootstrapper>>();
            using (_cancellationTokenSource)
            {
                SetPipelines(commandContext, commandSettings, engineManager);

                ExitCode exitCode = ExitCode.Normal;

                // Start the console listener
                ConsoleListener consoleListener = new ConsoleListener(
                    () =>
                    {
                        TriggerExit();
                        return Task.CompletedTask;
                    },
                    input => EvaluateScriptAsync(input, commandContext, commandSettings, engineManager));

                // Execute the engine for the first time
                exitCode = await engineManager.ExecuteAsync(_cancellationTokenSource);

                // Start previewing if we didn't cancel
                if (exitCode != ExitCode.OperationCanceled)
                {
                    await AfterInitialExecutionAsync(commandContext, commandSettings, engineManager, _cancellationTokenSource);

                    // Log that we're ready and start waiting on input
                    const string prompt = "Type Ctrl-C or \"Exit()\" to exit and \"Help()\" for global methods and properties";
                    logger.LogInformation(prompt);
                    ConsoleLoggerProvider.FlushAndWait();
                    consoleListener.StartReadingLines();

                    // Wait for activity
                    while (true)
                    {
                        // Blocks the current thread until a signal
                        // This will also reset the event (since it's an AutoResetEvent) so any triggering will cause a following execution
                        _triggerExecutionEvent.WaitOne();

                        // Stop listening while we run again
                        consoleListener.StopReadingLines();

                        // Break here before running if we're exiting
                        if (_exit)
                        {
                            break;
                        }

                        // Execute
                        exitCode = await ExecutionTriggeredAsync(commandContext, commandSettings, engineManager, exitCode, _cancellationTokenSource);

                        // Check one more time for exit
                        if (_exit)
                        {
                            break;
                        }

                        // Log that we're ready and start waiting on input (again)
                        logger.LogInformation(prompt);
                        ConsoleLoggerProvider.FlushAndWait();
                        consoleListener.StartReadingLines();
                    }
                }

                // Shutdown
                logger.LogInformation("Exiting...");
                await ExitingAsync(commandContext, commandSettings, engineManager);

                return (int)exitCode;
            }
        }

        public void TriggerExecution() => _triggerExecutionEvent.Set();

        public void TriggerExit()
        {
            _exit.Set();
            _triggerExecutionEvent.Set();
            _cancellationTokenSource.Cancel();
        }

        protected virtual Task AfterInitialExecutionAsync(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager,
            CancellationTokenSource cancellationTokenSource) =>
            Task.CompletedTask;

        protected virtual async Task<ExitCode> ExecutionTriggeredAsync(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager,
            ExitCode previousExitCode,
            CancellationTokenSource cancellationTokenSource) =>
            await engineManager.ExecuteAsync(cancellationTokenSource);

        protected virtual Task ExitingAsync(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager) =>
            Task.CompletedTask;

        protected virtual InteractiveGlobals GetInteractiveGlobals(
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager) =>
            new InteractiveGlobals(engineManager.Engine, TriggerExecution, TriggerExit);

        private async Task EvaluateScriptAsync(
            string code,
            CommandContext commandContext,
            TSettings commandSettings,
            IEngineManager engineManager)
        {
            if (!code.IsNullOrWhiteSpace())
            {
                // Create the script options
                if (_scriptOptions is null)
                {
                    IEnumerable<MetadataReference> references = engineManager.Engine.ScriptHelper
                        .GetScriptReferences().Select(x => MetadataReference.CreateFromFile(x.Location));
                    _scriptOptions = ScriptOptions.Default
                        .WithReferences(references)
                        .WithImports(engineManager.Engine.ScriptHelper.GetScriptNamespaces());
                }

                // Run the script
                try
                {
                    if (_scriptState is null)
                    {
                        InteractiveGlobals interactiveGlobals = GetInteractiveGlobals(commandContext, commandSettings, engineManager);
                        _scriptState = await CSharpScript.RunAsync(code, _scriptOptions, globals: interactiveGlobals, cancellationToken: _cancellationTokenSource.Token);
                    }
                    else
                    {
                        _scriptState = await _scriptState.ContinueWithAsync(code, _scriptOptions, cancellationToken: _cancellationTokenSource.Token);
                    }
                }
                catch (CompilationErrorException e)
                {
                    Console.WriteLine(string.Join(Environment.NewLine, e.Diagnostics));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }

                // Output the result (if any)
                if (_scriptState?.ReturnValue is object && TypeHelper.TryConvert(_scriptState.ReturnValue, out string result))
                {
                    Console.WriteLine(result);
                }
            }
        }

        // This needs to be lazily evaluated so that we can change it after the configuration settings are copied to the engine
        private class ResetCacheMetadataValue : IMetadataValue
        {
            public bool Value { get; set; }

            public object Get(string key, IMetadata metadata) => Value;
        }
    }
}