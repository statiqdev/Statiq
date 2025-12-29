using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions to <see cref="IExecutionContext"/> that create a document from <see cref="Stream"/> content sources.
    /// </summary>
    public static class IExecutionContextCreateDocumentFromStreamExtensions
    {
        public static IDocument CreateDocument(
            this IExecutionContext context,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null) =>
            context.CreateDocument(null, destination, items, context.GetContentProvider(stream, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            in NormalizedPath source,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null) =>
            context.CreateDocument(source, destination, null, context.GetContentProvider(stream, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null) =>
            context.CreateDocument(null, destination, null, context.GetContentProvider(stream, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null) =>
            context.CreateDocument(null, null, items, context.GetContentProvider(stream, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            Stream stream,
            string mediaType = null) =>
            context.CreateDocument(null, null, null, context.GetContentProvider(stream, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, destination, items, context.GetContentProvider(stream, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            in NormalizedPath source,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(source, destination, null, context.GetContentProvider(stream, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, destination, null, context.GetContentProvider(stream, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, null, items, context.GetContentProvider(stream, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, null, null, context.GetContentProvider(stream, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null) =>
            document?.Clone(destination, items, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument(destination, items, context.GetContentProvider(stream, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null) =>
            document?.Clone(source, destination, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument(source, destination, context.GetContentProvider(stream, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null) =>
            document?.Clone(destination, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument(destination, context.GetContentProvider(stream, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null) =>
            document?.Clone(items, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument(items, context.GetContentProvider(stream, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            Stream stream,
            string mediaType = null) =>
            document?.Clone(context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument(context.GetContentProvider(stream, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null) =>
            document?.Clone(source, destination, items, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument(source, destination, items, context.GetContentProvider(stream, mediaType));

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, items, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument<TDocument>(destination, items, context.GetContentProvider(stream, mediaType));

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument<TDocument>(source, destination, context.GetContentProvider(stream, mediaType));

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            in NormalizedPath destination,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument<TDocument>(destination, context.GetContentProvider(stream, mediaType));

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(items, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument<TDocument>(items, context.GetContentProvider(stream, mediaType));

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument<TDocument>(context.GetContentProvider(stream, mediaType));

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            Stream stream,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, items, context.GetContentProvider(stream, mediaType))
                ?? context.CreateDocument<TDocument>(source, destination, items, context.GetContentProvider(stream, mediaType));
    }
}
