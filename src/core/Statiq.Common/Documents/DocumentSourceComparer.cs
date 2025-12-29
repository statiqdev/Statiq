using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A simple <see cref="IEqualityComparer{IDocument}"/> that compares documents by <see cref="IDocument.Source"/>.
    /// </summary>
    public class DocumentSourceComparer : IEqualityComparer<IDocument>
    {
        public static DocumentSourceComparer Instance { get; } = new DocumentSourceComparer();

        public bool Equals(IDocument x, IDocument y) =>
            (x is null && y is null) || (x is object && y is object && x.Source.Equals(y.Source));

        public int GetHashCode(IDocument obj) => obj?.Source.GetHashCode() ?? 0;
    }
}