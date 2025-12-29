using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a directory. Not all implementations will support all
    /// available methods and may throw <see cref="NotSupportedException"/>.
    /// </summary>
    public interface IDirectory : IFileSystemEntry
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Gets the path to the directory.
        /// </summary>
        /// <value>The path.</value>
        new NormalizedPath Path { get; }

        /// <summary>
        /// Gets the parent directory.
        /// </summary>
        /// <value>
        /// The parent directory or <c>null</c> if the directory is a root.
        /// </value>
        IDirectory Parent { get; }

        /// <summary>
        /// Creates the directory, including any necessary parent directories.
        /// </summary>
        void Create();

        /// <summary>
        /// Deletes the directory.
        /// </summary>
        /// <param name="recursive">Will perform a recursive delete if set to <c>true</c>.</param>
        void Delete(bool recursive);

        /// <summary>
        /// Moves the directory and it's contents to a new parent path.
        /// </summary>
        /// <param name="destinationPath">The parent path to move the directory to.</param>
        void MoveTo(NormalizedPath destinationPath);

        /// <summary>
        /// Moves the directory and it's contents to a new parent directory.
        /// </summary>
        /// <param name="destinationDirectory">The parent directory to move the directory to.</param>
        void MoveTo(IDirectory destinationDirectory);

        /// <summary>
        /// Gets directories matching the specified filter and scope.
        /// </summary>
        /// <param name="searchOption">
        /// Specifies whether the operation should include only
        /// the current directory or should include all subdirectories.
        /// </param>
        /// <returns>Directories matching the filter and scope.</returns>
        IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets files matching the specified filter and scope.
        /// </summary>
        /// <param name="searchOption">
        /// Specifies whether the operation should include only
        /// the current directory or should include all subdirectories.
        /// </param>
        /// <returns>Files matching the specified filter and scope.</returns>
        IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly);

        /// <summary>
        /// Gets a directory by combining it's path with the current directory's path.
        /// The specified directory path must be relative.
        /// </summary>
        /// <param name="directory">The path of the directory.</param>
        /// <returns>The directory.</returns>
        IDirectory GetDirectory(NormalizedPath directory);

        /// <summary>
        /// Gets a file by combining it's path with the current directory's path.
        /// The specified file path must be relative.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The file.</returns>
        IFile GetFile(NormalizedPath path);
    }
}