using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngleSharp.Dom;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Converts all relative links to root-relative using the destination path of the containing document.
    /// </summary>
    /// <category name="Content" />
    public class MakeLinksRootRelative : ParallelModule
    {
        protected override Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context) =>
            ProcessHtml.ProcessElementsAsync(
                input,
                context,
                "[href],[src]",
                false,
                (d, c, e, m) =>
                {
                    MakeLinkRootRelative(e, "href", d, context);
                    MakeLinkRootRelative(e, "src", d, context);
                });

        private static void MakeLinkRootRelative(IElement element, string attribute, Common.IDocument document, IExecutionContext context)
        {
            string value = element.GetAttribute(attribute);
            if (!string.IsNullOrEmpty(value)
                && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri uri)
                && !uri.IsAbsoluteUri
                && !document.Destination.IsNullOrEmpty
                && !document.Destination.Parent.IsNullOrEmpty)
            {
                element.SetAttribute(attribute, context.GetLink(document.Destination.Parent.Combine(value)));
            }
        }
    }
}