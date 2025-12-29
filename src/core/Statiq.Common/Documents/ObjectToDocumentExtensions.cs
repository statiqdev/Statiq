using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    public static class ObjectToDocumentExtensions
    {
        // ToDocument

        public static IDocument ToDocument<T>(
            this T obj,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, destination, new Metadata(items), contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            in NormalizedPath source,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, source, destination, null, contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            in NormalizedPath destination,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, destination, null, contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, null, new Metadata(items), contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, null, null, null, contentProvider);

        public static IDocument ToDocument<T>(
            this T obj,
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            GetObjectDocument(obj, source, destination, new Metadata(items), contentProvider);

        // ToDocuments

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x is null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    new Metadata(itemsFunc?.Invoke(x)),
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> sourceFunc,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x is null
                ? null
                : GetObjectDocument(
                    x,
                    sourceFunc?.Invoke(x) ?? NormalizedPath.Null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x is null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x is null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    null,
                    new Metadata(itemsFunc?.Invoke(x)),
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x is null
                ? null
                : GetObjectDocument(
                    x,
                    null,
                    null,
                    null,
                    contentProviderFunc?.Invoke(x)));

        public static IEnumerable<IDocument> ToDocuments<T>(
            this IEnumerable<T> objs,
            Func<T, NormalizedPath> sourceFunc,
            Func<T, NormalizedPath> destinationFunc,
            Func<T, IEnumerable<KeyValuePair<string, object>>> itemsFunc,
            Func<T, IContentProvider> contentProviderFunc = null) =>
            objs?.Select(x => x is null
                ? null
                : GetObjectDocument(
                    x,
                    sourceFunc?.Invoke(x) ?? NormalizedPath.Null,
                    destinationFunc?.Invoke(x) ?? NormalizedPath.Null,
                    new Metadata(itemsFunc?.Invoke(x)),
                    contentProviderFunc?.Invoke(x)));

        // Construct an ObjectDocument<T> from the actual type of the document
        private static IDocument GetObjectDocument<T>(
            T obj,
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
        {
            obj.ThrowIfNull(nameof(obj));

            // Check if this is already an IDocument
            if (obj is IDocument document)
            {
                // It's already a document so return or clone it instead of wrapping
                if (source.IsNull && destination.IsNull && items is null && contentProvider is null)
                {
                    // Not setting anything new so return the same document
                    return document;
                }

                // Clone the document with the new values
                return document.Clone(source, destination, items, contentProvider);
            }

            // Check the actual type of the document
            Type objType = obj.GetType();
            if (typeof(T).Equals(objType))
            {
                // Fast track if the type is the same
                return new ObjectDocument<T>(
                    obj,
                    source,
                    destination,
                    items,
                    contentProvider);
            }

            // The generic type isn't the same as the actual type so use reflection to get it right
            return (IDocument)Activator.CreateInstance(
                typeof(ObjectDocument<>).MakeGenericType(objType),
                new object[]
                {
                    obj,
                    source,
                    destination,
                    items,
                    contentProvider
                });
        }
    }
}
