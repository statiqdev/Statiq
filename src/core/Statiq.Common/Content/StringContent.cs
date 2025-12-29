using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A content provider for strings.
    /// </summary>
    public class StringContent : IContentProvider
    {
        private readonly string _content;
        private readonly object _hashCodeLock = new object();
        private int _hashCode;

        public StringContent(string content)
            : this(content, null)
        {
        }

        public StringContent(string content, string mediaType)
        {
            _content = content;
            MediaType = mediaType;
        }

        /// <inheritdoc />
        public Stream GetStream() => _content is null ? Stream.Null : new StringStream(_content);

        /// <inheritdoc />
        public TextReader GetTextReader() => _content is null ? TextReader.Null : new StringReader(_content);

        /// <inheritdoc />
        public long GetLength() => Encoding.Default.GetByteCount(_content);

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            new StringContent(_content, mediaType);

        /// <inheritdoc />
        public Task<int> GetCacheCodeAsync()
        {
            // Cache the hash code since the string can never change
            lock (_hashCodeLock)
            {
                if (_hashCode == default)
                {
                    _hashCode = (int)Crc32.Calculate(_content);
                }
                return Task.FromResult(_hashCode);
            }
        }
    }
}