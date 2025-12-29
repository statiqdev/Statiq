using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Queries HTML content of the input documents and modifies the elements that
    /// match a query selector.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <category name="Content" />
    public class ProcessHtml : ParallelModule
    {
        private readonly string _querySelector;
        private readonly Action<Common.IDocument, IExecutionContext, IElement, Dictionary<string, object>> _processElement;
        private bool _first;

        /// <summary>
        /// Creates the module with the specified query selector and processing function.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="processElement">
        /// A delegate to apply to each found element.
        /// The <see cref="Dictionary{TKey, TValue}"/> holds any additional metadata that should be added to the document.
        /// </param>
        public ProcessHtml(
            string querySelector,
            Action<Common.IDocument, IExecutionContext, IElement, Dictionary<string, object>> processElement)
        {
            _querySelector = querySelector;
            _processElement = processElement.ThrowIfNull(nameof(processElement));
        }

        /// <summary>
        /// Creates the module with the specified query selector and processing function.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="processElement">
        /// A delegate to apply to each found element.
        /// </param>
        public ProcessHtml(string querySelector, Action<Common.IDocument, IExecutionContext, IElement> processElement)
            : this(querySelector, (d, c, e, _) => processElement(d, c, e))
        {
            processElement.ThrowIfNull(nameof(processElement));
        }

        /// <summary>
        /// Creates the module with the specified query selector and processing function.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="processElement">
        /// A delegate to apply to each found element.
        /// The <see cref="Dictionary{TKey, TValue}"/> holds any additional metadata that should be added to the document.
        /// </param>
        public ProcessHtml(string querySelector, Action<IElement, Dictionary<string, object>> processElement)
            : this(querySelector, (_, __, e, m) => processElement(e, m))
        {
            processElement.ThrowIfNull(nameof(processElement));
        }

        /// <summary>
        /// Creates the module with the specified query selector and processing function.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="processElement">
        /// A delegate to apply to each found element.
        /// </param>
        public ProcessHtml(string querySelector, Action<IElement> processElement)
            : this(querySelector, (_, __, e, m) => processElement(e))
        {
            processElement.ThrowIfNull(nameof(processElement));
        }

        /// <summary>
        /// Specifies that only the first query result should be processed (the default is <c>false</c>).
        /// </summary>
        /// <param name="first">If set to <c>true</c>, only the first result is processed.</param>
        /// <returns>The current module instance.</returns>
        public ProcessHtml First(bool first = true)
        {
            _first = first;
            return this;
        }

        protected override Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context) =>
            ProcessElementsAsync(input, context, _querySelector, _first, _processElement);

        internal static async Task<IEnumerable<Common.IDocument>> ProcessElementsAsync(
            Common.IDocument input,
            IExecutionContext context,
            string querySelector,
            bool first,
            Action<Common.IDocument, IExecutionContext, IElement, Dictionary<string, object>> processElement)
        {
            // Parse the HTML content
            IHtmlDocument originalHtmlDocument = await input.ParseHtmlAsync(false);
            if (originalHtmlDocument is null)
            {
                return input.Yield();
            }

            // Evaluate the query selector
            try
            {
                if (!string.IsNullOrWhiteSpace(querySelector))
                {
                    IHtmlDocument htmlDocument = (IHtmlDocument)originalHtmlDocument.Clone(true); // Clone the document so we know if it changed
                    IElement[] elements = first
                        ? new[] { htmlDocument.QuerySelector(querySelector) }
                        : htmlDocument.QuerySelectorAll(querySelector).ToArray();
                    if (elements.Length > 0 && elements[0] is object)
                    {
                        Dictionary<string, object> metadata = new Dictionary<string, object>();
                        foreach (IElement element in elements)
                        {
                            processElement(input, context, element, metadata);
                        }

                        if (originalHtmlDocument.Equals(htmlDocument))
                        {
                            // Elements were not edited so return the original document or clone it with new metadata
                            return metadata.Count == 0 ? input.Yield() : input.Clone(metadata).Yield();
                        }

                        // Elements were edited so get the new content
                        IContentProvider contentProvider = context.GetContentProvider(htmlDocument);
                        Common.IDocument output = metadata.Count == 0
                            ? input.Clone(contentProvider)
                            : input.Clone(metadata, contentProvider);
                        return output.Yield();
                    }
                }
                return input.Yield();
            }
            catch (Exception ex)
            {
                context.LogWarning("Exception while processing HTML for {0}: {1}", input.ToSafeDisplayString(), ex.Message);
                return input.Yield();
            }
        }
    }
}