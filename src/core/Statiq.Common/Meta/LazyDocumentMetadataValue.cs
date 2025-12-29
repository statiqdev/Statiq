using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// Stores the ID of a document and attempts to lazily find it in
    /// the current input documents and their children when the value is requested.
    /// </summary>
    /// <remarks>
    /// This will descend child documents contained in the <see cref="Keys.Children"/>
    /// metadata. It will also return the first document result with a matching ID.
    /// That means if the original document this value points to was cloned, the
    /// first matching clone (of possibly many) will be returned by this metadata.
    /// </remarks>
    public class LazyDocumentMetadataValue : IMetadataValue
    {
        private static readonly object Lock = new object();

        // Cache the results for a given context
        private IExecutionContext _context;
        private IDocument _result;

        /// <summary>
        /// Creates an instance without an ID. This is helpful in situations where
        /// you need to create a set of documents first and then go back and fill
        /// in the IDs after all the documents are created (I.e. next and previous).
        /// </summary>
        public LazyDocumentMetadataValue()
        {
        }

        public LazyDocumentMetadataValue(IDocument originalDocument)
        {
            OriginalDocument = originalDocument;
        }

        public IDocument OriginalDocument { get; set; }

        public object Get(string key, IMetadata metadata)
        {
            lock (Lock)
            {
                IExecutionContext context = IExecutionContext.Current;
                if (context == _context)
                {
                    return _result;
                }

                // Try the current inputs first then crawl up parent contexts
                _context = context;
                _result = null;
                HashSet<IDocument> visited = new HashSet<IDocument>();
                while (context is object)
                {
                    _result = Find(context.Inputs, visited);
                    if (_result is object)
                    {
                        return _result;
                    }
                    context = context.Parent;
                }

                // Fall back to the original document
                _result = OriginalDocument;
                return _result;
            }
        }

        private IDocument Find(ImmutableArray<IDocument> documents, HashSet<IDocument> visited)
        {
            // Use a stack so we don't overflow the call stack with recursive calls for deep trees
            Stack<IDocument> stack = new Stack<IDocument>(documents);
            while (stack.Count > 0)
            {
                IDocument current = stack.Pop();

                // Only process if we haven't already processed this document
                if (visited.Add(current))
                {
                    if (current.IdEquals(OriginalDocument))
                    {
                        return current;
                    }
                    IEnumerable<IDocument> children = current.GetDocumentList(Keys.Children);
                    if (children is object)
                    {
                        foreach (IDocument child in children.Where(x => x is object))
                        {
                            stack.Push(child);
                        }
                    }
                }
            }
            return null;
        }
    }
}
