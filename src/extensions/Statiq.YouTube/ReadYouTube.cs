using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.YouTube
{
    /// <summary>
    /// Outputs metadata for information from YouTube.
    /// </summary>
    /// <remarks>
    /// This modules uses the Google.Apis.YouTube.v3 library and associated types to submit requests to GitHub. Because
    /// of the large number of different kinds of requests, this module does not attempt to provide a fully abstract wrapper
    /// around the Google.Apis.YouTube.v3 library. Instead, it simplifies the housekeeping involved in setting up an
    /// Google.Apis.YouTube.v3 client and requires you to provide functions that fetch whatever data you need. Each request
    /// will be sent for each input document.
    /// </remarks>
    /// <category name="Metadata" />
    public class ReadYouTube : ParallelSyncModule, IDisposable
    {
        private readonly YouTubeService _youtube;

        private readonly Dictionary<string, Func<IDocument, IExecutionContext, YouTubeService, object>> _requests
            = new Dictionary<string, Func<IDocument, IExecutionContext, YouTubeService, object>>();

        /// <summary>
        /// Creates a connection to the YouTube API with authenticated access.
        /// </summary>
        /// <param name="apiKey">The API key to use.</param>
        public ReadYouTube(string apiKey)
        {
            _youtube = new YouTubeService(
                new BaseClientService.Initializer
                {
                    ApplicationName = "Statiq",
                    ApiKey = apiKey
                });
        }

        public void Dispose() => _youtube.Dispose();

        /// <summary>
        /// Submits a request to the YouTube client. This allows you to incorporate data from the execution context in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadYouTube WithRequest(string key, Func<IExecutionContext, YouTubeService, object> request)
        {
            key.ThrowIfNullOrEmpty(nameof(key));
            request.ThrowIfNull(nameof(request));

            _requests[key] = (doc, ctx, github) => request(ctx, github);
            return this;
        }

        /// <summary>
        /// Submits a request to the YouTube client. This allows you to incorporate data from the execution context and current document in your request.
        /// </summary>
        /// <param name="key">The metadata key in which to store the return value of the request function.</param>
        /// <param name="request">A function with the request to make.</param>
        /// <returns>The current module instance.</returns>
        public ReadYouTube WithRequest(string key, Func<IDocument, IExecutionContext, YouTubeService, object> request)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Argument is null or empty", nameof(key));
            }

            _requests[key] = request.ThrowIfNull(nameof(request));
            return this;
        }

        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            ConcurrentDictionary<string, object> results = new ConcurrentDictionary<string, object>();
            System.Threading.Tasks.Parallel.ForEach(_requests, request =>
            {
                context.LogDebug("Submitting {0} YouTube request for {1}", request.Key, input.ToSafeDisplayString());
                try
                {
                    results[request.Key] = request.Value(input, context, _youtube);
                }
                catch (Exception ex)
                {
                    context.LogWarning("Exception while submitting {0} YouTube request for {1}: {2}", request.Key, input.ToSafeDisplayString(), ex.ToString());
                }
            });
            return input.Clone(results).Yield();
        }
    }
}