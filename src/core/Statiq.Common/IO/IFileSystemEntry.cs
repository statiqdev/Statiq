using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Represents an entry in the file system.
    /// </summary>
    public interface IFileSystemEntry : IDisplayable
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Gets the path to the entry.
        /// </summary>
        /// <value>The path.</value>
        NormalizedPath Path { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IFileSystemEntry"/> exists.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the entry exists; otherwise, <c>false</c>.
        /// </value>
        bool Exists { get; }

        /// <summary>
        /// Gets the time when the current entry was last written to.
        /// </summary>
        public DateTime LastWriteTime { get; }

        /// <summary>
        /// Gets the time when the current entry was created.
        /// </summary>
        public DateTime CreationTime { get; }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this file system entry.
        /// </summary>
        /// <returns>
        /// The full path of the file system entry.
        /// </returns>
        string ToString();
    }
}
