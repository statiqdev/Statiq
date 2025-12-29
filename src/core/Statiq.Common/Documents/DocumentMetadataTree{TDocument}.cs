using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    public class DocumentMetadataTree<TDocument> : IDocumentTree<TDocument>
        where TDocument : IDocument
    {
        private readonly DocumentList<TDocument> _documents;
        private readonly string _childrenKey;

        public DocumentMetadataTree()
            : this(null, null)
        {
        }

        public DocumentMetadataTree(string childrenKey)
            : this(null, childrenKey)
        {
        }

        public DocumentMetadataTree(IEnumerable<TDocument> documents)
            : this(documents, null)
        {
        }

        public DocumentMetadataTree(IEnumerable<TDocument> documents, string childrenKey)
        {
            _documents = documents?.ToDocumentList() ?? DocumentList<TDocument>.Empty;
            _childrenKey = childrenKey ?? Keys.Children;
        }

        public TDocument GetParentOf(TDocument document)
        {
            document.ThrowIfNull(nameof(document));
            return GetParentOf(document, _documents);
        }

        private TDocument GetParentOf(TDocument document, IEnumerable<TDocument> parents)
        {
            TDocument parent = parents.FirstOrDefault(x => GetChildrenOf(x).Contains(document));
            if (parent is null)
            {
                foreach (TDocument candidate in parents)
                {
                    parent = GetParentOf(document, GetChildrenOf(candidate));
                    if (parent is object)
                    {
                        break;
                    }
                }
            }
            return parent;
        }

        public DocumentList<TDocument> GetChildrenOf(TDocument document)
        {
            document.ThrowIfNull(nameof(document));
            return document.GetDocumentList<TDocument>(_childrenKey);
        }

        public DocumentList<TDocument> GetSiblingsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));
            TDocument parent = GetParentOf(document);
            if (parent is object)
            {
                DocumentList<TDocument> siblings = GetChildrenOf(parent);
                if (siblings?.Count > 0)
                {
                    return includeSelf ? siblings : siblings.Where(x => !x.IdEquals(document)).ToDocumentList();
                }
            }
            return DocumentList<TDocument>.Empty;
        }

        public DocumentList<TDocument> GetDescendantsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            List<TDocument> descendants = new List<TDocument>();

            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<TDocument> stack = new Stack<TDocument>();
            stack.Push(document);
            if (includeSelf)
            {
                descendants.Add(document);
            }

            // Depth-first iterate children
            while (stack.Count > 0)
            {
                foreach (TDocument child in GetChildrenOf(stack.Pop()))
                {
                    stack.Push(child);
                    descendants.Add(child);
                }
            }

            return descendants.ToDocumentList();
        }

        public DocumentList<TDocument> GetAncestorsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            List<TDocument> ancestors = new List<TDocument>();
            if (includeSelf)
            {
                ancestors.Add(document);
            }
            TDocument parent = GetParentOf(document);
            while (parent is object)
            {
                ancestors.Add(parent);
                parent = GetParentOf(parent);
            }
            return ancestors.ToDocumentList();
        }
    }
}
