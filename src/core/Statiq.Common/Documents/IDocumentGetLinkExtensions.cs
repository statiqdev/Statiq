using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IDocumentGetLinkExtensions
    {
        /// <summary>
        /// Gets a link for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="document">The document to generate a link for.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(this IDocument document, bool includeHost = false) =>
            IExecutionContext.Current.GetLink(document, includeHost);

        /// <summary>
        /// Gets a link for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="document">The document to generate a link for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(this IDocument document, string queryAndFragment, bool includeHost = false) =>
            IExecutionContext.Current.GetLink(IExecutionContext.Current.LinkGenerator.AddQueryAndFragment(document.Destination.FullPath, queryAndFragment), includeHost);
    }
}