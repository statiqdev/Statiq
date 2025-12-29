using System.IO;

namespace Statiq.Common
{
    /// <summary>
    /// Provides pooled memory streams (via the RecyclableMemoryStream library).
    /// </summary>
    public interface IMemoryStreamFactory
    {
        /// <summary>
        /// Retrieve a new <see cref="MemoryStream"/> object with a default initial capacity.
        /// </summary>
        /// <returns>A <see cref="MemoryStream"/>.</returns>
        MemoryStream GetStream();

        /// <summary>
        /// Retrieve a new <see cref="MemoryStream"/> object with at least the given capacity.
        /// </summary>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <returns>A <see cref="MemoryStream"/>.</returns>
        MemoryStream GetStream(int requiredSize);

        /// <summary>
        /// Retrieve a new <see cref="MemoryStream"/> object with at least the given capacity, possibly using
        /// a single continugous underlying buffer.
        /// </summary>
        /// <remarks>Retrieving a MemoryStream which provides a single contiguous buffer can be useful in situations
        /// where the initial size is known and it is desirable to avoid copying data between the smaller underlying
        /// buffers to a single large one. This is most helpful when you know that you will always call GetBuffer
        /// on the underlying stream.</remarks>
        /// <param name="requiredSize">The minimum desired capacity for the stream.</param>
        /// <param name="asContiguousBuffer">Whether to attempt to use a single contiguous buffer.</param>
        /// <returns>A <see cref="MemoryStream"/>.</returns>
        MemoryStream GetStream(int requiredSize, bool asContiguousBuffer);

        /// <summary>
        /// Retrieve a new <see cref="MemoryStream"/> object with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <param name="offset">The offset from the start of the buffer to copy from.</param>
        /// <param name="count">The number of bytes to copy from the buffer.</param>
        /// <returns>A <see cref="MemoryStream"/>.</returns>
        MemoryStream GetStream(byte[] buffer, int offset, int count);

        /// <summary>
        /// Retrieve a new <see cref="MemoryStream"/> object with the provided string encoded as UTF8.
        /// </summary>
        /// <param name="content">The string to encode and store in the memory stream.</param>
        /// <returns>A <see cref="MemoryStream"/>.</returns>
        MemoryStream GetStream(string content);
    }
}
