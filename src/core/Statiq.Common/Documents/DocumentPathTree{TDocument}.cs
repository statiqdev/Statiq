using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NetFabric.Hyperlinq;

namespace Statiq.Common
{
    public class DocumentPathTree<TDocument> : IDocumentPathTree<TDocument>
        where TDocument : IDocument
    {
        // Global cache
        private static readonly ConcurrentCache<IEnumerable<TDocument>, ConcurrentDictionary<TDocument, CachedResults>> _resultsCache
                = new ConcurrentCache<IEnumerable<TDocument>, ConcurrentDictionary<TDocument, CachedResults>>(true);

        private class CachedResults
        {
            public DocumentList<TDocument> Children { get; set; }
            public DocumentList<TDocument> Siblings { get; set; }
            public DocumentList<TDocument> SiblingsAndSelf { get; set; }
            public DocumentList<TDocument> Descendants { get; set; }
            public DocumentList<TDocument> DescendantsAndSelf { get; set; }
            public DocumentList<TDocument> Ancestors { get; set; }
            public DocumentList<TDocument> AncestorsAndSelf { get; set; }
        }

        private readonly ConcurrentDictionary<TDocument, CachedResults> _cachedResults;

        // Cache the documents by their path for faster lookups
        private readonly Dictionary<NormalizedPath, TDocument> _documentsByPath = new Dictionary<NormalizedPath, TDocument>();

        private readonly Func<TDocument, NormalizedPath> _pathFunc;
        private readonly string _indexFileName;
        private readonly (NormalizedPath, TDocument)[] _documents;

        public DocumentPathTree(IEnumerable<TDocument> documents, Func<TDocument, NormalizedPath> pathFunc)
            : this(documents, pathFunc, IExecutionContext.Current.Settings.GetIndexFileName())
        {
        }

        public DocumentPathTree(IEnumerable<TDocument> documents, Func<TDocument, NormalizedPath> pathFunc, string indexFileName)
        {
            _pathFunc = pathFunc.ThrowIfNull(nameof(pathFunc));
            _indexFileName = indexFileName.ThrowIfNullOrEmpty(nameof(indexFileName));

            _cachedResults = _resultsCache.GetOrAdd(
                documents,
                _ => new ConcurrentDictionary<TDocument, CachedResults>(DocumentIdComparer<TDocument>.Instance));

            // Add them by hand instead of using .Distinct() so we can guarantee ordering
            // and that earlier documents win (pipeline outputs are ordered with later phase results first)
            HashSet<Guid> documentIdHashes = new HashSet<Guid>();
            _documents = documents is null
                ? Array.Empty<(NormalizedPath, TDocument)>()
                : documents
                    .AsValueEnumerable()
                    .Where(x => documentIdHashes.Add(x.Id))
                    .Select(x => (ResolvePath(x), x))
                    .Where(x =>
                    {
                        if (!x.Item1.IsNull)
                        {
                            // Cache the path if we're saving this one
                            // Do it in the Where clause so we don't have to loop again
                            // Use TryAdd so behavior matches FirstOrDefault on the array of documents if there are more than one
                            _documentsByPath.TryAdd(x.Item1, x.x);
                            return true;
                        }
                        return false;
                    })
                    .ToArray();
        }

        private NormalizedPath ResolvePath(TDocument document)
        {
            NormalizedPath path = _pathFunc(document);
            if (path.IsNullOrEmpty)
            {
                return default;
            }

            // Treat an index file as if it were the directory
            if (path.FileName.Equals(_indexFileName))
            {
                path = path.Parent;
                if (path.IsNull)
                {
                    return default;
                }
            }

            return path;
        }

        public TDocument Get(NormalizedPath path)
        {
            foreach ((NormalizedPath, TDocument) item in _documents)
            {
                if (_pathFunc(item.Item2).Equals(path))
                {
                    return item.Item2;
                }
            }

            return default;
        }

        public TDocument GetParentOf(TDocument document)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNullOrEmpty)
            {
                return default;
            }
            return _documentsByPath.TryGetValue(path.Parent, out TDocument parent) ? parent : default;
        }

        public DocumentList<TDocument> GetChildrenOf(TDocument document)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNull)
            {
                return DocumentList<TDocument>.Empty;
            }

            CachedResults results = _cachedResults.GetOrAdd(document, _ => new CachedResults());

            return results.Children ?? (results.Children = _documents
                .AsValueEnumerable()
                .Where(x => path.ContainsChild(x.Item1) && !x.Item2.IdEquals(document))
                .Select(x => x.Item2)
                .ToArray()
                .ToDocumentList());
        }

        public DocumentList<TDocument> GetSiblingsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNullOrEmpty)
            {
                return DocumentList<TDocument>.Empty;
            }

            CachedResults results = _cachedResults.GetOrAdd(document, _ => new CachedResults());
            if (includeSelf)
            {
                return results.SiblingsAndSelf ?? (results.SiblingsAndSelf = _documents
                    .AsValueEnumerable()
                    .Where(x => !x.Item1.IsNullOrEmpty && path.IsSiblingOrSelf(x.Item1))
                    .Select(x => x.Item2)
                    .ToArray()
                    .ToDocumentList());
            }
            return results.Siblings ?? (results.Siblings = _documents
                .AsValueEnumerable()
                .Where(x => !x.Item1.IsNullOrEmpty && path.IsSiblingOrSelf(x.Item1) && !x.Item2.IdEquals(document))
                .Select(x => x.Item2)
                .ToArray()
                .ToDocumentList());
        }

        public DocumentList<TDocument> GetDescendantsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNull)
            {
                return DocumentList<TDocument>.Empty;
            }

            CachedResults results = _cachedResults.GetOrAdd(document, _ => new CachedResults());

            if (includeSelf)
            {
                return results.DescendantsAndSelf ?? (results.DescendantsAndSelf = _documents
                    .AsValueEnumerable()
                    .Where(x => path.ContainsDescendantOrSelf(x.Item1))
                    .Select(x => x.Item2)
                    .ToArray()
                    .ToDocumentList());
            }
            return results.Descendants ?? (results.Descendants = _documents
                .AsValueEnumerable()
                .Where(x => path.ContainsDescendantOrSelf(x.Item1) && !x.Item2.IdEquals(document))
                .Select(x => x.Item2)
                .ToArray()
                .ToDocumentList());
        }

        public DocumentList<TDocument> GetAncestorsOf(TDocument document, bool includeSelf)
        {
            document.ThrowIfNull(nameof(document));

            NormalizedPath path = ResolvePath(document);
            if (path.IsNull)
            {
                return DocumentList<TDocument>.Empty;
            }

            CachedResults results = _cachedResults.GetOrAdd(document, _ => new CachedResults());
            if (includeSelf)
            {
                return results.AncestorsAndSelf ?? (results.AncestorsAndSelf = GetAncestorsOf());
            }
            return results.Ancestors ?? (results.Ancestors = GetAncestorsOf());

            DocumentList<TDocument> GetAncestorsOf()
            {
                // We want to build the ancestors up from closest to furthest
                List<TDocument> ancestors = new List<TDocument>();
                if (includeSelf)
                {
                    ancestors.Add(document);
                }
                path = path.Parent;
                while (!path.IsNull)
                {
                    if (_documentsByPath.TryGetValue(path, out TDocument parent) && !parent.IdEquals(document))
                    {
                        ancestors.Add(parent);
                    }
                    if (path.IsNullOrEmpty)
                    {
                        break;
                    }
                    path = path.Parent;
                }
                return ancestors.ToDocumentList();
            }
        }
    }
}