using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IDocumentFactoryCreateDocumentExtensions
    {
        public static IDocument CreateDocument(
            this IDocumentFactory documentFactory,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            documentFactory.CreateDocument(null, destination, items, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory documentFactory,
            in NormalizedPath source,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            documentFactory.CreateDocument(source, destination, null, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory documentFactory,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            documentFactory.CreateDocument(null, destination, null, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory documentFactory,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            documentFactory.CreateDocument(null, null, items, contentProvider);

        public static IDocument CreateDocument(
            this IDocumentFactory documentFactory,
            IContentProvider contentProvider = null) =>
            documentFactory.CreateDocument(null, null, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            documentFactory.CreateDocument<TDocument>(null, destination, items, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            in NormalizedPath source,
            in NormalizedPath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            documentFactory.CreateDocument<TDocument>(source, destination, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            in NormalizedPath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            documentFactory.CreateDocument<TDocument>(null, destination, null, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            documentFactory.CreateDocument<TDocument>(null, null, items, contentProvider);

        public static TDocument CreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            documentFactory.CreateDocument<TDocument>(null, null, null, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory documentFactory,
            IDocument document,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, items, contentProvider)
                ?? documentFactory.CreateDocument(destination, items, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory documentFactory,
            IDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, contentProvider)
                ?? documentFactory.CreateDocument(source, destination, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory documentFactory,
            IDocument document,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            document?.Clone(destination, contentProvider)
                ?? documentFactory.CreateDocument(destination, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory documentFactory,
            IDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(items, contentProvider)
                ?? documentFactory.CreateDocument(items, contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory documentFactory,
            IDocument document,
            IContentProvider contentProvider = null) =>
            document?.Clone(contentProvider)
                ?? documentFactory.CreateDocument(contentProvider);

        public static IDocument CloneOrCreateDocument(
            this IDocumentFactory documentFactory,
            IDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            document?.Clone(source, destination, items, contentProvider)
                ?? documentFactory.CreateDocument(source, destination, items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            TDocument document,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, items, contentProvider)
                ?? documentFactory.CreateDocument<TDocument>(destination, items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            TDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, contentProvider)
                ?? documentFactory.CreateDocument<TDocument>(source, destination, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            TDocument document,
            in NormalizedPath destination,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(destination, contentProvider)
                ?? documentFactory.CreateDocument<TDocument>(destination, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            TDocument document,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(items, contentProvider)
                ?? documentFactory.CreateDocument<TDocument>(items, contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            TDocument document,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(contentProvider)
                ?? documentFactory.CreateDocument<TDocument>(contentProvider);

        public static TDocument CloneOrCreateDocument<TDocument>(
            this IDocumentFactory documentFactory,
            TDocument document,
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)document?.Clone(source, destination, items, contentProvider)
                ?? documentFactory.CreateDocument<TDocument>(source, destination, items, contentProvider);
    }
}
