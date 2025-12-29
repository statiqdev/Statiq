using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions for working with specific types of collections.
    /// </summary>
    public static class IDocumentEnumerableExtensions
    {
        public static DocumentList<TDocument> ToDocumentList<TDocument>(this IEnumerable<TDocument> documents)
            where TDocument : IDocument =>
            new DocumentList<TDocument>(documents);

        /// <summary>
        /// Creates an immutable array from the specified document collection and removes null items.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents to convert to an immutable array.</param>
        /// <returns>An immutable array of documents.</returns>
        public static ImmutableArray<TDocument> ToImmutableDocumentArray<TDocument>(this IEnumerable<TDocument> documents)
            where TDocument : IDocument
        {
            if (documents is null)
            {
                return ImmutableArray<TDocument>.Empty;
            }

            // Convert to unsorted immutable array while eliminating nulls
            return documents is ImmutableArray<TDocument> documentsArray && !documentsArray.Any(x => x is null)
                ? documentsArray
                : documents.Where(x => x is object).ToImmutableArray();
        }

        /// <summary>
        /// Returns all documents that contain the specified metadata key.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKey">The key.</param>
        /// <returns>All documents that contain the specified metadata key.</returns>
        public static DocumentList<TDocument> WhereContainsKey<TDocument>(this IEnumerable<TDocument> documents, string metadataKey)
            where TDocument : IDocument =>
            documents.Where(x => x.ContainsKey(metadataKey)).ToDocumentList();

        /// <summary>
        /// Returns all documents that contain all of the specified metadata keys.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKeys">The metadata keys.</param>
        /// <returns>All documents that contain all of the specified metadata keys.</returns>
        public static DocumentList<TDocument> WhereContainsAllKeys<TDocument>(this IEnumerable<TDocument> documents, params string[] metadataKeys)
            where TDocument : IDocument =>
            documents.Where(x => metadataKeys.All(x.ContainsKey)).ToDocumentList();

        /// <summary>
        /// Returns all documents that contain any of the specified metadata keys.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents.</param>
        /// <param name="metadataKeys">The metadata keys.</param>
        /// <returns>All documents that contain any of the specified metadata keys.</returns>
        public static DocumentList<TDocument> WhereContainsAnyKeys<TDocument>(this IEnumerable<TDocument> documents, params string[] metadataKeys)
            where TDocument : IDocument =>
            documents.Where(x => metadataKeys.Any(x.ContainsKey)).ToDocumentList();

        /// <summary>
        /// Filters the documents by source.
        /// </summary>
        /// <remarks>
        /// This filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls. This also flattens the
        /// documents using <see cref="Keys.Children"/> before filtering.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static FilteredDocumentList<TDocument> FilterSources<TDocument>(this IEnumerable<TDocument> documents, params string[] patterns)
            where TDocument : IDocument =>
            documents.FilterSources((IEnumerable<string>)patterns);

        /// <summary>
        /// Filters the documents by source.
        /// </summary>
        /// <remarks>
        /// This module filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <param name="flatten">
        /// <c>true</c> to flatten the documents, <c>false</c> otherwise.
        /// If <c>false</c> only the top-level sequence (usually the parent-most documents) will be filtered.
        /// </param>
        /// <param name="childrenKey">
        /// The metadata key that contains the children or <c>null</c> to flatten documents in all metadata keys.
        /// This parameter has no effect if <paramref name="flatten"/> is <c>false</c>.
        /// </param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static FilteredDocumentList<TDocument> FilterSources<TDocument>(
            this IEnumerable<TDocument> documents,
            IEnumerable<string> patterns,
            bool flatten = true,
            string childrenKey = Keys.Children)
            where TDocument : IDocument
        {
            documents.ThrowIfNull(nameof(documents));

            DocumentFileProvider fileProvider = new DocumentFileProvider((IEnumerable<IDocument>)documents, true, flatten, childrenKey);
            IEnumerable<IDirectory> directories = IExecutionContext.Current.FileSystem
                .GetInputDirectories()
                .Select(x => fileProvider.GetDirectory(x.Path));
            IEnumerable<IFile> matches = directories.SelectMany(x => Globber.GetFiles(x, patterns));
            return new FilteredDocumentList<TDocument>(
                matches
                    .Select(x => x.Path)
                    .Distinct()
                    .Select(match => fileProvider.GetDocument(match))
                    .Cast<TDocument>()
                    .OrderByDescending(x => x.Timestamp),
                x => x.Source,
                (docs, patterns) => docs.FilterSources(patterns));
        }

        /// <summary>
        /// Filters the documents by destination.
        /// </summary>
        /// <remarks>
        /// This module filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls. This also flattens the
        /// documents using <see cref="Keys.Children"/> before filtering.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static FilteredDocumentList<TDocument> FilterDestinations<TDocument>(this IEnumerable<TDocument> documents, params string[] patterns)
            where TDocument : IDocument =>
            documents.FilterDestinations((IEnumerable<string>)patterns);

        /// <summary>
        /// Filters the documents by destination.
        /// </summary>
        /// <remarks>
        /// This module filters documents using "or" logic. If you want to also apply
        /// "and" conditions, chain additional calls.
        /// </remarks>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="patterns">The globbing pattern(s) to match.</param>
        /// <param name="flatten">
        /// <c>true</c> to flatten the documents, <c>false</c> otherwise.
        /// If <c>false</c> only the top-level sequence (usually the parent-most documents) will be filtered.
        /// </param>
        /// <param name="childrenKey">
        /// The metadata key that contains the children or <c>null</c> to flatten documents in all metadata keys.
        /// This parameter has no effect if <paramref name="flatten"/> is <c>false</c>.
        /// </param>
        /// <returns>The documents that match the globbing pattern(s).</returns>
        public static FilteredDocumentList<TDocument> FilterDestinations<TDocument>(
            this IEnumerable<TDocument> documents,
            IEnumerable<string> patterns,
            bool flatten = true,
            string childrenKey = Keys.Children)
            where TDocument : IDocument
        {
            documents.ThrowIfNull(nameof(documents));

            DocumentFileProvider fileProvider = new DocumentFileProvider((IEnumerable<IDocument>)documents, false, flatten, childrenKey);
            IEnumerable<IFile> matches = Globber.GetFiles(fileProvider.GetDirectory("/"), patterns);
            return new FilteredDocumentList<TDocument>(
                matches
                    .Select(x => x.Path)
                    .Distinct()
                    .Select(match => fileProvider.GetDocument(match))
                    .Cast<TDocument>()
                    .OrderByDescending(x => x.Timestamp),
                x => x.Destination,
                (docs, patterns) => docs.FilterDestinations(patterns));
        }

        public static TDocument FirstOrDefaultSource<TDocument>(
            this IEnumerable<TDocument> documents,
            IEnumerable<string> patterns,
            bool flatten = true,
            string childrenKey = Keys.Children)
            where TDocument : IDocument =>
            documents.FilterSources(patterns, flatten, childrenKey).FirstOrDefault();

        public static TDocument FirstOrDefaultSource<TDocument>(this IEnumerable<TDocument> documents, params string[] patterns)
            where TDocument : IDocument =>
            documents.FirstOrDefaultSource((IEnumerable<string>)patterns);

        public static TDocument FirstOrDefaultDestination<TDocument>(
            this IEnumerable<TDocument> documents,
            IEnumerable<string> patterns,
            bool flatten = true,
            string childrenKey = Keys.Children)
            where TDocument : IDocument =>
            documents.FilterDestinations(patterns, flatten, childrenKey).FirstOrDefault();

        public static TDocument FirstOrDefaultDestination<TDocument>(this IEnumerable<TDocument> documents, params string[] patterns)
            where TDocument : IDocument =>
            documents.FirstOrDefaultDestination((IEnumerable<string>)patterns);

        /// <summary>
        /// Determines whether a document is contained in a document collection by ID.
        /// </summary>
        /// <remarks>
        /// Note that the document ID my get "out of sync" when caching is being used. For example, if comparing
        /// the ID of documents read from disk on a subsequent execution against the same documents cached from
        /// an earlier execution, the IDs won't match even if the document is the same.
        /// </remarks>
        /// <param name="documents">The documents to check.</param>
        /// <param name="document">The document to check for.</param>
        /// <returns><c>true</c> if the document is contained in the collection, <c>false</c> otherwise.</returns>
        public static bool ContainsById(this IEnumerable<IDocument> documents, IDocument document) =>
            documents.Contains(document, DocumentIdComparer.Instance);

        /// <summary>
        /// Determines whether a document is contained in a document collection by source.
        /// </summary>
        /// <param name="documents">The documents to check.</param>
        /// <param name="document">The document to check for.</param>
        /// <returns><c>true</c> if the document is contained in the collection, <c>false</c> otherwise.</returns>
        public static bool ContainsBySource(this IEnumerable<IDocument> documents, IDocument document) =>
            documents.Contains(document, DocumentSourceComparer.Instance);

        /// <summary>
        /// Flattens a tree structure.
        /// </summary>
        /// <remarks>
        /// This extension will either get all descendants of all documents from
        /// a given metadata key (<see cref="Keys.Children"/> by default) or all
        /// descendants from all metadata if a <c>null</c> key is specified. The
        /// result also includes the initial documents in both cases.
        /// </remarks>
        /// <remarks>
        /// The documents will be returned in no particular order and only distinct
        /// documents will be returned (I.e., if a document exists as a
        /// child of more than one parent, it will only appear once in the result set).
        /// </remarks>
        /// <param name="documents">The documents to flatten.</param>
        /// <param name="childrenKey">The metadata key that contains the children or <c>null</c> to flatten documents in all metadata keys.</param>
        /// <returns>The flattened documents.</returns>
        public static DocumentList<TDocument> Flatten<TDocument>(this IEnumerable<TDocument> documents, string childrenKey = Keys.Children)
            where TDocument : IDocument =>
            documents.Flatten(false, childrenKey);

        /// <summary>
        /// Flattens a tree structure.
        /// </summary>
        /// <remarks>
        /// This extension will either get all descendants of all documents from
        /// a given metadata key (<see cref="Keys.Children"/> by default) or all
        /// descendants from all metadata if a <c>null</c> key is specified. The
        /// result also includes the initial documents in both cases.
        /// </remarks>
        /// <remarks>
        /// The documents will be returned in no particular order and only distinct
        /// documents will be returned (I.e., if a document exists as a
        /// child of more than one parent, it will only appear once in the result set).
        /// </remarks>
        /// <param name="documents">The documents to flatten.</param>
        /// <param name="removeTreePlaceholders"><c>true</c> to filter out documents with the <see cref="Keys.TreePlaceholder"/> metadata.</param>
        /// <param name="childrenKey">The metadata key that contains the children or <c>null</c> to flatten documents in all metadata keys.</param>
        /// <returns>The flattened documents.</returns>
        public static DocumentList<TDocument> Flatten<TDocument>(this IEnumerable<TDocument> documents, bool removeTreePlaceholders, string childrenKey = Keys.Children)
            where TDocument : IDocument =>
            documents.Flatten(removeTreePlaceholders ? Keys.TreePlaceholder : null, childrenKey);

        /// <summary>
        /// Flattens a tree structure.
        /// </summary>
        /// <remarks>
        /// This extension will either get all descendants of all documents from
        /// a given metadata key (<see cref="Keys.Children"/> by default) or all
        /// descendants from all metadata if a <c>null</c> key is specified. The
        /// result also includes the initial documents in both cases.
        /// </remarks>
        /// <remarks>
        /// The documents will be returned in no particular order and only distinct
        /// documents will be returned (I.e., if a document exists as a
        /// child of more than one parent, it will only appear once in the result set).
        /// </remarks>
        /// <param name="documents">The documents to flatten.</param>
        /// <param name="treePlaceholderKey">
        /// The metadata key that identifies placeholder documents (<see cref="Keys.TreePlaceholder"/> by default).
        /// If <c>null</c>, tree placeholders will not be removed.
        /// </param>
        /// <param name="childrenKey">The metadata key that contains the children or <c>null</c> to flatten documents in all metadata keys.</param>
        /// <returns>The flattened documents.</returns>
        public static DocumentList<TDocument> Flatten<TDocument>(this IEnumerable<TDocument> documents, string treePlaceholderKey, string childrenKey = Keys.Children)
            where TDocument : IDocument
        {
            documents.ThrowIfNull(nameof(documents));

            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<TDocument> stack = new Stack<TDocument>(documents);
            HashSet<TDocument> results = new HashSet<TDocument>();
            while (stack.Count > 0)
            {
                TDocument current = stack.Pop();

                // Only process if we haven't already processed this document
                if (results.Add(current))
                {
                    IEnumerable<TDocument> children = childrenKey is null
                        ? current.SelectMany(x => current.GetDocumentList<TDocument>(x.Key))
                        : current.GetDocumentList<TDocument>(childrenKey);
                    if (children is object)
                    {
                        foreach (TDocument child in children.Where(x => x is object))
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
            return treePlaceholderKey is null
                ? results.ToDocumentList()
                : results.RemoveTreePlaceholders(treePlaceholderKey).ToDocumentList();
        }

        /// <summary>
        /// Removes tree placeholder documents (this method will not flatten a tree).
        /// </summary>
        /// <param name="documents">
        /// The documents from which to remove the placeholder documents.
        /// </param>
        /// <param name="treePlaceholderKey">
        /// The metadata key that identifies placeholder documents (<see cref="Keys.TreePlaceholder"/> by default).
        /// </param>
        /// <returns>The documents without placeholder documents.</returns>
        public static IEnumerable<TDocument> RemoveTreePlaceholders<TDocument>(this IEnumerable<TDocument> documents, string treePlaceholderKey = Keys.TreePlaceholder)
            where TDocument : IDocument =>
            documents.Where(x => !x.GetBool(treePlaceholderKey));

        public static DocumentMetadataTree<TDocument> AsMetadataTree<TDocument>(this IEnumerable<TDocument> documents, string childrenKey = Keys.Children)
            where TDocument : IDocument =>
            new DocumentMetadataTree<TDocument>(documents, childrenKey);

        // Cache the destination and source trees since creating them is a bit expensive
        private static readonly ConcurrentCache<(int, Type), object> DestinationTrees = new ConcurrentCache<(int, Type), object>(true);
        private static readonly ConcurrentCache<(int, Type), object> SourceTrees = new ConcurrentCache<(int, Type), object>(true);

        public static DocumentPathTree<TDocument> AsDestinationTree<TDocument>(this IEnumerable<TDocument> documents)
            where TDocument : IDocument =>
            (DocumentPathTree<TDocument>)DestinationTrees.GetOrAdd(
                (documents.GetHashCode(), typeof(TDocument)),
                (_, d) => new DocumentPathTree<TDocument>(d, x => x.Destination),
                documents);

        public static DocumentPathTree<TDocument> AsSourceTree<TDocument>(this IEnumerable<TDocument> documents)
            where TDocument : IDocument =>
            (DocumentPathTree<TDocument>)SourceTrees.GetOrAdd(
                (documents.GetHashCode(), typeof(TDocument)),
                (_, d) => new DocumentPathTree<TDocument>(d, x => x.Source),
                documents);
    }
}