using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Helps generate normalized links.
    /// </summary>
    public interface ILinkGenerator
    {
        /// <summary>
        /// Generates a normalized link given a path and other conditions.
        /// </summary>
        /// <param name="path">The path to get a link for.</param>
        /// <param name="host">The host for the link (or <c>null</c> to omit the host).</param>
        /// <param name="root">The root path for the link (or <c>null</c> for no root path).</param>
        /// <param name="scheme">The scheme for the link (or <c>null</c> for "http").</param>
        /// <param name="hidePages">An array of file names to hide (or <c>null</c> to not hide any files).</param>
        /// <param name="hideExtensions">An array of file extensions to hide (or <c>null</c> to not hide extensions or an empty array to hide all file extensions).</param>
        /// <param name="lowercase">Indicates that the link should be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="scheme"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="scheme"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <param name="hiddenPageTrailingSlash">
        /// Indicates that a trailing slash should be appended when hiding a page due to <paramref name="hidePages" />.
        /// Setting to <c>false</c> means that hiding a page will result in the parent path without a trailing slash.
        /// </param>
        /// <returns>A generated link.</returns>
        string GetLink(
            string path,
            string host,
            string root,
            string scheme,
            string[] hidePages,
            string[] hideExtensions,
            bool lowercase,
            bool makeAbsolute,
            bool hiddenPageTrailingSlash);

        /// <summary>
        /// Checks if a string contains an absolute URI with a "http" or "https" scheme and returns it if it does.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <param name="absoluteUri">The resulting absolute URI.</param>
        /// <returns><c>true</c> if the string contains an absolute URI, <c>false</c> otherwise.</returns>
        bool TryGetAbsoluteHttpUri(string str, out string absoluteUri);

        /// <summary>
        /// Adds a query and/or fragment to a URL or path.
        /// </summary>
        /// <param name="path">The path or URL.</param>
        /// <param name="queryAndFragment">
        /// The query and/or fragment to add. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <returns>The path or URL with an appended query and/or fragment.</returns>
        string AddQueryAndFragment(string path, string queryAndFragment);
    }
}