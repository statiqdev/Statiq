using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestEngine : IEngine
    {
        public TestEngine()
        {
            IExecutionState.Current = this;

            _documentFactory = new DocumentFactory(this, Settings);
            _documentFactory.SetDefaultDocumentType<TestDocument>();

            TestLoggerProvider = new TestLoggerProvider(LogMessages);
            Services = new TestServiceProvider(
                serviceCollection =>
                {
                    serviceCollection.AddLogging();
                    serviceCollection.AddSingleton<ILoggerProvider>(TestLoggerProvider);
                    serviceCollection.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
                });
        }

        public TestLoggerProvider TestLoggerProvider { get; }

        public ILogger Logger => TestLoggerProvider.CreateLogger(null);

        public ConcurrentQueue<TestMessage> LogMessages { get; } = new ConcurrentQueue<TestMessage>();

        /// <inheritdoc />
        public Guid ExecutionId { get; set; } = Guid.Empty;

        /// <inheritdoc />
        public DateTime ExecutionDateTime { get; set; } = DateTime.Now;

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; set; }

        /// <inheritdoc />
        public IApplicationState ApplicationState { get; set; }

        /// <inheritdoc />
        public ClassCatalog ClassCatalog { get; } = new ClassCatalog();  // Don't initially populate in case we don't actually need it

        /// <inheritdoc />
        IApplicationState IExecutionState.ApplicationState => ApplicationState;

        /// <inheritdoc />
        public Settings Settings { get; set; } = new Settings();

        /// <inheritdoc />
        ISettings IEngine.Settings => Settings;

        /// <inheritdoc />
        IReadOnlySettings IExecutionState.Settings => Settings;

        /// <inheritdoc />
        public TestEventCollection Events { get; set; } = new TestEventCollection();

        /// <inheritdoc />
        IEventCollection IEngine.Events => Events;

        /// <inheritdoc />
        IReadOnlyEventCollection IExecutionState.Events => Events;

        /// <inheritdoc />
        public TestServiceProvider Services { get; set; }

        /// <inheritdoc />
        IServiceProvider IExecutionState.Services => Services;

        /// <inheritdoc />
        public TestFileSystem FileSystem { get; set; } = new TestFileSystem();

        /// <inheritdoc />
        IReadOnlyFileSystem IExecutionState.FileSystem => FileSystem;

        /// <inheritdoc />
        public TestMemoryStreamFactory MemoryStreamFactory { get; set; } = new TestMemoryStreamFactory();

        /// <inheritdoc />
        IMemoryStreamFactory IExecutionState.MemoryStreamFactory => MemoryStreamFactory;

        /// <inheritdoc />
        public TestPipelineCollection Pipelines { get; set; } = new TestPipelineCollection();

        /// <inheritdoc />
        IPipelineCollection IEngine.Pipelines => Pipelines;

        /// <inheritdoc />
        IReadOnlyPipelineCollection IExecutionState.Pipelines => Pipelines;

        /// <inheritdoc />
        public IReadOnlyPipelineCollection ExecutingPipelines => Pipelines;

        /// <inheritdoc />
        public TestShortcodeCollection Shortcodes { get; set; } = new TestShortcodeCollection();

        /// <inheritdoc />
        IShortcodeCollection IEngine.Shortcodes => Shortcodes;

        /// <inheritdoc />
        IReadOnlyShortcodeCollection IExecutionState.Shortcodes => Shortcodes;

        /// <inheritdoc />
        public TestNamespacesCollection Namespaces { get; set; } = new TestNamespacesCollection();

        /// <inheritdoc />
        INamespacesCollection IExecutionState.Namespaces => Namespaces;

        /// <inheritdoc />
        public bool SerialExecution { get; set; }

        /// <inheritdoc/>
        public TestPipelineOutputs Outputs { get; set; } = new TestPipelineOutputs();

        /// <inheritdoc />
        IPipelineOutputs IExecutionState.Outputs => Outputs;

        /// <inheritdoc />
        public TestAnalyzerCollection Analyzers { get; set; } = new TestAnalyzerCollection();

        /// <inheritdoc />
        IAnalyzerCollection IEngine.Analyzers => Analyzers;

        /// <inheritdoc />
        public IFileCleaner FileCleaner { get; set; } = new TestFileCleaner();

        /// <inheritdoc />
        public ILinkGenerator LinkGenerator { get; set; } = new LinkGenerator();

        /// <inheritdoc />
        public FilteredDocumentList<IDocument> OutputPages =>
            new FilteredDocumentList<IDocument>(
                Outputs
                    .Where(x => !x.Destination.IsNullOrEmpty
                        && Settings.GetPageFileExtensions().Any(e => x.Destination.Extension.Equals(e, NormalizedPath.DefaultComparisonType))),
                x => x.Destination,
                (docs, patterns) => docs.FilterDestinations(patterns));

        /// <inheritdoc/>
        public IScriptHelper ScriptHelper { get; set; }

        /// <inheritdoc />
        IScriptHelper IExecutionState.ScriptHelper => ScriptHelper;

        private readonly DocumentFactory _documentFactory;

        /// <inheritdoc/>
        public Stream GetContentStream(string content = null) => new TestContentStream(this, content);

        /// <inheritdoc/>
        public HttpClient CreateHttpClient() =>
            new HttpClient(new TestHttpMessageHandler(HttpResponseFunc, null));

        /// <inheritdoc/>
        public HttpClient CreateHttpClient(HttpMessageHandler handler) =>
            new HttpClient(new TestHttpMessageHandler(HttpResponseFunc, handler));

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory)
        {
            // No retry while testing
            using (HttpClient httpClient = CreateHttpClient())
            {
                return await httpClient.SendAsync(requestFactory());
            }
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory, int retryCount) =>
            await SendHttpRequestWithRetryAsync(requestFactory);

        /// <summary>
        /// A message handler that should be used to register <see cref="HttpResponseMessage"/>
        /// instances for a given request.
        /// </summary>
        public Func<HttpRequestMessage, HttpMessageHandler, HttpResponseMessage> HttpResponseFunc { get; set; }
            = (_, __) => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new System.Net.Http.StringContent(string.Empty)
            };

        /// <inheritdoc/>
        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            new TestJsEnginePool(JsEngineFunc, initializer);

        public Func<IJavaScriptEngine> JsEngineFunc { get; set; } = () =>
            throw new NotImplementedException("JavaScript test engine not initialized. Statiq.Testing.JavaScript can be used to return a working JavaScript engine");

        /// <inheritdoc />
        public void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            _documentFactory.SetDefaultDocumentType<TDocument>();

        /// <inheritdoc />
        public IDocument CreateDocument(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            _documentFactory.CreateDocument(source, destination, items, contentProvider);

        /// <inheritdoc />
        public TDocument CreateDocument<TDocument>(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            _documentFactory.CreateDocument<TDocument>(source, destination, items, contentProvider);
    }
}