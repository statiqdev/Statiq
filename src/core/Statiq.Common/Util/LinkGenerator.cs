using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// Helps generate normalized links.
    /// </summary>
    /// <remarks>
    /// Override this class and register your implementation to use alternate link generation logic.
    /// </remarks>
    public class LinkGenerator : ILinkGenerator
    {
        // Cache link generator results
        private static readonly ConcurrentCache<GetLinkCacheKey, string> _links =
            new ConcurrentCache<GetLinkCacheKey, string>(false);

        private class GetLinkCacheKey : IEquatable<GetLinkCacheKey>
        {
            public string Path { get; set; }

            public string Host { get; set; }

            public string Root { get; set; }

            public string Scheme { get; set; }

            public string[] HidePages { get; set; }

            public string[] HideExtensions { get; set; }

            public bool Lowercase { get; set; }

            public bool MakeAbsolute { get; set; }

            public bool HiddenPageTrailingSlash { get; set; }

            public bool Equals(GetLinkCacheKey other)
            {
                if (ReferenceEquals(null, other))
                {
                    return false;
                }

                if (ReferenceEquals(this, other))
                {
                    return true;
                }

                return Path == other.Path
                   && Host == other.Host
                   && Root == other.Root
                   && Scheme == other.Scheme
                   && Equals(HidePages, other.HidePages)
                   && Equals(HideExtensions, other.HideExtensions)
                   && Lowercase == other.Lowercase
                   && MakeAbsolute == other.MakeAbsolute
                   && HiddenPageTrailingSlash == other.HiddenPageTrailingSlash;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                if (ReferenceEquals(this, obj))
                {
                    return true;
                }

                if (obj.GetType() != this.GetType())
                {
                    return false;
                }

                return Equals((GetLinkCacheKey)obj);
            }

            public override int GetHashCode()
            {
                HashCode hashCode = default;
                hashCode.Add(Path);
                hashCode.Add(Host);
                hashCode.Add(Root);
                hashCode.Add(Scheme);
                hashCode.Add(HidePages);
                hashCode.Add(HideExtensions);
                hashCode.Add(Lowercase);
                hashCode.Add(MakeAbsolute);
                hashCode.Add(HiddenPageTrailingSlash);
                return hashCode.ToHashCode();
            }
        }

        /// <inheritdoc />
        public virtual string GetLink(
            string path,
            string host,
            string root,
            string scheme,
            string[] hidePages,
            string[] hideExtensions,
            bool lowercase,
            bool makeAbsolute,
            bool hiddenPageTrailingSlash) =>
            _links.GetOrAdd(
                new GetLinkCacheKey
                {
                    Path = path,
                    Host = host,
                    Root = root,
                    Scheme = scheme,
                    HidePages = hidePages,
                    HideExtensions = hideExtensions,
                    Lowercase = lowercase,
                    MakeAbsolute = makeAbsolute,
                    HiddenPageTrailingSlash = hiddenPageTrailingSlash
                },
                key => GetLinkImplementation(
                    key.Path,
                    key.Host,
                    key.Root,
                    key.Scheme,
                    key.HidePages,
                    key.HideExtensions,
                    key.Lowercase,
                    key.MakeAbsolute,
                    key.HiddenPageTrailingSlash));

        private static string GetLinkImplementation(
            string path,
            string host,
            string root,
            string scheme,
            string[] hidePages,
            string[] hideExtensions,
            bool lowercase,
            bool makeAbsolute,
            bool hiddenPageTrailingSlash)
        {
            if (path is object)
            {
                string fileName = Path.GetFileName(path);

                // Remove index pages
                if (hidePages is object && path != "/" && path != "\\"
                    && hidePages.Any(x => x is object && fileName.Equals(x)))
                {
                    path = Path.GetDirectoryName(path);

                    // Add a trailing slash if we hid the page and requested to do so
                    if (hiddenPageTrailingSlash
                        && !string.IsNullOrEmpty(path)
                        && !path.EndsWith("/")
                        && !path.EndsWith("\\"))
                    {
                        path += "/";
                    }

                    // We don't need to worry about hiding extensions if we crawled up
                    hideExtensions = null;
                }

                // Special case if this is relative and we removed the entire path
                if (!makeAbsolute && string.IsNullOrEmpty(path))
                {
                    // The "." indicates to a browser to link to the current directory, which is what we want
                    return ".";
                }

                // Hide extensions
                if (hideExtensions is object)
                {
                    string extension = Path.GetExtension(path);
                    if (!string.IsNullOrEmpty(extension)
                        && (hideExtensions.Length == 0
                        || hideExtensions
                            .Where(x => x is object)
                            .Select(x => x.StartsWith(".") ? x : "." + x)
                            .Contains(extension)))
                    {
                        path = Path.ChangeExtension(path, null);
                    }
                }

                // Prepend a slash, but only if we want to make the path absolute
                if (makeAbsolute)
                {
                    if (string.IsNullOrWhiteSpace(path) || path == ".")
                    {
                        path = "/";
                    }
                    if (!path.StartsWith("/") && !path.StartsWith("\\"))
                    {
                        path = "/" + path;
                    }
                }
            }
            else
            {
                path = string.Empty;
            }

            // Extract the fragment
            int fragmentIndex = path.IndexOf('#');
            string fragment = null;
            if (fragmentIndex > -1)
            {
                fragment = path.Substring(fragmentIndex);
                path = path.Substring(0, fragmentIndex);
            }

            // Extract the query
            int queryIndex = path.IndexOf('?');
            string query = null;
            if (queryIndex > -1)
            {
                query = path.Substring(queryIndex);
                path = path.Substring(0, queryIndex);
            }

            // If we're not making it absolute and it doesn't start with a slash, make sure to remove the slash when we're done,
            // otherwise collapse with the root path and combine them
            bool makeRelative = false;
            if (!makeAbsolute && (path.Length == 0 || (path[0] != '/' && path[0] != '\\')))
            {
                makeRelative = true;
            }
            else
            {
                string rootLink = root ?? string.Empty;
                if (rootLink.EndsWith("/") || rootLink.EndsWith("\\"))
                {
                    rootLink = rootLink.Substring(0, rootLink.Length - 1);
                }
                path = rootLink + path;
            }

            // Add the host and convert to URI for escaping
            UriBuilder builder = new UriBuilder
            {
                Scheme = scheme ?? "http",
                Path = path,
                Query = query,
                Fragment = fragment
            };
            bool hasHost = false;
            if (!makeRelative && !string.IsNullOrWhiteSpace(host))
            {
                builder.Host = host;
                hasHost = true;
            }
            Uri uri = builder.Uri;
            string renderedLink = hasHost
                ? uri.AbsoluteUri
                : uri.GetComponents(UriComponents.Path | UriComponents.Query | UriComponents.Fragment, UriFormat.SafeUnescaped);

            // Remove the slash prefix if we have one
            if (makeRelative && renderedLink[0] == '/')
            {
                renderedLink = renderedLink.Substring(1);

                // If the link started with a dot, add it back in
                if (path.Length > 0 && path[0] == '.' && (renderedLink.Length == 0 || renderedLink[0] != '.'))
                {
                    renderedLink = "." + renderedLink;
                }
            }

            return lowercase ? renderedLink.ToLowerInvariant() : renderedLink;
        }

        /// <inheritdoc />
        public virtual bool TryGetAbsoluteHttpUri(string str, out string absoluteUri)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                // Return the actual URI if it's absolute
                if (Uri.TryCreate(str, UriKind.Absolute, out Uri uri)
                    && (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
                        || uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
                {
                    absoluteUri = uri.ToString();
                    return true;
                }
            }
            absoluteUri = null;
            return false;
        }

        /// <inheritdoc />
        public virtual string AddQueryAndFragment(string path, string queryAndFragment)
        {
            if (!string.IsNullOrEmpty(queryAndFragment))
            {
                // If we have a query and fragment, make sure it starts with ? or #
                if (queryAndFragment[0] == '?' || queryAndFragment[0] == '#')
                {
                    return path + queryAndFragment;
                }
                return path + "?" + queryAndFragment;
            }
            return path;
        }
    }
}