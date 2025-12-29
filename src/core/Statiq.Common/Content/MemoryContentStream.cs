using System.IO;

namespace Statiq.Common
{
    internal class MemoryContentStream : ContentStream
    {
        private byte[] _buffer;

        public MemoryContentStream(MemoryStream memoryStream)
            : base(memoryStream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (_buffer is null)
            {
                // Copy the buffer out of the MemoryStream before disposing it
                _buffer = ((MemoryStream)Stream).ToArray();
            }

            Stream.Dispose();
        }

        public override IContentProvider GetContentProvider(string mediaType) =>
            _buffer is null
                ? new MemoryContent(((MemoryStream)Stream).ToArray(), mediaType)
                : new MemoryContent(_buffer, mediaType);
    }
}
