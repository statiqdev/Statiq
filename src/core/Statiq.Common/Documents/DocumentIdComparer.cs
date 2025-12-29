using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A simple <see cref="IEqualityComparer{IDocument}"/> that compares documents by <see cref="IDocument.Id"/>.
    /// </summary>
    public class DocumentIdComparer : IEqualityComparer<IDocument>
    {
        public static DocumentIdComparer Instance { get; } = new DocumentIdComparer();

        public bool Equals(IDocument x, IDocument y) => x?.Id == y?.Id;

        public int GetHashCode(IDocument obj) => obj?.Id.GetHashCode() ?? 0;
    }
}