using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Provides ways to work with relative URLs.
    /// </summary>
    public readonly struct RelativeUrl
    {
        /// <summary>
        /// <c>true</c> if the URL starts with ~/, otherwise <c>false</c>.
        /// </summary>
        public bool HasRoot { get; }

        /// <summary>
        /// The root to use if the URL starts with a ~/.
        /// </summary>
        public string Root { get; }

        /// <summary>
        /// The URL path.
        /// </summary>
        public NormalizedPath Path { get; }

        /// <summary>
        /// The URL query.
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// The URL fragment.
        /// </summary>
        public string Fragment { get; }

        /// <summary>
        /// Creates an instance of RelativeUrl using the given url string.
        /// </summary>
        /// <param name="url">The URL string.</param>
        /// <param name="root">The root to use if the URL starts with ~/.</param>
        public RelativeUrl(string url, string root = "")
        {
            if (!IsRelative(url))
            {
                throw new InvalidOperationException($"Absolute URL '{url}' cannot be used for a RelativeUrl.");
            }

            if (url == null)
            {
                HasRoot = false;
                Path = NormalizedPath.Null;
                Query = string.Empty;
                Fragment = string.Empty;
                Root = root;

                return;
            }

            Root = root;

            HasRoot = url.StartsWith("~/");
            if (HasRoot)
            {
                url = url.Substring(1);
            }

            int fragmentIndex = url.IndexOf('#');
            if (fragmentIndex > -1)
            {
                Fragment = url.Substring(fragmentIndex);
                url = url.Substring(0, fragmentIndex);
            }
            else
            {
                Fragment = string.Empty;
            }

            int queryIndex = url.IndexOf('?');
            if (queryIndex > -1)
            {
                Query = url.Substring(queryIndex);
                url = url.Substring(0, queryIndex);
            }
            else
            {
                Query = string.Empty;
            }

            Path = new NormalizedPath(url);
        }

        /// <summary>
        /// Determines if the given <paramref name="url"/> is relative.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns><c>true</c> if the URL is relative, otherwise <c>false</c>.</returns>
        public static bool IsRelative(string url)
        {
            return url == null ? true : url.IndexOf(':') == -1;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder url = new StringBuilder();

            NormalizedPath path = Path;

            if (HasRoot)
            {
                path = new NormalizedPath($"/{Root}/{path}");
            }

            url.Append(path);

            if (!string.IsNullOrEmpty(Query))
            {
                url.Append(Query);
            }

            if (!string.IsNullOrEmpty(Fragment))
            {
                url.Append(Fragment);
            }

            return url.ToString();
        }

        /// <inheritdoc/>
        public static implicit operator string(RelativeUrl url)
        {
            return url.ToString();
        }
    }
}
