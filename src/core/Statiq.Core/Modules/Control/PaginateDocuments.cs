using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Splits a sequence of documents into multiple pages.
    /// </summary>
    /// <remarks>
    /// This module forms pages from the input documents.
    /// Note that if there are no documents to paginate, this module will still
    /// output an empty page without any documents inside the page.
    /// </remarks>
    /// <metadata cref="Keys.Children" usage="Output" />
    /// <metadata cref="Keys.Next" usage="Output" />
    /// <metadata cref="Keys.Previous" usage="Output" />
    /// <metadata cref="Keys.Index" usage="Output" />
    /// <category name="Control" />
    public class PaginateDocuments : SyncModule
    {
        private readonly int _pageSize;
        private int _takePages = int.MaxValue;
        private int _skipPages = 0;
        private NormalizedPath _source;

        /// <summary>
        /// Partitions the result of the input documents into the specified number of pages.
        /// </summary>
        /// <param name="pageSize">The number of documents on each page.</param>
        public PaginateDocuments(int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentException(nameof(pageSize));
            }

            _pageSize = pageSize;
        }

        /// <summary>
        /// Only outputs a specific number of pages.
        /// </summary>
        /// <param name="count">The number of pages to output.</param>
        /// <returns>The current module instance.</returns>
        public PaginateDocuments TakePages(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException(nameof(count));
            }

            _takePages = count;
            return this;
        }

        /// <summary>
        /// Skips a specified number of pages before outputting pages.
        /// </summary>
        /// <param name="count">The number of pages to skip.</param>
        /// <returns>The current module instance.</returns>
        public PaginateDocuments SkipPages(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentException(nameof(count));
            }

            _skipPages = count;
            return this;
        }

        /// <summary>
        /// Sets the source (and destination) of the output document(s).
        /// </summary>
        /// <param name="source">The source to set for the output document(s).</param>
        /// <returns>The current module instance.</returns>
        public PaginateDocuments WithSource(in NormalizedPath source)
        {
            _source = source;
            return this;
        }

        /// <inheritdoc />
        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context)
        {
            // Partition the pages and get a total before skip/take
            IDocument[][] pages =
                Partition(context.Inputs, _pageSize)
                .ToArray();

            // Skip/take the pages
            pages = pages
                .Skip(_skipPages)
                .Take(_takePages)
                .ToArray();

            // Special case for no pages, create an empty one
            if (pages.Length == 0)
            {
                pages = new[] { Array.Empty<IDocument>() };
            }

            // Create the documents per page, setting previous and next values as we go
            Stack<(IDocument, LazyDocumentMetadataValue)> results = new Stack<(IDocument, LazyDocumentMetadataValue)>();
            for (int c = 0; c < pages.Length; c++)
            {
                MetadataItems items = new MetadataItems
                {
                    { Keys.Children, pages[c] },
                    { Keys.Index, c + 1 },
                    { Keys.TotalPages, pages.Length },
                    { Keys.TotalItems, context.Inputs.Length }
                };
                if (results.Count > 0)
                {
                    items.Add(Keys.Previous, new LazyDocumentMetadataValue(results.Peek().Item1));
                }
                LazyDocumentMetadataValue next = null;
                if (c < pages.Length - 1)
                {
                    next = new LazyDocumentMetadataValue();
                    items.Add(Keys.Next, next);
                }
                IDocument document = context.CreateDocument(
                    _source,
                    _source.IsNull ? _source : _source.GetRelativeInputPath(),
                    items);
                if (results.Count > 0)
                {
                    results.Peek().Item2.OriginalDocument = document;
                }
                results.Push((document, next));
            }

            return results.Select(x => x.Item1).Reverse();
        }

        // Interesting discussion of partitioning at
        // http://stackoverflow.com/questions/419019/split-list-into-sublists-with-linq
        // Note that this implementation won't work for very long sequences because it enumerates twice per chunk
        private static IEnumerable<T[]> Partition<T>(IReadOnlyList<T> source, int size)
        {
            int pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(size).ToArray();
                pos += size;
            }
        }
    }
}