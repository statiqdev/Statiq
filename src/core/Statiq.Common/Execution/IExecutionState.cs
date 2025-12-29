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
    /// <summary>
    /// A read-only interface to the <see cref="IEngine"/> used during an execution.
    /// </summary>
    public interface IExecutionState : IDocumentFactory
    {
        private static readonly AsyncLocal<IExecutionState> _current = new AsyncLocal<IExecutionState>();
        private static readonly AsyncLocal<IExecutionContext> _currentEmptyExecutionContext = new AsyncLocal<IExecutionContext>();

        /// <summary>
        /// The current execution state (which is the <see cref="IExecutionContext"/> if there is one, and usually the <see cref="IEngine"/> if not).
        /// </summary>
        public static IExecutionState Current
        {
            get
            {
                IExecutionContext context = IExecutionContext.Current;

                // If we got back the current empty execution context, then return the current execution state instead
                return context == _currentEmptyExecutionContext.Value ? _current.Value : context;
            }

            internal set
            {
                _current.Value = value;
                _currentEmptyExecutionContext.Value = new EmptyExecutionContext(value);
            }
        }

        internal static IExecutionContext CurrentEmptyExecutionContext =>
            _currentEmptyExecutionContext.Value ?? throw new ExecutionException("Could not get current execution state");

        /// <summary>
        /// Uniquely identifies the current execution cycle. This can be used to initialize and/or
        /// reset static data for a module on new generations (I.e., due to watching).
        /// For example, cache data could be cleared when this changes between runs.
        /// </summary>
        Guid ExecutionId { get; }

        /// <summary>
        /// Gets a cancellation token that will be canceled when processing should stop.
        /// </summary>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// The date/time that the current execution started.
        /// </summary>
        DateTime ExecutionDateTime { get; }

        /// <summary>
        /// Gets the state of the application when it was run.
        /// </summary>
        IApplicationState ApplicationState { get; }

        /// <summary>
        /// A catalog of all classes in all assemblies loaded by the current application context.
        /// </summary>
        ClassCatalog ClassCatalog { get; }

        /// <summary>
        ///  Indicates that the engine is executing pipeline phases and modules in serial.
        /// </summary>
        bool SerialExecution { get; }

        /// <summary>
        /// Gets global events and event handlers.
        /// </summary>
        IReadOnlyEventCollection Events { get; }

        /// <summary>
        /// Gets the current file system.
        /// </summary>
        IReadOnlyFileSystem FileSystem { get; }

        /// <summary>
        /// The application configuration as metadata.
        /// </summary>
        IReadOnlySettings Settings { get; }

        /// <summary>
        /// Gets the available shortcodes.
        /// </summary>
        IReadOnlyShortcodeCollection Shortcodes { get; }

        /// <summary>
        /// Gets a set of namespaces that should be brought into scope for modules that perform dynamic compilation.
        /// </summary>
        INamespacesCollection Namespaces { get; }

        /// <summary>
        /// Provides pooled memory streams (via the RecyclableMemoryStream library).
        /// </summary>
        IMemoryStreamFactory MemoryStreamFactory { get; }

        /// <summary>
        /// Gets the collection of output documents from all previously processed pipelines.
        /// </summary>
        IPipelineOutputs Outputs { get; }

        /// <summary>
        /// Gets the collection of output documents from all previously processed pipelines,
        /// filtered to destination paths with a <see cref="Keys.PageFileExtensions"/>
        /// extension (which defaults to ".htm" and ".html").
        /// </summary>
        FilteredDocumentList<IDocument> OutputPages { get; }

        /// <summary>
        /// Gets the dependency injection service provider.
        /// </summary>
        IServiceProvider Services { get; }

        ILogger Logger { get; }

        /// <summary>
        /// Gets a helper that can compile and evaluate C# scripts.
        /// </summary>
        IScriptHelper ScriptHelper { get; }

        /// <summary>
        /// Gets the pipelines.
        /// </summary>
        IReadOnlyPipelineCollection Pipelines { get; }

        /// <summary>
        /// The pipelines currently being executed.
        /// </summary>
        IReadOnlyPipelineCollection ExecutingPipelines { get; }

        /// <summary>
        /// Helps generate normalized links.
        /// </summary>
        ILinkGenerator LinkGenerator { get; }

        /// <summary>
        /// Gets a <see cref="Stream"/> that can be used for document content. If <paramref name="content"/>
        /// is not null, the stream is initialized with the specified content. It is preferred to use
        /// this method to obtain a stream over creating your own if the source of the content does
        /// not already provide one. The returned streams are optimized for memory usage and performance.
        /// Instances of the returned stream should be disposed when writing is complete.
        /// </summary>
        /// <remarks>The position is set to the beginning of the stream when returned.</remarks>
        /// <param name="content">Content to initialize the stream with.</param>
        /// <returns>A stream for document content.</returns>
        Stream GetContentStream(string content = null);

        /// <summary>
        /// Creates a <see cref="HttpClient"/> instance that should be used for all HTTP communication.
        /// </summary>
        /// <returns>A new <see cref="HttpClient"/> instance.</returns>
        HttpClient CreateHttpClient();

        /// <summary>
        /// Creates a new <see cref="HttpClient"/> instance that uses a custom message handler.
        /// </summary>
        /// <param name="handler">The message handler to use for this client.</param>
        /// <returns>A new <see cref="HttpClient"/> instance.</returns>
        HttpClient CreateHttpClient(HttpMessageHandler handler);

        /// <summary>
        /// Sends an <see cref="HttpRequestMessage"/> with exponential back-off.
        /// </summary>
        /// <param name="requestFactory">A factory that creates the request message to send (a fresh message is needed for each request).</param>
        /// <returns>The response.</returns>
        Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory);

        /// <summary>
        /// Sends an <see cref="HttpRequestMessage"/> with exponential back-off.
        /// </summary>
        /// <param name="requestFactory">A factory that creates the request message to send (a fresh message is needed for each request).</param>
        /// <param name="retryCount">The number of times to retry.</param>
        /// <returns>The response.</returns>
        Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(Func<HttpRequestMessage> requestFactory, int retryCount);

        /// <summary>
        /// Gets a new <see cref="IJavaScriptEnginePool"/>. The returned engine pool should be disposed
        /// when no longer needed.
        /// </summary>
        /// <param name="initializer">
        /// The code to run when a new engine is created. This should configure
        /// the environment and set up any required JavaScript libraries.
        /// </param>
        /// <param name="startEngines">The number of engines to initially start when a pool is created.</param>
        /// <param name="maxEngines">The maximum number of engines that will be created in the pool.</param>
        /// <param name="maxUsagesPerEngine">The maximum number of times an engine can be reused before it is disposed.</param>
        /// <param name="engineTimeout">
        /// The default timeout to use when acquiring an engine from the pool (defaults to 5 seconds).
        /// If an engine can not be acquired in this time frame, an exception will be thrown.
        /// </param>
        /// <returns>A new JavaScript engine pool.</returns>
        IJavaScriptEnginePool GetJavaScriptEnginePool(
            Action<IJavaScriptEngine> initializer = null,
            int startEngines = 10,
            int maxEngines = 25,
            int maxUsagesPerEngine = 100,
            TimeSpan? engineTimeout = null);
    }
}