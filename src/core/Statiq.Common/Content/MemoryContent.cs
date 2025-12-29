using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A content provider for raw memory.
    /// </summary>
    public class MemoryContent : IContentProvider
    {
        private readonly byte[] _buffer;
        private readonly int _index;
        private readonly int _count;
        private readonly object _hashCodeLock = new object();
        private int _hashCode;

        public MemoryContent(byte[] buffer)
            : this(buffer, 0, buffer?.Length ?? 0, null)
        {
        }

        public MemoryContent(byte[] buffer, int index, int count)
            : this(buffer, index, count, null)
        {
        }

        public MemoryContent(byte[] buffer, string mediaType)
            : this(buffer, 0, buffer?.Length ?? 0, mediaType)
        {
        }

        public MemoryContent(byte[] buffer, int index, int count, string mediaType)
        {
            if (buffer is null && index != 0)
            {
                throw new ArgumentException($"{nameof(index)} cannot be specified for null buffer");
            }
            if (buffer is null && count != 0)
            {
                throw new ArgumentException($"{nameof(count)} cannot be specified for null buffer");
            }
            if (index > buffer.Length)
            {
                throw new ArgumentException($"{nameof(index)} is greater than buffer length");
            }
            if (index + count > buffer.Length)
            {
                throw new ArgumentException($"{nameof(index)} + {nameof(count)} is greater than buffer length");
            }

            _buffer = buffer;
            _index = index;
            _count = count;

            MediaType = mediaType;
        }

        /// <inheritdoc />
        public Stream GetStream() => _buffer is null ? Stream.Null : new MemoryStream(_buffer, _index, _count, false);

        /// <inheritdoc />
        public TextReader GetTextReader() => _buffer is null ? TextReader.Null : new StreamReader(GetStream());

        /// <inheritdoc />
        public long GetLength() => _buffer?.Length ?? 0;

        /// <inheritdoc />
        public string MediaType { get; }

        /// <inheritdoc />
        public IContentProvider CloneWithMediaType(string mediaType) =>
            new MemoryContent(_buffer, _index, _count, mediaType);

        /// <inheritdoc />
        public Task<int> GetCacheCodeAsync()
        {
            // Cache the hash code since the bytes can never change
            lock (_hashCodeLock)
            {
                if (_hashCode == default)
                {
                    _hashCode = (int)Crc32.Calculate(_buffer);
                }
                return Task.FromResult(_hashCode);
            }
        }
    }
}