using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Outputs metadata for information from any API with its client.
    /// </summary>
    /// <remarks>
    /// This modules used to submit requests to the API with the API client. Because of the possible large
    /// number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the API. Instead, it simplifies the housekeeping involved in setting up an
    /// API client and requires you to provide functions that fetch whatever data you need. Each request
    /// will be sent for each input document.
    /// </remarks>
    /// <category name="Metadata" />
    public class ReadApi<TClient> : ParallelModule, IDisposable
        where TClient : class
    {
        private readonly List<Func<IDocument, IExecutionContext, TClient, Task<IEnumerable<KeyValuePair<string, object>>>>> _requests
            = new List<Func<IDocument, IExecutionContext, TClient, Task<IEnumerable<KeyValuePair<string, object>>>>>();

        private readonly TClient _client;

        private readonly Config<TClient> _clientFactory;

        // Controls client disposal, but only for clients from the factory. Otherwise, if _client is set,
        // and is disposable, it'll be disposed regardless.
        private readonly bool _shouldDisposeFactoryClient = false;

        private readonly string _clientName;

        private Action<IDocument, IExecutionContext, TClient> _init;

        private SemaphoreSlim _throttler;

        private int _requestDelay;

        /// <summary>
        /// Creates a connection to the API using the client.
        /// </summary>
        /// <param name="client">The API client to use.</param>
        /// <param name="clientName">The client name ("API" by default).</param>
        public ReadApi(TClient client, string clientName = "API")
        {
            _client = client.ThrowIfNull(nameof(client));
            _clientName = clientName.ThrowIfNullOrWhiteSpace(nameof(clientName));
        }

        /// <summary>
        /// Creates a connection to the API using the client factory.
        /// </summary>
        /// <param name="clientFactory">The API client factory to use which will be called for each document.</param>
        /// <param name="clientName">The client name ("API" by default).</param>
        public ReadApi(Config<TClient> clientFactory, string clientName = "API")
        {
            _clientFactory = clientFactory.ThrowIfNull(nameof(clientFactory));
            _shouldDisposeFactoryClient = true;
            _clientName = clientName.ThrowIfNullOrWhiteSpace(nameof(clientName));
        }

        /// <summary>
        /// Creates a connection to the API using the client factory.
        /// </summary>
        /// <param name="clientFactory">The API client factory to use which will be called for each document.</param>
        /// <param name="shouldDisposeClient"><c>true</c> to dispose the client returned by the factory after each document.</param>
        /// <param name="clientName">The client name ("API" by default).</param>
        public ReadApi(Config<TClient> clientFactory, bool shouldDisposeClient, string clientName = "API")
        {
            _clientFactory = clientFactory.ThrowIfNull(nameof(clientFactory));
            _shouldDisposeFactoryClient = shouldDisposeClient;
            _clientName = clientName.ThrowIfNullOrWhiteSpace(nameof(clientName));
        }

        /// <summary>
        /// Dispose API client resources.
        /// </summary>
        public void Dispose()
        {
            if (_client is IDisposable disposableClient)
            {
                disposableClient.Dispose();
            }
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithClientInitialization(Action<TClient> init)
        {
            init.ThrowIfNull(nameof(init));

            _init = (_, __, client) => init(client);
            return this;
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithClientInitialization(Action<IExecutionContext, TClient> init)
        {
            init.ThrowIfNull(nameof(init));

            _init = (_, ctx, client) => init(ctx, client);
            return this;
        }

        /// <summary>
        /// Initialize the API client.
        /// </summary>
        /// <param name="init">An API client initialization action.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithClientInitialization(Action<IDocument, IExecutionContext, TClient> init)
        {
            _init = init.ThrowIfNull(nameof(init));
            return this;
        }

        /// <summary>
        /// Submits the limit of executing requests.
        /// </summary>
        /// <param name="requestLimit">The limit of executing requests.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequestLimit(int requestLimit)
        {
            if (requestLimit > 0)
            {
                _throttler = new SemaphoreSlim(requestLimit);
            }
            return this;
        }

        /// <summary>
        /// Submits the requests delay for each request in the current instance of module.
        /// </summary>
        /// <param name="requestDelay">The requests delay in milliseconds.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequestDelay(int requestDelay)
        {
            _requestDelay = requestDelay;
            return this;
        }

        /// <summary>
        /// Submits a request to the API client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(string key, Func<IExecutionContext, TClient, object> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests.Add((_, ctx, client) =>
                Task.FromResult<IEnumerable<KeyValuePair<string, object>>>(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>(key, request(ctx, client))
                    }));
            return this;
        }

        /// <summary>
        /// Submits a request to the API. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(string key, Func<IDocument, IExecutionContext, TClient, object> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests.Add((doc, ctx, client) =>
                Task.FromResult<IEnumerable<KeyValuePair<string, object>>>(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>(key, request(doc, ctx, client))
                    }));
            return this;
        }

        /// <summary>
        /// Submits a request to the API client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(string key, Func<IExecutionContext, TClient, Task<object>> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests.Add(async (_, ctx, client) =>
                new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>(key, await request(ctx, client))
                });
            return this;
        }

        /// <summary>
        /// Submits a request to the API. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(string key, Func<IDocument, IExecutionContext, TClient, Task<object>> request)
        {
            key.ThrowIfNullOrWhiteSpace(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests.Add(async (doc, ctx, client) =>
                new KeyValuePair<string, object>[]
                {
                    new KeyValuePair<string, object>(key, await request(doc, ctx, client))
                });
            return this;
        }

        /// <summary>
        /// Submits a request to the API client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(Func<IExecutionContext, TClient, IEnumerable<KeyValuePair<string, object>>> request)
        {
            request.ThrowIfNull(nameof(request));

            _requests.Add((_, ctx, client) => Task.FromResult(request(ctx, client)));
            return this;
        }

        /// <summary>
        /// Submits a request to the API. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(Func<IDocument, IExecutionContext, TClient, IEnumerable<KeyValuePair<string, object>>> request)
        {
            request.ThrowIfNull(nameof(request));

            _requests.Add((doc, ctx, client) => Task.FromResult(request(doc, ctx, client)));
            return this;
        }

        /// <summary>
        /// Submits a request to the API client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(Func<IExecutionContext, TClient, Task<IEnumerable<KeyValuePair<string, object>>>> request)
        {
            request.ThrowIfNull(nameof(request));

            _requests.Add(async (_, ctx, client) => await request(ctx, client));
            return this;
        }

        /// <summary>
        /// Submits a request to the API. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadApi<TClient> WithRequest(Func<IDocument, IExecutionContext, TClient, Task<IEnumerable<KeyValuePair<string, object>>>> request)
        {
            request.ThrowIfNull(nameof(request));

            _requests.Add(request);
            return this;
        }

        /// <inheritdoc/>
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            // Get the client
            TClient client = _client ?? await _clientFactory.GetValueAsync(input, context);
            if (client is null)
            {
                return input.Yield();
            }

            try
            {
                // Run initialization
                _init?.Invoke(input, context, client);

                // Get tasks for each request so they can be executed asynchronously
                IEnumerable<Task<IEnumerable<KeyValuePair<string, object>>>> requestTasks = _requests.Select(async request =>
                {
                    // Wait for the throttler
                    if (_throttler is object)
                    {
                        await _throttler.WaitAsync();
                    }

                    // Get a task to execute the request with a delay after
                    Task<IEnumerable<KeyValuePair<string, object>>> requestTask = ExecuteRequestAsync(request, input, context, client);
                    _ = requestTask.ContinueWith(
                        async _ =>
                        {
                            if (_requestDelay > 0)
                            {
                                await Task.Delay(_requestDelay);
                            }
                            _throttler?.Release();
                        },
                        TaskScheduler.Current);

                    // Execute the request and return the result (null results will be filtered out)
                    return await requestTask;
                });

                // Execute the requests
                IEnumerable<KeyValuePair<string, object>>[] results = await Task.WhenAll(requestTasks);

                // Eliminate null results and clone the document if any are left
                KeyValuePair<string, object>[] metadata = results.Where(x => x is object).SelectMany(x => x).ToArray();
                return results.Length > 0 ? context.CloneOrCreateDocument(input, metadata).Yield() : input.Yield();
            }
            finally
            {
                // Dispose the factory client if requested
                if (_shouldDisposeFactoryClient && client is IDisposable disposableClient)
                {
                    disposableClient.Dispose();
                }
            }
        }

        /// <summary>
        /// This should be overridden in derived classes to provide preparation for each request if needed.
        /// </summary>
        protected virtual async Task<IEnumerable<KeyValuePair<string, object>>> ExecuteRequestAsync(
            Func<IDocument, IExecutionContext, TClient, Task<IEnumerable<KeyValuePair<string, object>>>> request,
            IDocument input,
            IExecutionContext context,
            TClient client)
        {
            try
            {
                return await request(input, context, client);
            }
            catch (Exception ex)
            {
                context.LogWarning($"Exception while submitting {_clientName} request for {input.ToSafeDisplayString()}: {ex}");
                return default;
            }
        }
    }
}