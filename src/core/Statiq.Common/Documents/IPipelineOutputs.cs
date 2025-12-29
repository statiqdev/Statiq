using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// Contains a collection of documents output by pipelines.
    /// </summary>
    public interface IPipelineOutputs : IEnumerable<IDocument>, IDocumentPathTree<IDocument>
    {
        /// <summary>
        /// Gets documents by pipeline.
        /// </summary>
        /// <returns>All documents output by each pipeline.</returns>
        IReadOnlyDictionary<string, DocumentList<IDocument>> ByPipeline();

        /// <summary>
        /// Gets documents from a specific pipeline in their natural output order from the pipeline.
        /// </summary>
        /// <param name="pipelineName">The pipeline.</param>
        /// <returns>The documents output by the specified pipeline.</returns>
        DocumentList<IDocument> FromPipeline(string pipelineName);

        /// <summary>
        /// Gets all documents output by every pipeline except those from the
        /// specified pipeline in their natural output order.
        /// </summary>
        /// <param name="pipelineName">The pipeline.</param>
        /// <returns>All documents output by every pipeline except the specified one.</returns>
        DocumentList<IDocument> ExceptPipeline(string pipelineName);

        /// <summary>
        /// Returns documents with destination paths from all pipelines that satisfy the globbing pattern(s),
        /// ordering documents in descending order of their timestamp
        /// (I.e. the most recently created documents are returned first).
        /// </summary>
        /// <param name="destinationPatterns">The globbing pattern(s) to filter by (can be a single path).</param>
        /// <returns>The documents that satisfy the pattern or <c>null</c>.</returns>
        public FilteredDocumentList<IDocument> this[params string[] destinationPatterns] => this.FilterDestinations(destinationPatterns);

        /// <summary>
        /// Gets the first document in the list with the given destination path.
        /// </summary>
        /// <param name="destinationPath">The destination path of the document to get.</param>
        /// <returns>The first matching document or <c>null</c> if no document contains the given destination path.</returns>
        IDocument GetDestination(NormalizedPath destinationPath) =>
            this.FirstOrDefault(x => x.Destination.Equals(destinationPath));

        /// <summary>
        /// Gets the first document in the list with the given source path (note that source paths are generally absolute).
        /// </summary>
        /// <param name="sourcePath">The source path of the document to get.</param>
        /// <returns>The first matching document or <c>null</c> if no document contains the given source path.</returns>
        IDocument GetSource(NormalizedPath sourcePath) =>
            this.FirstOrDefault(x => x.Source.Equals(sourcePath));

        /// <summary>
        /// Gets the first document in the list with the given relative source path
        /// (since source paths are generally absolute, this tests against the source path relative to it's input path).
        /// </summary>
        /// <param name="relativeSourcePath">The relative source path of the document to get.</param>
        /// <returns>The first matching document or <c>null</c> if no document contains the given relative source path.</returns>
        IDocument GetRelativeSource(NormalizedPath relativeSourcePath) =>
            this.FirstOrDefault(x => !x.Source.IsNull && x.Source.GetRelativeInputPath().Equals(relativeSourcePath));

        // IDocumentPathTree implementation - get a fresh tree on each call since the outputs change

        IDocument IDocumentPathTree<IDocument>.Get(NormalizedPath destinationPath) => GetDestination(destinationPath);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetAncestorsOf(IDocument document, bool includeSelf) =>
            this.AsDestinationTree().GetAncestorsOf(document, includeSelf);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetChildrenOf(IDocument document) =>
            this.AsDestinationTree().GetChildrenOf(document);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetDescendantsOf(IDocument document, bool includeSelf) =>
            this.AsDestinationTree().GetDescendantsOf(document, includeSelf);

        IDocument IDocumentTree<IDocument>.GetParentOf(IDocument document) =>
            this.AsDestinationTree().GetParentOf(document);

        DocumentList<IDocument> IDocumentTree<IDocument>.GetSiblingsOf(IDocument document, bool includeSelf) =>
            this.AsDestinationTree().GetSiblingsOf(document, includeSelf);
    }
}