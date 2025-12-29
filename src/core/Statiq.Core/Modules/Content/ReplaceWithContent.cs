using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces a search string in the specified content with the content of input documents.
    /// </summary>
    /// <remarks>
    /// This is sort of like the inverse of the Replace module and is very useful for simple
    /// template substitution.
    /// </remarks>
    /// <category name="Content" />
    public class ReplaceWithContent : ParallelConfigModule<string>
    {
        private readonly string _search;
        private bool _isRegex;
        private RegexOptions _regexOptions = RegexOptions.None;

        /// <summary>
        /// Replaces all occurrences of the search string in the string value of the returned
        /// object with the content of each input document. This allows you to specify different
        /// content for each document depending on the input document.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">A delegate that returns the content within which
        /// to search for the search string.</param>
        public ReplaceWithContent(string search, Config<string> content)
            : base(content, true)
        {
            _search = search;
        }

        /// <summary>
        /// Indicates that the search string(s) should be treated as a regular expression(s)
        /// with the specified options.
        /// </summary>
        /// <param name="regexOptions">The options to use (if any).</param>
        /// <returns>The current module instance.</returns>
        public ReplaceWithContent IsRegex(RegexOptions regexOptions = RegexOptions.None)
        {
            _isRegex = true;
            _regexOptions = regexOptions;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, string value)
        {
            if (value is null)
            {
                value = string.Empty;
            }
            if (string.IsNullOrEmpty(_search))
            {
                return input.Clone(context.GetContentProvider(value, input.ContentProvider.MediaType)).Yield();
            }
            string inputContent = await input.GetContentStringAsync();
            string replaced = _isRegex
                ? Regex.Replace(inputContent, _search, value, _regexOptions)
                : value.Replace(_search, inputContent);
            return input.Clone(context.GetContentProvider(replaced, input.ContentProvider.MediaType)).Yield();
        }
    }
}