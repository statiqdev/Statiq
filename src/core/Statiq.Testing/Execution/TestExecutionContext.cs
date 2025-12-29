using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Statiq.Common;

namespace Statiq.Testing
{
    /// <summary>
    /// An <see cref="IExecutionContext"/> that can be used for testing.
    /// </summary>
    public class TestExecutionContext : IExecutionContext
    {
        private readonly DocumentFactory _documentFactory;
        private readonly ILogger _logger;

        public TestExecutionContext()
            : this((IEnumerable<IDocument>)null)
        {
        }

        public TestExecutionContext(params IDocument[] inputs)
            : this((IEnumerable<IDocument>)inputs)
        {
        }

        public TestExecutionContext(IEnumerable<IDocument> inputs)
        {
            _documentFactory = new DocumentFactory(this, Settings);
            _documentFactory.SetDefaultDocumentType<TestDocument>();

            if (inputs is object)
            {
                SetInputs(inputs);
            }

            _logger = Engine.TestLoggerProvider.CreateLogger(null);

            IExecutionContext.Current = this;
        }

        public TestLoggerProvider TestLoggerProvider => Engine.TestLoggerProvider;

        public ILogger Logger => Engine.Logger;

        public ConcurrentQueue<TestMessage> LogMessages => Engine.LogMessages;

        // IExecutionContext

        public TestEngine Engine { get; set; } = new TestEngine();

        /// <inheritdoc/>
        public Guid ExecutionId => Engine.ExecutionId;

        /// <inheritdoc />
        public DateTime ExecutionDateTime => Engine.ExecutionDateTime;

        /// <inheritdoc/>
        IExecutionState IExecutionContext.ExecutionState => Engine;

        /// <inheritdoc />
        public TestServiceProvider Services
        {
            get => Engine.Services;
            set => Engine.Services = value;
        }

        /// <inheritdoc />
        IServiceProvider IExecutionState.Services => Services;

        /// <inheritdoc />
        public Settings Settings
        {
            get => Engine.Settings;
            set => Engine.Settings = value;
        }

        /// <inheritdoc />
        IReadOnlySettings IExecutionState.Settings => Settings;

        /// <inheritdoc/>
        public TestNamespacesCollection Namespaces
        {
            get => Engine.Namespaces;
            set => Engine.Namespaces = value;
        }

        /// <inheritdoc/>
        INamespacesCollection IExecutionState.Namespaces => Namespaces;

        /// <inheritdoc/>
        public ILinkGenerator LinkGenerator
        {
            get => Engine.LinkGenerator;
            set => Engine.LinkGenerator = value;
        }

        /// <inheritdoc />
        public TestEventCollection Events
        {
            get => Engine.Events;
            set => Engine.Events = value;
        }

        /// <inheritdoc />
        IReadOnlyEventCollection IExecutionState.Events => Events;

        /// <inheritdoc/>
        public TestFileSystem FileSystem
        {
            get => Engine.FileSystem;
            set => Engine.FileSystem = value;
        }

        /// <inheritdoc/>
        IReadOnlyFileSystem IExecutionState.FileSystem => FileSystem;

        /// <inheritdoc/>
        public TestPipelineOutputs Outputs
        {
            get => Engine.Outputs;
            set => Engine.Outputs = value;
        }

        /// <inheritdoc/>
        IPipelineOutputs IExecutionState.Outputs => Outputs;

        /// <inheritdoc />
        public FilteredDocumentList<IDocument> OutputPages =>
            new FilteredDocumentList<IDocument>(
                Outputs
                    .Where(x => !x.Destination.IsNullOrEmpty
                        && Settings.GetPageFileExtensions().Any(e => x.Destination.Extension.Equals(e, NormalizedPath.DefaultComparisonType))),
                x => x.Destination,
                (docs, patterns) => docs.FilterDestinations(patterns));

        /// <inheritdoc/>
        public IApplicationState ApplicationState
        {
            get => Engine.ApplicationState;
            set => Engine.ApplicationState = value;
        }

        /// <inheritdoc/>
        IApplicationState IExecutionState.ApplicationState => ApplicationState;

        /// <inheritdoc/>
        public ClassCatalog ClassCatalog => Engine.ClassCatalog;

        /// <inheritdoc/>
        public bool SerialExecution
        {
            get => Engine.SerialExecution;
            set => Engine.SerialExecution = value;
        }

        /// <inheritdoc />
        public TestShortcodeCollection Shortcodes
        {
            get => Engine.Shortcodes;
            set => Engine.Shortcodes = value;
        }

        /// <inheritdoc/>
        IReadOnlyShortcodeCollection IExecutionState.Shortcodes => Shortcodes;

        /// <inheritdoc />
        public TestPipelineCollection Pipelines
        {
            get => Engine.Pipelines;
            set => Engine.Pipelines = value;
        }

