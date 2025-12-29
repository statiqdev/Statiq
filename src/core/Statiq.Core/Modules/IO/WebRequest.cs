using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// A download request for use with the <see cref="ReadWeb"/> module.
    /// </summary>
    public class WebRequest
    {
        /// <summary>
        /// The URI to download from.
        /// </summary>
        public Uri Uri { get; }

        /// <summary>
        /// Request headers.
        /// </summary>
        public WebRequestHeaders Headers { get; set; }

        /// <summary>
        /// The query string parameters. These will be combined with any that already exist in <see cref="Uri"/>.
        /// </summary>
        public IDictionary<string, string> QueryString { get; } = new Dictionary<string, string>();

        /// <summary>
        /// The method to use.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// The content of the request (has no effect for some methods like GET).
        /// </summary>
        public HttpContent Content { get; set; }

        /// <summary>
        /// The network credentials to use for the request.
        /// </summary>
        public NetworkCredential Credentials { get; set; }

        /// <summary>
        /// Creates a new download request.
        /// </summary>
        /// <param name="uri">The URI to download from.</param>
        public WebRequest(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(uri));
            }

            Uri = new Uri(uri);
        }

        /// <summary>
        /// Creates a new download request.
        /// </summary>
        /// <param name="uri">The URI to download from.</param>
        public WebRequest(Uri uri) =>
            Uri = uri.ThrowIfNull(nameof(uri));

        /// <summary>
        /// Sets the request headers.
        /// </summary>
        /// <param name="headers">The request headers to set.</param>
        /// <returns>The current instance.</returns>
        public WebRequest WithHeaders(WebRequestHeaders headers)
        {
            Headers = headers.ThrowIfNull(nameof(headers));
            return this;
        }

        /// <summary>
        /// Sets a query string value.
        /// </summary>
        /// <param name="name">The name of the query string parameter.</param>
        /// <param name="value">The value of the query string parameter.</param>
        /// <returns>The current instance.</returns>
        public WebRequest WithQueryString(string name, string value)
        {
            name.ThrowIfNull(nameof(name));
            QueryString[name] = value;
            return this;
        }

        /// <summary>
        /// Sets the request method.
        /// </summary>
        /// <param name="method">The method to set.</param>
        /// <returns>The current instance.</returns>
        public WebRequest WithMethod(HttpMethod method)
        {
            Method = method.ThrowIfNull(nameof(method));
            return this;
        }

        /// <summary>
        /// Sets the content of the request (only applicable to some request methods).
        /// </summary>
        /// <param name="content">The content to set.</param>
        /// <returns>The current instance.</returns>
        public WebRequest WithContent(HttpContent content)
        {
            Content = content;
            return this;
        }

        /// <summary>
        /// Sets the string content of the request (only applicable to some request methods).
        /// </summary>
        /// <param name="content">The content to set.</param>
        /// <returns>The current instance.</returns>
        public WebRequest WithContent(string content)
        {
            content.ThrowIfNull(nameof(content));
            Content = new System.Net.Http.StringContent(content);
            return this;
        }

        /// <summary>
        /// Sets the credentials to use for the request.
        /// </summary>
        /// <param name="credentials">The credentials to use.</param>
        /// <returns>The current instance.</returns>
        public WebRequest WithCredentials(NetworkCredential credentials)
        {
            Credentials = credentials.ThrowIfNull(nameof(credentials));
            return this;
        }

        /// <summary>
        /// Sets the credentials to use for the request.
        /// </summary>
        /// <param name="userName">The username to use.</param>
        /// <param name="password">The password to use.</param>
        /// <returns>The current instance.</returns>
        public WebRequest WithCredentials(string userName, string password)
        {
            Credentials = new NetworkCredential(userName, password);
            return this;
        }
    }
}