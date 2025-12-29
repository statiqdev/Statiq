using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// A filtered list of documents that also implements <see cref="IDocumentPathTree{TDocument}"/>
    /// so that the resulting documents can be easily traversed as a tree structure.
    /// </summary>
    /// <typeparam name="TDocument">The document type the list contains.</typeparam>
    public class FilteredDocumentList<TDocument> : DocumentList<TDocument>, IDocumentPathTree<TDocument>
        where TDocument : IDocument
    {
        private readonly DocumentPathTree<TDocument> _tree;
        private readonly Func<IReadOnlyList<TDocument>, string[], FilteredDocumentList<TDocument>> _filterFunc;

        internal FilteredDocumentList(
            IEnumerable<TDocument> documents,
            Func<TDocument, NormalizedPath> pathFunc,
            Func<IReadOnlyList<TDocument>, string[], FilteredDocumentList<TDocument>> filterFunc)
            : base(documents)
        {
            _tree = new DocumentPathTree<TDocument>(documents, pathFunc);
            _filterFunc = filterFunc.ThrowIfNull(nameof(filterFunc));
        }

        public override FilteredDocumentList<TDocument> this[params string[] patterns] => _filterFunc(this, patterns);

        public TDocument Get(NormalizedPath destinationPath) => _tree.Get(destinationPath);

        public DocumentList<TDocument> GetAncestorsOf(TDocument document, bool includeSelf) => _tree.GetAncestorsOf(document, includeSelf);

        public DocumentList<TDocument> GetChildrenOf(TDocument document) => _tree.GetChildrenOf(document);

        public DocumentList<TDocument> GetDescendantsOf(TDocument document, bool includeSelf) => _tree.GetDescendantsOf(document, includeSelf);

        public TDocument GetParentOf(TDocument document) => _tree.GetParentOf(document);

        public DocumentList<TDocument> GetSiblingsOf(TDocument document, bool includeSelf) => _tree.GetSiblingsOf(document, includeSelf);
    }
}