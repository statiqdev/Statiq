using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Compares <see cref="IFile"/> equality.
    /// </summary>
    public class FileEqualityComparer : IEqualityComparer<IFile>
    {
        public static FileEqualityComparer Default => new FileEqualityComparer();

        /// <inheritdoc />
        public bool Equals(IFile x, IFile y) => x.Path.Equals(y.Path);

        /// <inheritdoc />
        public int GetHashCode(IFile obj) => obj.Path.GetHashCode();
    }
}