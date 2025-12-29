using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Finds the first occurrence of a specified HTML comment or element and stores it's contents as metadata.
    /// </summary>
    /// <remarks>
    /// This module is useful for situations like displaying the first paragraph of your most recent
    /// blog posts or generating RSS and Atom feeds.
    /// This module looks for the first occurrence of an excerpt separator (default of <c>more</c> or <c>excerpt</c>)
    /// contained within an HTML comment (<c>&lt;!--more--&gt;</c>). If a separator comment isn't found, the module
    /// will fallback to looking for the first occurrence of a specific HTML element (<c>p</c> paragraph elements by default)
    /// and will use the outer HTML content. In both cases, the excerpt is placed in metadata with a key of <c>Excerpt</c>.
    /// The content of the original input document is left unchanged.
    /// </remarks>
    /// <metadata cref="Keys.Excerpt" usage="Output"/>
    /// <category name="Metadata" />
    public class GenerateExcerpt : ParallelModule
    {
        private bool _keepExisting = true;
        private string[] _separators = { "more", "excerpt" };
        private string _querySelector = "p";
        private string _metadataKey = Keys.Excerpt;
        private bool _outerHtml = true;

        /// <summary>
        /// Creates the module with the default query selector of <c>p</c>.
        /// </summary>
        /// <param name="keepExisting"><c>true</c> to keep existing excerpt metadata, <c>false</c> to always replace it with a calculated excerpt.</param>
        public GenerateExcerpt(bool keepExisting = true)
        {
            _keepExisting = keepExisting;
        }

        /// <summary>
        /// Specifies alternate separators to be used in an HTML comment.
        /// Setting this to <c>null</c> will disable looking for separators
        /// and rely only on the query selector.
        /// </summary>
        /// <param name="separators">The excerpt separators.</param>
        /// <param name="keepExisting"><c>true</c> to keep existing excerpt metadata, <c>false</c> to always replace it with a calculated excerpt.</param>
        public GenerateExcerpt(string[] separators, bool keepExisting = true)
        {
            _separators = separators;
            _keepExisting = keepExisting;
        }

        /// <summary>
        /// Specifies an alternate query selector for the content.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <param name="keepExisting"><c>true</c> to keep existing excerpt metadata, <c>false</c> to always replace it with a calculated excerpt.</param>
        public GenerateExcerpt(string querySelector, bool keepExisting = true)
        {
            _querySelector = querySelector;
            _keepExisting = keepExisting;
        }

        /// <summary>
        /// Allows you to specify an alternate metadata key.
        /// </summary>
        /// <param name="metadataKey">The metadata key to store the excerpt in.</param>
        /// <returns>The current module instance.</returns>
        public GenerateExcerpt WithMetadataKey(string metadataKey)
        {
            _metadataKey = metadataKey;
            return this;
        }

        /// <summary>
        /// Specifies alternate separators to be used in an HTML comment.
        /// Setting this to <c>null</c> will disable looking for separators
        /// and rely only on the query selector.
        /// </summary>
        /// <param name="separators">The excerpt separators.</param>
        /// <returns>The current module instance.</returns>
        public GenerateExcerpt WithSeparators(string[] separators)
        {
            _separators = separators;
            return this;
        }

        /// <summary>
        /// Allows you to specify an alternate query selector. If a separator
        /// comment was found then the query selector will be used to determine which
        /// elements prior to the separator the excerpt should be taken from.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <returns>The current module instance.</returns>
        public GenerateExcerpt WithQuerySelector(string querySelector)
        {
            _querySelector = querySelector;
            return this;
        }

        /// <summary>
        /// Controls whether the inner HTML (not including the containing element's HTML) or
        /// outer HTML (including the containing element's HTML) of the first result from
        /// the query selector is added to metadata. The default is true, which gets the outer
        /// HTML content. This setting has no effect if a separator comment is found.
        /// </summary>
        /// <param name="outerHtml">If set to <c>true</c>, outer HTML will be stored.</param>
        /// <returns>The current module instance.</returns>
        public GenerateExcerpt WithOuterHtml(bool outerHtml)
        {
            _outerHtml = outerHtml;
            return this;
        }

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context)
        {
            if (string.IsNullOrWhiteSpace(_metadataKey) || (_keepExisting && input.ContainsKey(_metadataKey)))
            {
                return input.Yield();
            }

            // Parse the HTML content
            IHtmlDocument htmlDocument = await input.ParseHtmlAsync(false);
            if (htmlDocument is null)
            {
                return input.Yield();
            }

            // Get the query string excerpt first
            string queryExcerpt = GetQueryExcerpt(htmlDocument);

            // Now try to get a excerpt separator
            string separatorExcerpt = GetSeparatorExcerpt(htmlDocument);

            // Set the metadata
            string excerpt = separatorExcerpt ?? queryExcerpt;
            if (excerpt is object)
            {
                return input
                    .Clone(new MetadataItems
                    {
                        { _metadataKey,  excerpt.Trim() }
                    })
                    .Yield();
            }
            return input.Yield();
        }

        private string GetQueryExcerpt(IHtmlDocument htmlDocument)
        {
            if (!string.IsNullOrEmpty(_querySelector))
            {
                IElement element = htmlDocument.QuerySelector(_querySelector);
                return _outerHtml ? element?.FormattedOuterHtml() : element?.FormattedInnerHtml();
            }
            return null;
        }

        // Use this after attempting to find the excerpt element because it destroys the HTML document
        private string GetSeparatorExcerpt(IHtmlDocument htmlDocument)
        {
            if (_separators?.Length > 0)
            {
                ITreeWalker walker = htmlDocument.CreateTreeWalker(htmlDocument.DocumentElement, FilterSettings.Comment);
                IComment comment = (IComment)walker.ToFirst();
                while (comment is object && !_separators.Contains(comment.NodeValue.Trim(), StringComparer.OrdinalIgnoreCase))
                {
                    comment = (IComment)walker.ToNext();
                }

                // Found the first separator
                if (comment is object)
                {
                    // Get a clone of the parent element
                    IElement parent = comment.ParentElement;
                    if (parent.TagName.Equals("p", StringComparison.OrdinalIgnoreCase))
                    {
                        // If we were in a tag inside a paragraph, ascend to the paragraph's parent
                        parent = parent.ParentElement;
                    }

                    // Now remove everything after the separator
                    walker = htmlDocument.CreateTreeWalker(parent);
                    bool remove = false;
                    Stack<INode> removeStack = new Stack<INode>();
                    INode node = walker.ToFirst();
                    while (node is object)
                    {
                        if (node == comment)
                        {
                            remove = true;
                        }

                        // Also remove if it's a top-level element that doesn't match the query selector
                        if (remove
                            || (node.Parent == parent
                            && node is IElement element
                            && !string.IsNullOrEmpty(_querySelector)
                            && !element.Matches(_querySelector)))
                        {
                            removeStack.Push(node);
                        }
                        node = walker.ToNext();
                    }
                    while (removeStack.Count > 0)
                    {
                        node = removeStack.Pop();
                        node.Parent.RemoveChild(node);
                    }

                    return parent.FormattedInnerHtml();
                }
            }
            return null;
        }
    }
}