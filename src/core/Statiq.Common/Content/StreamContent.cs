using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A content provider that wraps a seekable stream.
    /// </summary>
    public class StreamContent : LockingStreamWrapper, IContentProvider
    {
        public StreamContent(Stream stream)
            : this(stream, null)
        {
        }

        public StreamContent(Stream stream, string mediaType)
            : base(stream, false)
        {
            MediaType = mediaType;
        }

        /// <inheritdoc />
        public TextReader GetTextReader() => Stream is null ? TextReader.Null : new StreamReader(GetStream());

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            new StreamContent(Stream, mediaType);

        /// <inheritdoc />
        public async Task<int> GetCacheCodeAsync()
        {
            // The stream might have changed so we can't cache the hash code, get it fresh each time
            using (Stream stream = GetStream())
            {
                return (int)await Crc32.CalculateAsync(stream);
            }
        }
    }
}