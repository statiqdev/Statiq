using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public static class IReadOnlySettingsExtensions
    {
        private const string DefaultIndexFileName = "index.html";

        private static readonly string[] DefaultPageFileExtensions = new string[] { ".html", ".htm" };

        /// <summary>
        /// Gets a guaranteed index file name, returning a default of "index.html" if
        /// <see cref="Keys.IndexFileName"/> is not defined or is empty or whitespace.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The index file name.</returns>
        public static string GetIndexFileName(this IReadOnlySettings settings)
        {
            settings.ThrowIfNull(nameof(settings));
            string indexFileName = settings.GetString(Keys.IndexFileName);
            return indexFileName.IsNullOrWhiteSpace() ? DefaultIndexFileName : indexFileName;
        }

        /// <summary>
        /// Gets a guaranteed set of page file extensions (with a preceding "."), returning a default of ".html", ".htm"
        /// if <see cref="Keys.PageFileExtensions"/> is not defined or is empty.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns>The page file extensions.</returns>
        public static string[] GetPageFileExtensions(this IReadOnlySettings settings)
        {
            settings.ThrowIfNull(nameof(settings));
            IReadOnlyList<string> pageFileExtensions = settings.GetList<string>(Keys.PageFileExtensions);
            return pageFileExtensions is null || pageFileExtensions.Count == 0
                ? DefaultPageFileExtensions
                : pageFileExtensions.Select(x => x.StartsWith('.') ? x : "." + x).ToArray();
        }
    }
}