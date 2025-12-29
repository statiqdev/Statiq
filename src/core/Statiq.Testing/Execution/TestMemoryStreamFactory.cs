using System.IO;
using System.Text;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestMemoryStreamFactory : IMemoryStreamFactory
    {
        public MemoryStream GetStream() => new MemoryStream();

        public MemoryStream GetStream(int requiredSize) => new MemoryStream(requiredSize);

        public MemoryStream GetStream(int requiredSize, bool asContiguousBuffer) => new MemoryStream(requiredSize);

        public MemoryStream GetStream(byte[] buffer, int offset, int count) => new MemoryStream(buffer, offset, count);

        public MemoryStream GetStream(string content)
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
