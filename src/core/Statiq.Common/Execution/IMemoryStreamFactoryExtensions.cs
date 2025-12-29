using System.IO;

namespace Statiq.Common
{
    public static class IMemoryStreamFactoryExtensions
    {
        /// <summary>
        /// Retrieve a new <see cref="MemoryStream"/> object with contents copied from the provided
        /// buffer. The provided buffer is not wrapped or used after construction.
        /// </summary>
        /// <remarks>The new stream's position is set to the beginning of the stream when returned.</remarks>
        /// <param name="factory">The memory stream factory.</param>
        /// <param name="buffer">The byte buffer to copy data from.</param>
        /// <returns>A <see cref="MemoryStream"/>.</returns>
        public static MemoryStream GetStream(this IMemoryStreamFactory factory, byte[] buffer) => factory.GetStream(buffer, 0, buffer.Length);
    }
}
