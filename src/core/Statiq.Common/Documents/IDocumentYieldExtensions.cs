using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IDocumentYieldExtensions
    {
        /// <summary>
        /// Returns a document enumerable given a single document. This is just a convenience
        /// method for converting a single document into an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="document">The document to return.</param>
        /// <returns>A document enumerable.</returns>
        public static IEnumerable<TDocument> Yield<TDocument>(this TDocument document)
            where TDocument : IDocument
        {
            yield return document;
        }

        /// <summary>
        /// Returns a document enumerable given a single document. This is just a convenience
        /// method for converting a single document into an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="document">The document to return.</param>
        /// <returns>A document enumerable.</returns>
        public static Task<IEnumerable<TDocument>> YieldAsync<TDocument>(this TDocument document)
            where TDocument : IDocument
        {
            return Task.FromResult(document.Yield());
        }
    }
}
