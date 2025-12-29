using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A content provider that uses a delegate to get the stream.
    /// </summary>
    public class DelegateContent : IContentProvider
    {
        private readonly Func<Stream> _getStream;

        public DelegateContent(Func<Stream> getStream)
            : this(getStream, null)
        {
        }

        public DelegateContent(Func<Stream> getStream, string mediaType)
        {
            _getStream = getStream.ThrowIfNull(nameof(getStream));
        }

        /// <inheritdoc />
        public string MediaType { get; }

        public IContentProvider CloneWithMediaType(string mediaType) => new DelegateContent(_getStream, mediaType);

        /// <inheritdoc />
        public async Task<int> GetCacheCodeAsync()
        {
            // The stream might have changed so we can't cache the hash code, get it fresh each time
            using (Stream stream = GetStream())
            {
                return (int)await Crc32.CalculateAsync(stream);
            }
        }

        /// <inheritdoc />
        public Stream GetStream() => _getStream() ?? Stream.Null;

        /// <inheritdoc />
        public TextReader GetTextReader() => new StreamReader(GetStream());

        /// <inheritdoc />
        public long GetLength()
        {
            using (Stream stream = GetStream())
            {
                if (stream.CanSeek)
                {
                    return stream.Length;
                }

                // If the stream isn't seekable the only way to get length is to copy it out and see how long it is
                using (MemoryStream buffer = IExecutionState.Current.MemoryStreamFactory.GetStream())
                {
                    stream.CopyTo(buffer);
                    return buffer.Length;
                }
            }
        }
    }
}