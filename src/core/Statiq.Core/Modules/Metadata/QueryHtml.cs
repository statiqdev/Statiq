using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Queries HTML content of the input documents and creates new documents with content and metadata from the results.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Once you provide a DOM query selector, the module creates new output documents
    /// for each query result and allows you to set the new document content and/or set new
    /// metadata based on the query result.
    /// </para>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <metadata cref="Keys.OuterHtml" usage="Output"/>
    /// <metadata cref="Keys.InnerHtml" usage="Output"/>
    /// <metadata cref="Keys.TextContent" usage="Output"/>
    /// <category name="Metadata" />
    public class QueryHtml : ParallelModule
    {
        private readonly List<Action<IElement, Dictionary<string, object>>> _metadataActions
            = new List<Action<IElement, Dictionary<string, object>>>();

        private readonly string _querySelector;
        private bool _first;
        private bool? _outerHtmlContent;

        /// <summary>
        /// Creates the module with the specified query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        public QueryHtml(string querySelector)
        {
            _querySelector = querySelector;
        }

        /// <summary>
        /// Specifies that only the first query result should be processed (the default is <c>false</c>).
        /// </summary>
        /// <param name="first">If set to <c>true</c>, only the first result is processed.</param>
        /// <returns>The current module instance.</returns>
        public QueryHtml First(bool first = true)
        {
            _first = first;
            return this;
        }

        /// <summary>
        /// Sets the content of the result document(s) to the content of the corresponding query result,
        /// optionally specifying whether inner or outer HTML content should be used. The default is
        /// <c>null</c>, which does not add any content to the result documents (only metadata).
        /// </summary>
        /// <param name="outerHtml">
        /// If set to <c>true</c>, outer HTML content is used for the document content.
        /// If set to <c>false</c>, inner HTML content is used for the document content.
        /// If <c>null</c>, no document content is set.
        /// </param>
        /// <returns>The current module instance.</returns>
        public QueryHtml SetContent(bool? outerHtml = true)
        {
            _outerHtmlContent = outerHtml;
            return this;
        }

        /// <summary>
        /// Gets the outer HTML of each query result and sets it in the metadata of the
        /// corresponding result document(s) with the specified key.
        /// </summary>
        /// <param name="metadataKey">The metadata key in which to place the outer HTML.</param>
        /// <returns>The current module instance.</returns>
        public QueryHtml GetOuterHtml(string metadataKey = Keys.OuterHtml)
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.FormattedOuterHtml());
            }
            return this;
        }

        /// <summary>
        /// Gets the inner HTML of each query result and sets it in the metadata of the
        /// corresponding result document(s) with the specified key.
        /// </summary>
        /// <param name="metadataKey">The metadata key in which to place the inner HTML.</param>
        /// <returns>The current module instance.</returns>
        public QueryHtml GetInnerHtml(string metadataKey = Keys.InnerHtml)
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.FormattedInnerHtml());
            }
            return this;
        }

        /// <summary>
        /// Gets the text content of each query result and sets it in the metadata of
        /// the corresponding result document(s) with the specified key.
        /// </summary>
        /// <param name="metadataKey">The metadata key in which to place the text content.</param>
        /// <returns>The current module instance.</returns>
        public QueryHtml GetTextContent(string metadataKey = Keys.TextContent)
        {
            if (!string.IsNullOrWhiteSpace(metadataKey))
            {
                _metadataActions.Add((e, d) => d[metadataKey] = e.TextContent);
            }
            return this;
        }

        /// <summary>
        /// Gets the specified attribute value of each query result and sets it in the metadata
        /// of the corresponding result document(s). If the attribute is not found for a given
        /// query result, no metadata is set. If <c>metadataKey</c> is <c>null</c>, the attribute name will
        /// be used as the metadata key, otherwise the specified metadata key will be used.
        /// </summary>
        /// <param name="attributeName">Name of the attribute to get.</param>
        /// <param name="metadataKey">The metadata key in which to place the attribute value.</param>
        /// <returns>The current module instance.</returns>
        public QueryHtml GetAttributeValue(string attributeName, string metadataKey = null)
        {
            if (string.IsNullOrWhiteSpace(metadataKey))
            {
                metadataKey = attributeName;
            }
            _metadataActions.Add((e, d) =>
            {
                if (e.HasAttribute(attributeName))
                {
                    d[metadataKey] = e.GetAttribute(attributeName);
                }
            });
            return this;
        }

        /// <summary>
        /// Gets the values for all attributes of each query result and sets them in the
        /// metadata of the corresponding result document(s) with keys names equal to the attribute local name.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public QueryHtml GetAttributeValues()
        {
            _metadataActions.Add((e, d) =>
            {
                foreach (IAttr attribute in e.Attributes)
                {
                    d[attribute.LocalName] = attribute.Value;
                }
            });
            return this;
        }

        /// <summary>
        /// Gets all information for each query result and sets the metadata of the corresponding result
        /// document(s). This is equivalent to calling <c>GetOuterHtml()</c>, <c>GetInnerHtml()</c>,
        /// <c>GetTextContent()</c>, and <c>GetAttributeValues()</c> with default arguments.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public QueryHtml GetAll()
        {
            GetOuterHtml();
            GetInnerHtml();
            GetTextContent();
            GetAttributeValues();
            return this;
        }

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context)
        {
            // Parse the HTML content
            IHtmlDocument htmlDocument = await input.ParseHtmlAsync(false);
            if (htmlDocument is null)
            {
                return input.Yield();
            }

            // Evaluate the query selector
            try
            {
                if (!string.IsNullOrWhiteSpace(_querySelector))
                {
                    IElement[] elements = _first
                        ? new[] { htmlDocument.QuerySelector(_querySelector) }
                        : htmlDocument.QuerySelectorAll(_querySelector).ToArray();
                    if (elements.Length > 0 && elements[0] is object)
                    {
                        List<Common.IDocument> documents = new List<Common.IDocument>();
                        foreach (IElement element in elements)
                        {
                            // Get the metadata
                            Dictionary<string, object> metadata = new Dictionary<string, object>();
                            foreach (Action<IElement, Dictionary<string, object>> metadataAction in _metadataActions)
                            {
                                metadataAction(element, metadata);
                            }

                            // Clone the document and optionally change content to the HTML element
                            if (_outerHtmlContent.HasValue)
                            {
                                documents.Add(
                                    input.Clone(
                                        metadata.Count == 0 ? null : metadata,
                                        context.GetContentProvider(
                                            _outerHtmlContent.Value ? (IMarkupFormattable)element : element.ChildNodes)));
                            }
                            else
                            {
                                documents.Add(input.Clone(metadata));
                            }
                        }
                        return documents;
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