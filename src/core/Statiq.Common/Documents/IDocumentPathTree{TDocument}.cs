namespace Statiq.Common
{
    public interface IDocumentPathTree<TDocument> : IDocumentTree<TDocument>
        where TDocument : IDocument
    {
        TDocument Get(NormalizedPath destinationPath);
    }
}
