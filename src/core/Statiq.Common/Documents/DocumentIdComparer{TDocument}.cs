using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A simple document comparer that compares documents by <see cref="IDocument.Id"/>.
    /// </summary>
    public class DocumentIdComparer<TDocument> : IEqualityComparer<TDocument>
        where TDocument : IDocument
    {
        public static DocumentIdComparer<TDocument> Instance { get; } = new DocumentIdComparer<TDocument>();

        public bool Equals(TDocument x, TDocument y) => x?.Id == y?.Id;

        public int GetHashCode(TDocument obj) => obj?.Id.GetHashCode() ?? 0;
    }
}