using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Extracts the first part of content for each document and sends it to a child module for processing.
    /// </summary>
    /// <remarks>
    /// This module is typically used in conjunction with the Yaml module to enable putting YAML front
    /// matter in a file. First, the content of each input document is scanned for a line that consists
    /// entirely of the delimiter character or (- by default) or the delimiter string. Once found, the
    /// content before the delimiter is passed to the specified child modules. Any metadata from the child
    /// module output document(s) is added to the input document. Note that if the child modules result
    /// in more than one output document, multiple clones of the input document will be made for each one.
    /// The output document content is set to the original content without the front matter.
    /// </remarks>
    /// <category name="Control" />
    public class ExtractFrontMatter : ParentModule
    {
        // Cache compiled string-based regular expressions so we don't have to recompile them every time
        private static readonly ConcurrentCache<string, Regex> StringRegexCache =
            new ConcurrentCache<string, Regex>(false);

        private readonly Config<IEnumerable<Regex>> _regexes;

        // Explicit front matter definition
        private readonly string _endDelimiter;
        private readonly bool _endRepeated;
        private bool _ignoreEndDelimiterOnFirstLine = true;
        private string _startDelimiter;
        private bool _startRepeated;

        private bool _preserveFrontMatter;

        /// <summary>
        /// Uses the default delimiter character and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(params IModule[] modules)
            : base(modules)
        {
            _endDelimiter = "-";
            _endRepeated = true;
        }

        /// <summary>
        /// Identifies front matter using one or more regular expressions
        /// and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <remarks>
        /// If a group named "frontmatter" is returned from the regex, it will be used for front matter
        /// content. Otherwise, the first group will be used.
        /// The provided regular expressions are evaluated with both the
        /// <see cref="RegexOptions.Singleline"/> and <see cref="RegexOptions.Multiline"/> options.
        /// To use different options, supply <see cref="Regex"/> instances using the alternate constructor.
        /// </remarks>
        /// <param name="regexes">The regular expressions to use to find the front matter.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(Config<IEnumerable<string>> regexes, params IModule[] modules)
            : this(
                regexes.Transform(x =>
                    x.Select(r =>
                        StringRegexCache.GetOrAdd(
                            r,
                            k => new Regex(k, RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline)))),
                modules)
        {
        }

        /// <summary>
        /// Identifies front matter using one or more regular expressions
        /// and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <remarks>
        /// If a group named "frontmatter" is returned from the regex, it will be used for front matter
        /// content. Otherwise, the first group will be used.
        /// </remarks>
        /// <param name="regexes">The regular expressions to use to find the front matter.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(Config<IEnumerable<Regex>> regexes, params IModule[] modules)
            : base(modules)
        {
            _regexes = regexes.EnsureNonDocument(nameof(regexes));
        }

        /// <summary>
        /// Uses the specified delimiter string and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(string delimiter, params IModule[] modules)
            : base(modules)
        {
            _endDelimiter = delimiter;
            _endRepeated = false;
        }

        /// <summary>
        /// Uses the specified delimiter character and passes any front matter to the specified child modules for processing.
        /// </summary>
        /// <param name="delimiter">The delimiter to use.</param>
        /// <param name="modules">The modules to execute against the front matter.</param>
        public ExtractFrontMatter(char delimiter, params IModule[] modules)
            : base(modules)
        {
            _endDelimiter = new string(delimiter, 1);
            _endRepeated = true;
        }

        /// <summary>
        /// Ignores the delimiter if it appears on the first line. This is useful when processing Jekyll style front matter that
        /// has the delimiter both above and below the front matter content. The default behavior is <c>true</c>.
        /// This setting has no effect if a start delimiter is required.
        /// </summary>
        /// <param name="ignore">If set to <c>true</c>, ignore the delimiter if it appears on the first line.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter IgnoreDelimiterOnFirstLine(bool ignore = true)
        {
            _ignoreEndDelimiterOnFirstLine = ignore;
            return this;
        }

        /// <summary>
        /// Requires a start delimiter as the first line of the file.
        /// </summary>
        /// <param name="startDelimiter">The delimiter to require.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter RequireStartDelimiter(string startDelimiter)
        {
            _startDelimiter = startDelimiter;
            _startRepeated = false;
            return this;
        }

        /// <summary>
        /// Requires a start delimiter as the first line of the file.
        /// </summary>
        /// <param name="startDelimiter">The delimiter to require.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter RequireStartDelimiter(char startDelimiter)
        {
            _startDelimiter = new string(startDelimiter, 1);
            _startRepeated = true;
            return this;
        }

        /// <summary>
        /// By default the front matter is removed from the source file. This allows you to preserve
        /// it in the file for further processing.
        /// </summary>
        /// <param name="preserveFrontMatter">Set to <c>true</c> to preserve the front matter, <c>false</c> to remove it.</param>
        /// <returns>The current module instance.</returns>
        public ExtractFrontMatter PreserveFrontMatter(bool preserveFrontMatter = true)
        {
            _preserveFrontMatter = preserveFrontMatter;
            return this;
        }

        // Execute at the context level so we can compile the RegEx pattern(s) once.
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            List<Regex> regexes = new List<Regex>();

            // Get the explicit delimiter regex
            string delimiterRegexString = GetDelimiterRegex(
                _startDelimiter, _startRepeated, _endDelimiter, _endRepeated, _ignoreEndDelimiterOnFirstLine);
            if (delimiterRegexString is object)
            {
                // Single line mode instructs . to match newlines and is different than multiline mode
                regexes.Add(new Regex(
                    delimiterRegexString,
                    RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline));
            }

            // Get other regexes
            if (_regexes is object)
            {
                IEnumerable<Regex> configRegexes = await _regexes.GetValueAsync(null, context);
                if (configRegexes is object)
                {
                    regexes.AddRange(configRegexes);
                }
            }

            // Iterate the documents
            IEnumerable<IDocument> aggregateResults = null;
            foreach (IDocument input in context.Inputs)
            {
                IEnumerable<IDocument> results = await ExecuteInputFuncAsync(
                    input, context, (i, c) => ExecuteInputAsync(i, c, regexes));
                if (results is object)
                {
                    aggregateResults = aggregateResults?.Concat(results) ?? results;
                }
            }
            return aggregateResults;
        }

        private async Task<IEnumerable<IDocument>> ExecuteInputAsync(
            IDocument input, IExecutionContext context, List<Regex> regexes)
        {
            // Really wish we could scan for matches over a stream but oh well
            string inputContent = await input.GetContentStringAsync();
            if (inputContent.IsNullOrWhiteSpace())
            {
                return input.Yield();
            }

            // Find the first regex that matches
            foreach (Regex regex in regexes)
            {
                Match match = regex.Match(inputContent);
                if (match.Success)
                {
                    // If we have a match, get the capture group named "matter" or group 1 if name not found
                    Group group = match.Groups["frontmatter"];
                    if (!group.Success)
                    {
                        group = match.Groups[1];
                    }
                    if (!group.Success)
                    {
                        continue;
                    }

                    // Extract the front matter
                    IContentProvider frontMatterContent = context.GetContentProvider(group.Value);
                    IContentProvider outputContent = input.ContentProvider;
                    if (!_preserveFrontMatter)
                    {
                        outputContent = context.GetContentProvider(
                            inputContent.Remove(match.Index, match.Length),
                            input.ContentProvider.MediaType);
                    }

                    // Execute the child modules and clone the input document
                    foreach (IDocument result in await context.ExecuteModulesAsync(Children, input.Clone(frontMatterContent).Yield()))
                    {
                        return result.Clone(outputContent).Yield();
                    }
                }
            }

            // No front matter found
            return input.Yield();
        }

        /// <summary>
        /// Gets a regex pattern for front matter using the specified delimiters.
        /// </summary>
        /// <param name="endDelimiter">
        /// The front matter end delimiter. If this is null then a null value is returned.
        /// </param>
        /// <param name="endRepeated">
        /// <c>true</c> to allow the end delimiter to repeat, <c>false</c> to require it verbatim.
        /// </param>
        /// <param name="ignoreEndDelimiterOnFirstLine">
        /// Ignores the delimiter if it appears on the first line. This is useful when processing
        /// Jekyll style front matter that has the delimiter both above and below the front matter
        /// content. This has no effect if a start delimiter is specified.
        /// </param>
        /// <returns>A regular expression that finds front matter using the specified delimiters.</returns>
        public static string GetDelimiterRegex(
            string endDelimiter,
            bool endRepeated,
            bool ignoreEndDelimiterOnFirstLine = true) =>
            GetDelimiterRegex(null, false, endDelimiter, endRepeated, ignoreEndDelimiterOnFirstLine);

        /// <summary>
        /// Gets a regex pattern for front matter using the specified delimiters.
        /// </summary>
        /// <remarks>
        /// With Jekyll style delimiters ("-" for the end delimiter allowing repeating and no start delimiter),
        /// this returns <c>\A(?:^\r*-+[^\S\n]*$\r?\n)?(.*?)(?:^\r*-+[^\S\n]*$\r?\n)</c>.
        /// </remarks>
        /// <param name="startDelimiter">
        /// The front matter start delimiter, or null to use the end delimiter as the start delimiter.
        /// </param>
        /// <param name="startRepeated">
        /// <c>true</c> to allow the start delimiter to repeat, <c>false</c> to require it verbatim.
        /// </param>
        /// <param name="endDelimiter">
        /// The front matter end delimiter. If this is null then a null value is returned.
        /// </param>
        /// <param name="endRepeated">
        /// <c>true</c> to allow the end delimiter to repeat, <c>false</c> to require it verbatim.
        /// </param>
        /// <param name="ignoreEndDelimiterOnFirstLine">
        /// Ignores the delimiter if it appears on the first line. This is useful when processing
        /// Jekyll style front matter that has the delimiter both above and below the front matter
        /// content. This has no effect if a start delimiter is specified.
        /// </param>
        /// <returns>A regular expression that finds front matter using the specified delimiters.</returns>
        public static string GetDelimiterRegex(
            string startDelimiter,
            bool startRepeated,
            string endDelimiter,
            bool endRepeated,
            bool ignoreEndDelimiterOnFirstLine = true)
        {
            if (endDelimiter is null)
            {
                return null;
            }

            // Start delimiter
            StringBuilder regexBuilder = new StringBuilder();
            if (startDelimiter is object || ignoreEndDelimiterOnFirstLine)
            {
                regexBuilder.Append(@"\A(?:^\r*");
                if (startDelimiter is object)
                {
                    regexBuilder.Append(Regex.Escape(startDelimiter));
                    if (startRepeated)
                    {
                        regexBuilder.Append("+");
                    }
                }
                else
                {
                    regexBuilder.Append(Regex.Escape(endDelimiter));
                    if (endRepeated)
                    {
                        regexBuilder.Append("+");
                    }
                }
                regexBuilder.Append(@"[^\S\n]*$\r?\n)");
                if (startDelimiter is null)
                {
                    // Only make the start delimiter optional if it's the end delimiter and ignore is toggled
                    regexBuilder.Append("?");
                }
            }

            // Capture group (use .*? since it's lazy and will stop at the first instance of the next group)
            regexBuilder.Append("(.*?)");

            // End delimiter
            regexBuilder.Append(@"(?:^\r*");
            regexBuilder.Append(Regex.Escape(endDelimiter));
            if (endRepeated)
            {
                regexBuilder.Append("+");
            }
            regexBuilder.Append(@"[^\S\n]*$(\r?\n)?)");

            return regexBuilder.ToString();
        }
    }
}