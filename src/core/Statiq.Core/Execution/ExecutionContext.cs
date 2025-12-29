using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ExecutionContext : IExecutionContext
    {
        private readonly ExecutionContextData _contextData;
        private readonly string _logPrefix;

        // Cache the output pages per-context for performance
        private readonly Lazy<FilteredDocumentList<IDocument>> _outputPages;

        internal ExecutionContext(ExecutionContextData contextData, IExecutionContext parent, IModule module, ImmutableArray<IDocument> inputs)
        {
            _contextData = contextData.ThrowIfNull(nameof(contextData));
            Logger = contextData.Services.GetRequiredService<ILogger<ExecutionContext>>();
            _logPrefix = GetLogPrefix(parent, module, contextData.PipelinePhase);

            _outputPages = new Lazy<FilteredDocumentList<IDocument>>(
                () => new FilteredDocumentList<IDocument>(
                    Outputs
                        .Where(x => !x.Destination.IsNullOrEmpty
                            && Settings.GetPageFileExtensions().Any(e => x.Destination.Extension.Equals(e, NormalizedPath.DefaultComparisonType))),
                    x => x.Destination,
                    (docs, patterns) => docs.FilterDestinations(patterns)),
                LazyThreadSafetyMode.ExecutionAndPublication);

            Parent = parent;
            Module = module; // Can be null if in an analyzer
            Inputs = inputs;

            IExecutionContext.Current = this;
        }

        private static string GetLogPrefix(IExecutionContext parent, IModule module, PipelinePhase pipelinePhase)
        {
            string moduleComponent = string.Empty;
            if (module is object)
            {
                Stack<string> modules = new Stack<string>();
                modules.Push(module.GetType().Name);
                while (parent is object)
                {
                    modules.Push(parent.Module.GetType().Name);
                    parent = parent.Parent;
                }
                moduleComponent = $"{string.Join(" » ", modules)} » ";
            }
            return $"{pipelinePhase.PipelineName}/{pipelinePhase.Phase} » {moduleComponent}";
        }

        /// <inheritdoc/>
        public IExecutionState ExecutionState => _contextData.Engine;

        /// <inheritdoc/>
        public Guid ExecutionId => _contextData.Engine.ExecutionId;

        /// <inheritdoc/>
        public DateTime ExecutionDateTime => _contextData.Engine.ExecutionDateTime;

        /// <inheritdoc/>
        public INamespacesCollection Namespaces => _contextData.Engine.Namespaces;

        /// <inheritdoc/>
        public IReadOnlyPipeline Pipeline => _contextData.PipelinePhase.Pipeline;

        /// <inheritdoc/>
        public string PipelineName => _contextData.PipelinePhase.PipelineName;

        /// <inheritdoc/>
        public Phase Phase => _contextData.PipelinePhase.Phase;

        /// <inheritdoc/>
        public IReadOnlyEventCollection Events => _contextData.Engine.Events;

        /// <inheritdoc/>
        public IPipelineOutputs Outputs => _contextData.Outputs;

        /// <inheritdoc/>
        public ILogger Logger { get; }

        /// <inheritdoc />
        public FilteredDocumentList<IDocument> OutputPages => _outputPages.Value;

        /// <inheritdoc/>
        public IReadOnlyFileSystem FileSystem => _contextData.Engine.FileSystem;

        /// <inheritdoc/>
        public IReadOnlySettings Settings => _contextData.Engine.Settings;

        /// <inheritdoc/>
        public IReadOnlyShortcodeCollection Shortcodes => _contextData.Engine.Shortcodes;

        /// <inheritdoc/>
        public IApplicationState ApplicationState => _contextData.Engine.ApplicationState;

        /// <inheritdoc/>
        public ClassCatalog ClassCatalog => _contextData.Engine.ClassCatalog;

        /// <inheritdoc/>
        public IServiceProvider Services => _contextData.Services; // Expose the scoped provider inside the execution context

        /// <inheritdoc/>
        public bool SerialExecution => _contextData.Engine.SerialExecution;

        /// <inheritdoc/>
        public IMemoryStreamFactory MemoryStreamFactory => _contextData.Engine.MemoryStreamFactory;

        /// <inheritdoc/>
        public IScriptHelper ScriptHelper => _contextData.Engine.ScriptHelper;

        /// <inheritdoc/>
        public IReadOnlyPipelineCollection Pipelines => _contextData.Engine.Pipelines;

        /// <inheritdoc/>
        public IReadOnlyPipelineCollection ExecutingPipelines => _contextData.Engine.ExecutingPipelines;

        /// <inheritdoc/>
        public ILinkGenerator LinkGenerator => _contextData.Engine.LinkGenerator;

        /// <inheritdoc/>
        public CancellationToken CancellationToken => _contextData.Engine.CancellationToken;

        /// <inheritdoc/>
        public IExecutionContext Parent { get; }

        /// <inheritdoc/>
        public virtual IModule Module { get; }

        /// <inheritdoc/>
        public ImmutableArray<IDocument> Inputs { get; }

        /// <inheritdoc/>
        public HttpClient CreateHttpClient() => _contextData.Engine.CreateHttpClient();

        /// <inheritdoc/>
        public HttpClient CreateHttpClient(HttpMessageHandler handler) => _contextData.Engine.CreateHttpClient(handler);

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory) =>
            _contextData.Engine.SendHttpRequestWithRetryAsync(requestFactory);

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory, int retryCount) =>
            _contextData.Engine.SendHttpRequestWithRetryAsync(requestFactory, retryCount);

        /// <inheritdoc/>
        public async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs) =>
            await Engine.ExecuteModulesAsync(_contextData, this, modules, inputs?.ToImmutableArray() ?? ImmutableArray<IDocument>.Empty, this);

        /// <inheritdoc/>
        public Stream GetContentStream(string content = null) => _contextData.Engine.GetContentStream(content);

        /// <inheritdoc/>
        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            _contextData.Engine.GetJavaScriptEnginePool(initializer, startEngines, maxEngines, maxUsagesPerEngine, engineTimeout);

        // IDocumentFactory

        void IDocumentFactory.SetDefaultDocumentType<TDocument>() =>
            throw new NotSupportedException("Cannot change default document type during execution");

        /// <inheritdoc />
        public IDocument CreateDocument(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            _contextData.Engine.DocumentFactory.CreateDocument(source, destination, items, contentProvider);

        /// <inheritdoc />
        public TDocument CreateDocument<TDocument>(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            _contextData.Engine.DocumentFactory.CreateDocument<TDocument>(source, destination, items, contentProvider);

        // IServiceProvider

        public object GetService(Type serviceType) => _contextData.Services.GetService(serviceType);

        // ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            Logger.Log(logLevel, eventId, state, exception, (s, e) => _logPrefix + formatter(s, e));

        public bool IsEnabled(LogLevel logLevel) => Logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => Logger.BeginScope(state);
    }
}