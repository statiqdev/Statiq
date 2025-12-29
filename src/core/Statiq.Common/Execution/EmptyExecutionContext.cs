using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    internal class EmptyExecutionContext : IExecutionContext
    {
        public EmptyExecutionContext(IExecutionState executionState)
        {
            ExecutionState = executionState.ThrowIfNull(nameof(executionState));
        }

        public IExecutionState ExecutionState { get; }

        public string PipelineName => default;

        public IReadOnlyPipeline Pipeline => default;

        public Phase Phase => default;

        public IExecutionContext Parent => null;

        public IModule Module => default;

        public ImmutableArray<IDocument> Inputs => ImmutableArray<IDocument>.Empty;

        public Guid ExecutionId => ExecutionState.ExecutionId;

        public DateTime ExecutionDateTime => ExecutionState.ExecutionDateTime;

        public CancellationToken CancellationToken => ExecutionState.CancellationToken;

        public IApplicationState ApplicationState => ExecutionState.ApplicationState;

        public ClassCatalog ClassCatalog => ExecutionState.ClassCatalog;

        public bool SerialExecution => ExecutionState.SerialExecution;

        public IReadOnlyEventCollection Events => ExecutionState.Events;

        public IReadOnlyFileSystem FileSystem => ExecutionState.FileSystem;

        public IReadOnlySettings Settings => ExecutionState.Settings;

        public IReadOnlyShortcodeCollection Shortcodes => ExecutionState.Shortcodes;

        public INamespacesCollection Namespaces => ExecutionState.Namespaces;

        public IMemoryStreamFactory MemoryStreamFactory => ExecutionState.MemoryStreamFactory;

        public IPipelineOutputs Outputs => ExecutionState.Outputs;

        public FilteredDocumentList<IDocument> OutputPages => ExecutionState.OutputPages;

        public IServiceProvider Services => ExecutionState.Services;

        public ILogger Logger => ExecutionState.Logger;

        public IScriptHelper ScriptHelper => ExecutionState.ScriptHelper;

        public IReadOnlyPipelineCollection Pipelines => ExecutionState.Pipelines;

        public IReadOnlyPipelineCollection ExecutingPipelines => ExecutionState.ExecutingPipelines;

        public ILinkGenerator LinkGenerator => ExecutionState.LinkGenerator;

        public void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            ExecutionState.SetDefaultDocumentType<TDocument>();

        public IDocument CreateDocument(NormalizedPath source, NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null) =>
            ExecutionState.CreateDocument(source, destination, items, contentProvider);

        public TDocument CreateDocument<TDocument>(NormalizedPath source, NormalizedPath destination, IEnumerable<KeyValuePair<string, object>> items, IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            ExecutionState.CreateDocument<TDocument>(source, destination, items, contentProvider);

        public HttpClient CreateHttpClient() => ExecutionState.CreateHttpClient();

        public HttpClient CreateHttpClient(HttpMessageHandler handler) => ExecutionState.CreateHttpClient(handler);

        public Task<ImmutableArray<IDocument>> ExecuteModulesAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs) =>
            throw new NotSupportedException("Not supported in an empty execution context");

        public Stream GetContentStream(string content = null) => ExecutionState.GetContentStream(content);

        public IJavaScriptEnginePool GetJavaScriptEnginePool(Action<IJavaScriptEngine> initializer = null, int startEngines = 10, int maxEngines = 25, int maxUsagesPerEngine = 100, TimeSpan? engineTimeout = null) =>
            ExecutionState.GetJavaScriptEnginePool(initializer, startEngines, maxEngines, maxUsagesPerEngine, engineTimeout);

        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory) =>
            ExecutionState.SendHttpRequestWithRetryAsync(requestFactory);

        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory, int retryCount) =>
            ExecutionState.SendHttpRequestWithRetryAsync(requestFactory, retryCount);

        // IServiceProvider

        public object GetService(Type serviceType) => ExecutionState.Services.GetService(serviceType);

        // ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            Logger.Log(logLevel, eventId, state, exception, (s, e) => formatter(s, e));

        public bool IsEnabled(LogLevel logLevel) => Logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => Logger.BeginScope(state);
    }
}