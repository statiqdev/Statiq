using System;
using System.Linq;

namespace Statiq.Common
{
    public static class IDocumentPathTreeExtensions
    {
        public static DocumentList<TDocument> GetAncestorsOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath,
            bool includeSelf)
            where TDocument : IDocument
        {
            tree.ThrowIfNull(nameof(tree));
            TDocument document = tree.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<TDocument>.Empty;
            }
            return tree.GetAncestorsOf(document, includeSelf);
        }

        public static DocumentList<TDocument> GetAncestorsOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath)
            where TDocument : IDocument =>
            tree.GetAncestorsOf(destinationPath, false);

        public static DocumentList<TDocument> GetChildrenOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath)
            where TDocument : IDocument
        {
            tree.ThrowIfNull(nameof(tree));
            TDocument document = tree.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<TDocument>.Empty;
            }
            return tree.GetChildrenOf(document);
        }

        public static DocumentList<TDocument> GetDescendantsOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath,
            bool includeSelf)
            where TDocument : IDocument
        {
            tree.ThrowIfNull(nameof(tree));
            TDocument document = tree.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<TDocument>.Empty;
            }
            return tree.GetDescendantsOf(document, includeSelf);
        }

        public static DocumentList<TDocument> GetDescendantsOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath)
            where TDocument : IDocument =>
            tree.GetDescendantsOf(destinationPath, false);

        public static TDocument GetParentOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath)
            where TDocument : IDocument
        {
            tree.ThrowIfNull(nameof(tree));
            TDocument document = tree.Get(destinationPath);
            if (document is null)
            {
                return default;
            }
            return tree.GetParentOf(document);
        }

        public static DocumentList<TDocument> GetSiblingsOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath,
            bool includeSelf)
            where TDocument : IDocument
        {
            tree.ThrowIfNull(nameof(tree));
            TDocument document = tree.Get(destinationPath);
            if (document is null)
            {
                return DocumentList<TDocument>.Empty;
            }
            return tree.GetSiblingsOf(document, includeSelf);
        }

        public static DocumentList<TDocument> GetSiblingsOf<TDocument>(
            this IDocumentPathTree<TDocument> tree,
            in NormalizedPath destinationPath)
            where TDocument : IDocument =>
            tree.GetSiblingsOf(destinationPath, false);
    }
}
