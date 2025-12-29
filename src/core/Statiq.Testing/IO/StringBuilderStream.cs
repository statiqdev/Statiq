using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Statiq.Testing
{
    // Provides a writeable stream to a StringBuilder
    // Initially based on code from Simple.Web (https://github.com/markrendle/Simple.Web)
    public class StringBuilderStream : Stream
    {
        private readonly Random _random = new Random();
        private readonly StringBuilder _resultBuilder;
        private readonly MemoryStream _buffer;

        public StringBuilderStream(StringBuilder resultBuilder)
        {
            _resultBuilder = resultBuilder;
            _buffer = new MemoryStream();

            // Copy the old result into the current buffer.
            using (StreamWriter writer = new StreamWriter(_buffer, Encoding.Default, 2048, true))
            {
                writer.Write(resultBuilder.ToString());
                writer.Flush();
            }
            _buffer.Position = 0;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _buffer.Length;

        public override long Position
        {
            get => _buffer.Position;
            set => _buffer.Position = value;
        }

        public override void Flush()
        {
            string content = _buffer.TryGetBuffer(out ArraySegment<byte> segment)
                ? Encoding.UTF8.GetString(segment.Array, segment.Offset, segment.Count)
                : Encoding.UTF8.GetString(_buffer.ToArray());
            _resultBuilder.Clear();
            _resultBuilder.Append(content);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => _buffer.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            Thread.Sleep(_random.Next(200)); // Sleep for a random amount to simulate realistic file operations
            _buffer.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            Flush();
            base.Dispose(disposing);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            Thread.Sleep(_random.Next(200)); // Sleep for a random amount to simulate realistic file operations
            return _buffer.BeginWrite(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _buffer.EndWrite(asyncResult);
            Flush();
        }
    }
}
