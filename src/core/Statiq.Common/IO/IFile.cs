using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a file. Not all implementations will support all
    /// available methods and may throw <see cref="NotSupportedException"/>.
    /// </summary>
    // Initially based on code from Cake (http://cakebuild.net/)
    public interface IFile : IFileSystemEntry, IContentProviderFactory, ICacheCode
    {
        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        /// <value>The path.</value>
        new NormalizedPath Path { get; }

        /// <summary>
        /// Gets the directory of the file.
        /// </summary>
        /// <value>
        /// The directory of the file.
        /// </value>
        IDirectory Directory { get; }

        /// <summary>
        /// Gets the length of the file.
        /// </summary>
        /// <value>The length of the file.</value>
        long Length { get; }

        /// <summary>
        /// Gets the media type of the file.
        /// </summary>
        /// <remarks>
        /// A registered IANA media type will be used if available.
        /// Unregistered media type names may be returned if a registered type is unavailable.
        /// If the media type is unknown this may be null or empty.
        /// </remarks>
        string MediaType { get; }

        /// <summary>
        /// Deletes the file.
        /// </summary>
        void Delete();

        /// <summary>
        /// Reads all text from the file.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>All text in the file.</returns>
        Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes the specified text to a file.
        /// </summary>
        /// <param name="contents">The text to write.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads all bytes from the file.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>All bytes in the file.</returns>
        Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes the specified bytes to a file.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        Task WriteAllBytesAsync(byte[] bytes, bool createDirectory = true, CancellationToken cancellationToken = default);

        /// <summary>
        /// Opens the file for reading. If it does not exist, an exception will be thrown.
        /// </summary>
        /// <returns>A <see cref="Stream"/> for the file.</returns>
        Stream OpenRead();

        /// <summary>
        /// Opens the file for reading as text. If it does not exist, an exception will be thrown.
        /// </summary>
        /// <returns>A <see cref="TextReader"/> for the file.</returns>
        TextReader OpenText();

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// You must manually call <see cref="Stream.SetLength(long)"/>
        /// when done to ensure previously existing data is truncated.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream OpenWrite(bool createDirectory = true);

        /// <summary>
        /// Opens the file for writing. This will either create the file
        /// if it doesn't exist or append to it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream OpenAppend(bool createDirectory = true);

        /// <summary>
        /// Opens the file for reading and writing. This will either create the file
        /// if it doesn't exist or overwrite it if it does.
        /// </summary>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <returns>The stream.</returns>
        Stream Open(bool createDirectory = true);

        /// <summary>
        /// Refreshes any cached information about the file.
        /// </summary>
        /// <remarks>
        /// For example, a file from the file system might use a <see cref="FileInfo"/> that needs to be refreshed when it changes. Note that implementations
        /// should internally refresh their state as appropriate so external refresh calls are only needed if the underlying file changes outside the file instance.
        /// </remarks>
        void Refresh();
    }
}
