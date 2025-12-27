using System.IO;
using System.Text;
using Microsoft.IO;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Forwards calls to an underlying <see cref="RecyclableMemoryStreamManager"/>
    /// so that Statiq.Common doesn't have to maintain a reference to it.
    /// </summary>
    public class MemoryStreamFactory : IMemoryStreamFactory
    {
        private const int BlockSize = 16384;

        private readonly RecyclableMemoryStreamManager _manager =
            new RecyclableMemoryStreamManager(new RecyclableMemoryStreamManager.Options
            {
                BlockSize = BlockSize,
                LargeBufferMultiple = RecyclableMemoryStreamManager.DefaultLargeBufferMultiple,
                MaximumBufferSize = RecyclableMemoryStreamManager.DefaultMaximumBufferSize,
                MaximumLargePoolFreeBytes = BlockSize * 32768L * 2, // 1 GB
            });

        public virtual MemoryStream GetStream() => _manager.GetStream();

        public virtual MemoryStream GetStream(int requiredSize) => _manager.GetStream(null, requiredSize);

        public virtual MemoryStream GetStream(int requiredSize, bool asContiguousBuffer) =>
            _manager.GetStream(null, requiredSize, asContiguousBuffer);

        public virtual MemoryStream GetStream(byte[] buffer, int offset, int count) =>
            _manager.GetStream(null, buffer, offset, count);

        public virtual MemoryStream GetStream(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return GetStream();
            }
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            return GetStream(contentBytes, 0, contentBytes.Length);
        }
    }
}