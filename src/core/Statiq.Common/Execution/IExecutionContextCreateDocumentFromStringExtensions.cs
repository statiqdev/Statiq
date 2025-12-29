using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions to <see cref="IExecutionContext"/> that create a document from string content sources.
    /// </summary>
    public static class IExecutionContextCreateDocumentFromStringExtensions
    {
        public static IDocument CreateDocument(
            this IExecutionContext context,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, destination, items, context.GetContentProvider(content, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            context.CreateDocument(source, destination, null, context.GetContentProvider(content, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, destination, null, context.GetContentProvider(content, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, null, items, context.GetContentProvider(content, mediaType));

        public static IDocument CreateDocument(
            this IExecutionContext context,
            string content,
            string mediaType = null) =>
            context.CreateDocument(null, null, null, context.GetContentProvider(content, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, destination, items, context.GetContentProvider(content, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(source, destination, null, context.GetContentProvider(content, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, destination, null, context.GetContentProvider(content, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, null, items, context.GetContentProvider(content, mediaType));

        public static TDocument CreateDocument<TDocument>(
            this IExecutionContext context,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            context.CreateDocument<TDocument>(null, null, null, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document?.Clone(destination, items, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument(destination, items, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            document?.Clone(source, destination, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument(source, destination, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath destination,
            string content,
            string mediaType = null) =>
            document?.Clone(destination, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument(destination, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document?.Clone(items, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument(items, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            string content,
            string mediaType = null) =>
            document?.Clone(context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument(context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument(
            this IExecutionContext context,
            IDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null) =>
            document?.Clone(source, destination, items, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument(source, destination, items, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, items, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument<TDocument>(destination, items, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument<TDocument>(source, destination, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath destination,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument<TDocument>(destination, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(items, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument<TDocument>(items, context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument<TDocument>(context.GetContentProvider(content, mediaType));

        public static IDocument CloneOrCreateDocument<TDocument>(
            this IExecutionContext context,
            TDocument document,
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            string content,
            string mediaType = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, items, context.GetContentProvider(content, mediaType))
                ?? context.CreateDocument<TDocument>(source, destination, items, context.GetContentProvider(content, mediaType));
    }
}
