using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IDocumentCloneExtensions
    {
        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document.Clone(null, destination, items, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            document.Clone(source, destination, null, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            document.Clone(null, destination, null, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document.Clone(null, null, items, contentProvider);

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            IContentProvider contentProvider) =>
            document.Clone(null, null, null, contentProvider);

        // Stream

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items.</param>
        /// <param name="stream">A stream that contains the new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null) =>
            document.Clone(null, destination, items, IExecutionContext.Current.GetContentProvider(stream, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="stream">A stream that contains the new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null) =>
            document.Clone(source, destination, null, IExecutionContext.Current.GetContentProvider(stream, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="stream">A stream that contains the new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null) =>
            document.Clone(null, destination, null, IExecutionContext.Current.GetContentProvider(stream, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="stream">A stream that contains the new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null) =>
            document.Clone(null, null, items, IExecutionContext.Current.GetContentProvider(stream, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="stream">A stream that contains the new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            Stream stream,
            string mediaType = null) =>
            document.Clone(null, null, null, IExecutionContext.Current.GetContentProvider(stream, mediaType));

        // String

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items.</param>
        /// <param name="content">The new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document.Clone(null, destination, items, IExecutionContext.Current.GetContentProvider(content, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="content">The new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            document.Clone(source, destination, null, IExecutionContext.Current.GetContentProvider(content, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="content">The new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            document.Clone(null, destination, null, IExecutionContext.Current.GetContentProvider(content, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="content">The new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document.Clone(null, null, items, IExecutionContext.Current.GetContentProvider(content, mediaType));

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="content">The new content.</param>
        /// <param name="mediaType">The media type of the content.</param>
        /// <returns>A new document of the same type as this document.</returns>
        public static IDocument Clone(
            this IDocument document,
            string content,
            string mediaType = null) =>
            document.Clone(null, null, null, IExecutionContext.Current.GetContentProvider(content, mediaType));
    }
}
