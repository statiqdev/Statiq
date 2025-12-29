using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Wraps a <see cref="FileStream"/> and logs that it was written to on disposal,
    /// but only if it was actually written to.
    /// </summary>
    internal class WrittenFileStream : DelegatingStream
    {
        private readonly IFileWriteTracker _fileWriteTracker;
        private readonly LocalFile _file;

        private bool _wroteData;

        public WrittenFileStream(FileStream fileStream, IFileWriteTracker fileWriteTracker, LocalFile file)
            : base(fileStream)
        {
            _fileWriteTracker = fileWriteTracker.ThrowIfNull(nameof(fileWriteTracker));
            _file = file.ThrowIfNull(nameof(file));
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            _wroteData = true;
            return base.BeginWrite(buffer, offset, count, callback, state);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _wroteData = true;
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _wroteData = true;
            base.Write(buffer, offset, count);
        }

        public override void WriteByte(byte value)
        {
            _wroteData = true;
            base.WriteByte(value);
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            _wroteData = true;
            base.Write(buffer);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _wroteData = true;
            return base.WriteAsync(buffer, cancellationToken);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _wroteData = true;
            base.EndWrite(asyncResult);
        }

        protected override void Dispose(bool disposing)
        {
            Stream.Dispose();
            if (_wroteData)
            {
                _file.Refresh();
                _fileWriteTracker.TrackWrite(_file.Path, _file.GetCacheCode(), true);
            }
        }

        public override async ValueTask DisposeAsync()
        {
            await Stream.DisposeAsync();
            if (_wroteData)
            {
                _file.Refresh();
                _fileWriteTracker.TrackWrite(_file.Path, await _file.GetCacheCodeAsync(), true);
            }
        }
    }
}