using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Delegates all stream access to an underlying stream. Disposing this stream
    /// will not dispose the wrapped stream unless the derived implementation
    /// does that explicitly.
    /// </summary>
    public abstract class DelegatingStream : Stream
    {
        protected Stream Stream { get; }

        protected DelegatingStream(Stream stream)
        {
            Stream = stream.ThrowIfNull(nameof(stream));
        }

        public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) =>
            await Stream.CopyToAsync(destination, bufferSize, cancellationToken);

        public override void CopyTo(Stream destination, int bufferSize) => Stream.CopyTo(destination, bufferSize);

        public override void Flush() => Stream.Flush();

        public override async Task FlushAsync(CancellationToken cancellationToken) =>
            await Stream.FlushAsync(cancellationToken);

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
            Stream.BeginRead(buffer, offset, count, callback, state);

        public override int EndRead(IAsyncResult asyncResult) => Stream.EndRead(asyncResult);

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            await Stream.ReadAsync(buffer, offset, count, cancellationToken);

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
            Stream.BeginWrite(buffer, offset, count, callback, state);

        public override void EndWrite(IAsyncResult asyncResult) => Stream.EndWrite(asyncResult);

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
            await Stream.WriteAsync(buffer, offset, count, cancellationToken);

        public override long Seek(long offset, SeekOrigin origin) => Stream.Seek(offset, origin);

        public override void SetLength(long value) => Stream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count) => Stream.Read(buffer, offset, count);

        public override int ReadByte() => Stream.ReadByte();

        public override void Write(byte[] buffer, int offset, int count) => Stream.Write(buffer, offset, count);

        public override void WriteByte(byte value) => Stream.WriteByte(value);

        public override bool CanRead => Stream.CanRead;

        public override bool CanSeek => Stream.CanSeek;

        public override bool CanTimeout => Stream.CanTimeout;

        public override bool CanWrite => Stream.CanWrite;

        public override long Length => Stream.Length;

        public override long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }

        public override int ReadTimeout
        {
            get => Stream.ReadTimeout;
            set => Stream.ReadTimeout = value;
        }

        public override int WriteTimeout
        {
            get => Stream.WriteTimeout;
            set => Stream.WriteTimeout = value;
        }
    }
}