        /// <inheritdoc/>
        IReadOnlyPipelineCollection IExecutionState.Pipelines => Pipelines;

        /// <inheritdoc/>
        IReadOnlyPipelineCollection IExecutionState.ExecutingPipelines => Pipelines;

        /// <inheritdoc/>
        public TestMemoryStreamFactory MemoryStreamFactory
        {
            get => Engine.MemoryStreamFactory;
            set => Engine.MemoryStreamFactory = value;
        }

        /// <inheritdoc/>
        IMemoryStreamFactory IExecutionState.MemoryStreamFactory => MemoryStreamFactory;

        /// <inheritdoc/>
        public IScriptHelper ScriptHelper
        {
            get => Engine.ScriptHelper;
            set => Engine.ScriptHelper = value;
        }

        /// <inheritdoc/>
        IScriptHelper IExecutionState.ScriptHelper => ScriptHelper;

        /// <inheritdoc/>
        public CancellationToken CancellationToken
        {
            get => Engine.CancellationToken;
            set => Engine.CancellationToken = value;
        }

        /// <inheritdoc/>
        public string PipelineName { get; set; }

        /// <inheritdoc/>
        public TestPipeline Pipeline { get; set; } = new TestPipeline();

        /// <inheritdoc/>
        IReadOnlyPipeline IExecutionContext.Pipeline => Pipeline;

        /// <inheritdoc/>
        public Phase Phase { get; set; } = Phase.Process;

        /// <inheritdoc/>
        public IExecutionContext Parent { get; set; }

        /// <inheritdoc/>
        public IModule Module { get; set; }

        /// <inheritdoc/>
        public ImmutableArray<IDocument> Inputs { get; set; } = ImmutableArray<IDocument>.Empty;

        public void SetInputs(IEnumerable<IDocument> inputs) =>
            Inputs = inputs?.Where(x => x is object).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;

        public void SetInputs(params IDocument[] inputs) =>
            SetInputs((IEnumerable<IDocument>)inputs);

        /// <inheritdoc/>
        public Stream GetContentStream(string content = null) => Engine.GetContentStream(content);

        /// <inheritdoc/>
        public HttpClient CreateHttpClient() => Engine.CreateHttpClient();

        /// <inheritdoc/>
        public HttpClient CreateHttpClient(HttpMessageHandler handler) => Engine.CreateHttpClient(handler);

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory) =>
            Engine.SendHttpRequestWithRetryAsync(requestFactory);

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory, int retryCount) =>
            Engine.SendHttpRequestWithRetryAsync(requestFactory, retryCount);

        /// <summary>
        /// A message handler that should be used to register <see cref="HttpResponseMessage"/>
        /// instances for a given request.
        /// </summary>
        public Func<HttpRequestMessage, HttpMessageHandler, HttpResponseMessage> HttpResponseFunc
        {
            get => Engine.HttpResponseFunc;
            set => Engine.HttpResponseFunc = value;
        }

        /// <inheritdoc/>
        public async Task<ImmutableArray<IDocument>> ExecuteModulesAsync(IEnumerable<IModule> modules, IEnumerable<IDocument> inputs)
        {
            if (Engine is object)
            {
                await Events.RaiseAsync(new BeforeEngineExecution(Engine, ExecutionId));
            }

            if (modules is null)
            {
                if (Engine is object)
                {
                    await Events.RaiseAsync(new AfterEngineExecution(Engine, ExecutionId, Outputs, 0));
                }
                return ImmutableArray<IDocument>.Empty;
            }

            foreach (IModule module in modules)
            {
                // We need a new context for each module so just do a member-wise clone of this one and set module and documents
                TestExecutionContext moduleContext = (TestExecutionContext)MemberwiseClone();
                moduleContext.SetInputs(inputs);
                moduleContext.Module = module;
                moduleContext.Parent = this;
                inputs = await module.ExecuteAsync(moduleContext);
            }

            if (Engine is object)
            {
                await Events.RaiseAsync(new AfterEngineExecution(Engine, ExecutionId, Outputs, 0));
            }
            return inputs?.Where(x => x is object).ToImmutableArray() ?? ImmutableArray<IDocument>.Empty;
        }

        /// <inheritdoc/>
        public IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null) =>
            Engine.GetJavaScriptEnginePool(initializer, startEngines, maxEngines, maxUsagesPerEngine, engineTimeout);

        public Func<IJavaScriptEngine> JsEngineFunc
        {
            get => Engine.JsEngineFunc;
            set => Engine.JsEngineFunc = value;
        }

        /// <inheritdoc />
        public void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            _documentFactory.SetDefaultDocumentType<TDocument>();

        // IDocumentFactory

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

        // IServiceProvider

        public object GetService(Type serviceType) => Services.GetService(serviceType);

        // ILogger

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            _logger.Log(logLevel, eventId, state, exception, formatter);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);
    }
}