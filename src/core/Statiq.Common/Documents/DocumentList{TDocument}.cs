using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NetFabric.Hyperlinq;

namespace Statiq.Common
{
    /// <summary>
    /// A list of documents.
    /// </summary>
    /// <typeparam name="TDocument">The document type the list contains.</typeparam>
    public class DocumentList<TDocument> : IReadOnlyList<TDocument>
        where TDocument : IDocument
    {
        public static readonly DocumentList<TDocument> Empty = new DocumentList<TDocument>(null);

        private readonly IReadOnlyList<TDocument> _documents;

        public DocumentList(IEnumerable<TDocument> documents)
        {
            if (documents is null)
            {
                // If it's null create an empty list
                _documents = Array.Empty<TDocument>();
            }
            else if (documents is IReadOnlyList<TDocument> list && !list.Any(x => x is null))
            {
                // It's already a list and doesn't contain nulls
                _documents = list;
            }
            else
            {
                // Remove nulls
                _documents = documents.AsValueEnumerable().Where(x => x is object).ToArray();
            }
        }

        /// <summary>
        /// Returns documents with destination paths that satisfy the globbing pattern(s).
        /// </summary>
        /// <param name="destinationPatterns">The globbing pattern(s) to filter by (can be a single path).</param>
        /// <returns>The documents that satisfy the pattern or <c>null</c>.</returns>
        public virtual FilteredDocumentList<TDocument> this[params string[] destinationPatterns] =>
            _documents.FilterDestinations(destinationPatterns);

        /// <summary>
        /// Gets the first document in the list with the given destination path.
        /// </summary>
        /// <param name="destinationPath">The destination path of the document to get.</param>
        /// <returns>The first matching document or <c>null</c> if no document contains the given destination path.</returns>
        public IDocument GetDestination(NormalizedPath destinationPath) =>
            _documents.FirstOrDefault(x => x.Destination.Equals(destinationPath));

        /// <summary>
        /// Gets the first document in the list with the given source path (note that source paths are generally absolute).
        /// </summary>
        /// <param name="sourcePath">The source path of the document to get.</param>
        /// <returns>The first matching document or <c>null</c> if no document contains the given source path.</returns>
        public IDocument GetSource(NormalizedPath sourcePath) =>
            _documents.FirstOrDefault(x => x.Source.Equals(sourcePath));

        /// <summary>
        /// Gets the first document in the list with the given relative source path
        /// (since source paths are generally absolute, this tests against the source path relative to it's input path).
        /// </summary>
        /// <param name="relativeSourcePath">The relative source path of the document to get.</param>
        /// <returns>The first matching document or <c>null</c> if no document contains the given relative source path.</returns>
        public IDocument GetRelativeSource(NormalizedPath relativeSourcePath) =>
            _documents.FirstOrDefault(x => !x.Source.IsNull && x.Source.GetRelativeInputPath().Equals(relativeSourcePath));

        // IReadOnlyList<IDocument>

        /// <inheritdoc />
        public TDocument this[int index] => _documents[index];

        /// <inheritdoc />
        public int Count => _documents.Count;

        /// <inheritdoc />
        public IEnumerator<TDocument> GetEnumerator() => _documents.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => _documents.GetEnumerator();
    }
}