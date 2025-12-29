namespace Statiq.Common
{
    public interface IDocumentTree<TDocument>
        where TDocument : IDocument
    {
        TDocument GetParentOf(TDocument document);

        DocumentList<TDocument> GetChildrenOf(TDocument document);

        DocumentList<TDocument> GetSiblingsOf(TDocument document, bool includeSelf);

        DocumentList<TDocument> GetDescendantsOf(TDocument document, bool includeSelf);

        DocumentList<TDocument> GetAncestorsOf(TDocument document, bool includeSelf);
    }
}
