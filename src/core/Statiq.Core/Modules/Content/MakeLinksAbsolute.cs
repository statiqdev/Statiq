using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Converts all relative links to absolute using the "Host" and other settings values.
    /// </summary>
    /// <remarks>
    /// This module is particularly useful when presenting content for external consumption such
    /// as with the <c>GenerateFeeds</c> module or for use in an API.
    /// </remarks>
    /// <category name="Content" />
    public class MakeLinksAbsolute : ParallelModule
    {
        protected override Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context) =>
            ProcessHtml.ProcessElementsAsync(
                input,
                context,
                "[href],[src]",
                false,
                (d, c, e, m) =>
                {
                    MakeLinkAbsolute(e, "href", d, context);
                    MakeLinkAbsolute(e, "src", d, context);
                });

        private static void MakeLinkAbsolute(IElement element, string attribute, Common.IDocument document, IExecutionContext context)
        {
            string value = element.GetAttribute(attribute);
            if (!string.IsNullOrEmpty(value)
                && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri uri)
                && !uri.IsAbsoluteUri)
            {
                element.SetAttribute(attribute, context.GetLink(document.Destination.Parent.Combine(value), true));
            }
        }
    }
}