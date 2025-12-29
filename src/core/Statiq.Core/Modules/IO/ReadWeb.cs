using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Downloads from HTTP and outputs the results as new documents.
    /// </summary>
    /// <remarks>
    /// The original input documents are ignored and are not part of the output
    /// of this module.
    /// </remarks>
    /// <metadata cref="Keys.SourceUri" usage="Output" />
    /// <metadata cref="Keys.SourceHeaders" usage="Output" />
    /// <category name="Input/Output" />
    public class ReadWeb : Module
    {
        private readonly List<WebRequest> _requests = new List<WebRequest>();
        private List<WebResponse> _cachedResponses;
        private bool _cacheResponses;

        /// <summary>
        /// Downloads the specified URIs with a default request header.
        /// </summary>
        /// <param name="uris">The URIs to download.</param>
        public ReadWeb(params string[] uris)
        {
            WithUris(uris);
        }

        /// <summary>
        /// Downloads the specified URI with the specified request header.
        /// </summary>
        /// <param name="uri">The URI to download.</param>
        /// <param name="headers">The request header to use.</param>
        public ReadWeb(string uri, WebRequestHeaders headers)
        {
            WithUri(uri, headers);
        }

        /// <summary>
        /// Downloads the specified URIs with a default request header.
        /// </summary>
        /// <param name="uris">The URIs to download.</param>
        /// <returns>The current module instance.</returns>
        public ReadWeb WithUris(params string[] uris)
        {
            foreach (string uri in uris)
            {
                _requests.Add(new WebRequest(uri));
            }
            return this;
        }

        /// <summary>
        /// Downloads the specified URI with the specified request header.
        /// </summary>
        /// <param name="uri">The URI to download.</param>
        /// <param name="headers">The request header to use.</param>
        /// <returns>The current module instance.</returns>
        public ReadWeb WithUri(string uri, WebRequestHeaders headers = null)
        {
            _requests.Add(new WebRequest(uri)
            {
                Headers = headers
            });
            return this;
        }

        /// <summary>
        /// Downloads the specified requests.
        /// </summary>
        /// <param name="requests">The requests to download.</param>
        /// <returns>The current module instance.</returns>
        public ReadWeb WithRequests(params WebRequest[] requests)
        {
            requests.ThrowIfNull(nameof(requests));
            _requests.AddRange(requests.Where(x => x is object));
            return this;
        }

        /// <summary>
        /// Indicates whether the downloaded response should be cached between regenerations.
        /// </summary>
        /// <param name="cacheResponses">If set to <c>true</c>, the response is cached (the default is <c>false</c>).</param>
        /// <returns>The current module instance.</returns>
        public ReadWeb CacheResponses(bool cacheResponses = true)
        {
            _cacheResponses = cacheResponses;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            List<WebResponse> responses = _cachedResponses;
            if (responses is null)
            {
                responses =
                    (await _requests.ParallelSelectAsync(async x => await GetResponseAsync(x, context)))
                    .Where(x => x is object)
                    .ToList();
                if (_cacheResponses)
                {
                    _cachedResponses = responses;
                }
            }
            return responses.Select(response =>
                context.CreateDocument(
                    new MetadataItems
                    {
                        { Keys.SourceUri, response.Uri },
                        { Keys.SourceHeaders, response.Headers }
                    },
                    context.GetContentProvider(response.Stream, response.MediaType)));
        }

        private static readonly Dictionary<HttpMethod, Func<HttpClient, Uri, HttpContent, Task<HttpResponseMessage>>> MethodMapping =
            new Dictionary<HttpMethod, Func<HttpClient, Uri, HttpContent, Task<HttpResponseMessage>>>
            {
                { HttpMethod.Get, (client, uri, content) => client.GetAsync(uri) },
                { HttpMethod.Post, (client, uri, content) => client.PostAsync(uri, content) },
                { HttpMethod.Delete, (client, uri, content) => client.DeleteAsync(uri) },
                { HttpMethod.Put, (client, uri, content) => client.PutAsync(uri, content) }
            };

        private async Task<WebResponse> GetResponseAsync(WebRequest request, IExecutionContext context)
        {
            // Get the HTTP client
            using (HttpClientHandler clientHandler = new HttpClientHandler())
            {
                if (request.Credentials is object)
                {
                    clientHandler.Credentials = request.Credentials;
                }
                using (HttpClient client = context.CreateHttpClient(clientHandler))
                {
                    // Apply request headers
                    request.Headers?.ApplyTo(client.DefaultRequestHeaders);

                    // Apply the query string
                    Uri uri = ApplyQueryString(request.Uri, request.QueryString);

                    // Now that we are set and ready, go and do the download call
                    if (!MethodMapping.TryGetValue(
                        request.Method ?? HttpMethod.Get,
                        out Func<HttpClient, Uri, HttpContent, Task<HttpResponseMessage>> requestFunc))
                    {
                        context.LogError($"Invalid download method for {request.Uri}: {request.Method.Method}");
                        return null;
                    }
                    using (HttpResponseMessage response = await requestFunc(client, uri, request.Content))
                    {
                        using (HttpContent content = response.Content)
                        {
                            Stream result = await content.ReadAsStreamAsync();
                            MemoryStream memoryStream = context.MemoryStreamFactory.GetStream();
                            await result.CopyToAsync(memoryStream);
                            Dictionary<string, string> headers = content.Headers.ToDictionary(
                                x => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(x.Key), x => string.Join(",", x.Value));
                            return new WebResponse(request.Uri, memoryStream, response.Content.Headers.ContentType?.MediaType, headers);
                        }
                    }
                }
            }
        }

        private Uri ApplyQueryString(Uri uri, IDictionary<string, string> queryString)
        {
            UriBuilder builder = new UriBuilder(uri);
            if (queryString.Count > 0)
            {
                string query = builder.Query;
                if (string.IsNullOrEmpty(query))
                {
                    query = "?";
                }
                else
                {
                    query += "&";
                }
                query += string.Join(
                        "&",
                        queryString.Select(x => string.IsNullOrEmpty(x.Value)
                            ? WebUtility.UrlEncode(x.Key)
                            : $"{WebUtility.UrlEncode(x.Key)}={WebUtility.UrlEncode(x.Value)}"));
                builder.Query = query;
            }
            return builder.Uri;
        }

        private class WebResponse
        {
            public Uri Uri { get; }
            public Stream Stream { get; }
            public string MediaType { get; }
            public Dictionary<string, string> Headers { get; }

            public WebResponse(Uri uri, Stream stream, string mediaType, Dictionary<string, string> headers)
            {
                Uri = uri;
                Stream = stream;
                MediaType = mediaType;
                Headers = headers;
            }
        }
    }
}