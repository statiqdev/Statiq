using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces a search string in the content of each input document with new content.
    /// </summary>
    /// <category name="Content" />
    public class ReplaceInContent : ParallelConfigModule<string>
    {
        private readonly string _search;
        private readonly Func<Match, IDocument, string> _contentFinder;
        private bool _isRegex;
        private RegexOptions _regexOptions = RegexOptions.None;

        /// <summary>
        /// Replaces all occurrences of the search string in every input document with the
        /// string value of the returned object. This allows you to specify different content
        /// for each document depending on the input document.
        /// </summary>
        /// <param name="search">The string to search for.</param>
        /// <param name="content">A delegate that returns the content to replace the search string with.</param>
        public ReplaceInContent(string search, Config<string> content)
            : base(content, true)
        {
            _search = search;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in every input document
        /// with the string value of the objects returned by the delegate. The delegate will be called
        /// for each Match in the supplied regular expression.
        /// </summary>
        /// <param name="search">The string to search for (interpreted as a regular expression).</param>
        /// <param name="contentFinder">A delegate that returns the content to replace the match.</param>
        public ReplaceInContent(string search, Func<Match, string> contentFinder)
            : base(null as string, true)
        {
            _search = search;
            _contentFinder = (match, _) => contentFinder(match);
            _isRegex = true;
        }

        /// <summary>
        /// Replaces all occurrences of the search string in every input document
        /// with the string value of the objects returned by the delegate. The delegate will be called
        /// for each Match in the supplied regular expression.
        /// </summary>
        /// <param name="search">The string to search for (interpreted as a regular expression).</param>
        /// <param name="contentFinder">A delegate that returns the content to replace the match.</param>
        public ReplaceInContent(string search, Func<Match, IDocument, string> contentFinder)
            : base(null as string, true)
        {
            _search = search;
            _contentFinder = contentFinder;
            _isRegex = true;
        }

        /// <summary>
        /// Indicates that the search string(s) should be treated as a regular expression(s)
        /// with the specified options.
        /// </summary>
        /// <param name="regexOptions">The options to use (if any).</param>
        /// <returns>The current module instance.</returns>
        public ReplaceInContent IsRegex(RegexOptions regexOptions = RegexOptions.None)
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
                return input.Yield();
            }
            string currentDocumentContent = await input.GetContentStringAsync();
            if (_contentFinder is object)
            {
                string newDocumentContent = Regex.Replace(
                    currentDocumentContent,
                    _search,
                    match => _contentFinder(match, input)?.ToString() ?? string.Empty,
                    _regexOptions);
                return (currentDocumentContent == newDocumentContent
                    ? input
                    : input.Clone(context.GetContentProvider(newDocumentContent, input.ContentProvider.MediaType)))
                    .Yield();
            }
            string replaced = _isRegex
                ? Regex.Replace(currentDocumentContent, _search, value, _regexOptions)
                : currentDocumentContent.Replace(_search, value);
            return input.Clone(context.GetContentProvider(replaced, input.ContentProvider.MediaType)).Yield();
        }
    }
}