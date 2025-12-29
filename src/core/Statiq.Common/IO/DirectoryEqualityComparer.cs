using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Compares <see cref="IDirectory"/> equality.
    /// </summary>
    public class DirectoryEqualityComparer : IEqualityComparer<IDirectory>
    {
        public static DirectoryEqualityComparer Default => new DirectoryEqualityComparer();

        /// <inheritdoc />
        public bool Equals(IDirectory x, IDirectory y) => x.Path.Equals(y.Path);

        /// <inheritdoc />
        public int GetHashCode(IDirectory obj) => obj.Path.GetHashCode();
    }
}
