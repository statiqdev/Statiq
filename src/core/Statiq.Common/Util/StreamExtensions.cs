using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Extension methods for use with <see cref="Stream"/>.
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Creates a <see cref="StreamWriter"/> for the specified stream. The
        /// biggest difference between this and creating a <see cref="StreamWriter"/>
        /// directly is that the new <see cref="StreamWriter"/> will default to
        /// leaving the underlying stream open on disposal. Remember to flush the
        /// returned writer after all data have been written.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the underlying stream open on disposal.</param>
        /// <returns>A new <see cref="StreamWriter"/> for the specified stream.</returns>
        public static StreamWriter GetWriter(this Stream stream, bool leaveOpen = true) =>
            new StreamWriter(stream, Encoding.Default, 1024, leaveOpen);

        /// <summary>
        /// Copies a <see cref="Stream"/> to a <see cref="TextWriter"/> using a buffer and leaves the stream
        /// open after copying.
        /// </summary>
        /// <param name="stream">The stream to copy from.</param>
        /// <param name="writer">The text writer to write stream content to.</param>
        /// <param name="bufferLength">The length of the buffer to populate for each block.</param>
        public static async Task CopyToAsync(this Stream stream, TextWriter writer, int bufferLength = 4096)
        {
            stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(stream, leaveOpen: true))
            {
                char[] buffer = new char[bufferLength];
                int read;
                do
                {
                    read = await reader.ReadBlockAsync(buffer, 0, bufferLength);
                    await writer.WriteAsync(buffer, 0, read);
                }
                while (read > 0);
            }
        }
    }
}