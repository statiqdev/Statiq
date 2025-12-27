using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A read-only, non-seeking stream produced by iterating over a collection of arbitrary objects.
    /// </summary>
    public abstract class ItemStream<TItem> : Stream
    {
        private ReadOnlyMemory<byte> _remainingMemory;
        private IEnumerator<TItem> _itemEnumerator;
        private byte[] _remainingBuffer = Array.Empty<byte>();
        private bool _readPrefix;
        private bool _readSuffix;

        protected ItemStream(IEnumerable<TItem> items)
        {
            Items = items;
        }
        public IEnumerable<TItem> Items { get; }

        public sealed override bool CanRead => true;

        public sealed override bool CanSeek => false;

        public sealed override bool CanWrite => false;

        public sealed override long Length => throw new NotSupportedException();

        public sealed override long Position
        {
            get
            {
                if (_itemEnumerator is null)
                {
                    return 0;
                }
                throw new NotSupportedException();
            }
            set
            {
                if (value == 0)
                {
                    // Reset the stream if setting position to 0
                    Reset();
                }
                else
                {
                    // We can't seek to a non-0 position otherwise
                    throw new NotSupportedException();
                }
            }
        }

        public sealed override long Seek(long offset, SeekOrigin origin)
        {
            // We can only seek to the beginning of the stream
            if (offset == 0 && origin == SeekOrigin.Begin)
            {
                Reset();
                return 0;
            }
            throw new NotSupportedException();
        }

        public sealed override void SetLength(long value) => throw new NotSupportedException();

        public sealed override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public sealed override void Flush()
        {
        }

        public virtual void Reset()
        {
            _itemEnumerator = null;
            _remainingMemory = default;
            _readPrefix = false;
            _readSuffix = false;
        }

        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan().Slice(offset, count));

        public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(Read(new Span<byte>(buffer, offset, count)));

        public sealed override ValueTask<int> ReadAsync(Memory<byte> memory, CancellationToken cancellationToken)
            => new ValueTask<int>(Read(memory.Span));

        public sealed override int Read(Span<byte> buffer)
        {
            // Read until we've filled the buffer with the requested count
            int read = 0;
            while (true)
            {
                // Get the next chunk of data
                ReadOnlySpan<byte> itemBytes;
                if (!_readPrefix)
                {
                    // Read the prefix if we haven't already
                    itemBytes = GetPrefix();
                    _readPrefix = true;
                }
                else
                {
                    // Do we have leftover bytes?
                    itemBytes = _remainingMemory.Span;
                    _remainingMemory = default;
                    if (itemBytes.IsEmpty)
                    {
                        // If we've already read the suffix then we're done
                        if (_readSuffix)
                        {
                            return read;
                        }

                        // If we're not already enumerating, get an enumerator
                        if (_itemEnumerator is null)
                        {
                            _itemEnumerator = Items?.GetEnumerator();
                        }

                        // Do we actually have an enumerator (or have we reached the end)?
                        if (_itemEnumerator?.MoveNext() != true)
                        {
                            // We're at the end or the enumerator was null, so read the suffix
                            itemBytes = GetSuffix();
                            _readSuffix = true;
                        }
                        else
                        {
                            // Get the current item bytes
                            itemBytes = GetItemBytes(_itemEnumerator.Current);
                        }
                    }
                }

                // Read the bytes if we have some
                if (!itemBytes.IsEmpty)
                {
                    // If we have exactly the number of bytes to fill the remaining buffer, copy to the buffer and return
                    if (itemBytes.Length == buffer.Length)
                    {
                        itemBytes.CopyTo(buffer);
                        read += itemBytes.Length;
                        return read;
                    }

                    // If we have more bytes than we need, slice and retain the rest
                    if (itemBytes.Length > buffer.Length)
                    {
                        ReadOnlySpan<byte> copyBytes = itemBytes.Slice(0, buffer.Length);
                        copyBytes.CopyTo(buffer);
                        read += copyBytes.Length;

                        // Save remaining bytes
                        ReadOnlySpan<byte> remainingSlice = itemBytes.Slice(buffer.Length);
                        if (_remainingBuffer.Length < remainingSlice.Length)
                        {
                            _remainingBuffer = new byte[remainingSlice.Length];
                        }
                        remainingSlice.CopyTo(_remainingBuffer);
                        _remainingMemory = _remainingBuffer.AsMemory(0, remainingSlice.Length);

                        return read;
                    }

                    // We have fewer bytes than we need, fill what we can and slice the destination buffer to continue trying to fill it
                    itemBytes.CopyTo(buffer);
                    read += itemBytes.Length;
                    buffer = buffer.Slice(itemBytes.Length);
                }
            }
        }

        /// <summary>
        /// Gets the memory for a given item.
        /// </summary>
        /// <param name="item">The item to get memory for (can be <c>default</c>/<c>null</c>).</param>
        /// <returns>Memory that represents the item.</returns>
        protected abstract ReadOnlySpan<byte> GetItemBytes(TItem item);

        protected virtual ReadOnlySpan<byte> GetPrefix() => default;

        protected virtual ReadOnlySpan<byte> GetSuffix() => default;

        // Seal the rest to avoid confusion

        public sealed override bool CanTimeout => base.CanTimeout;

        public sealed override int ReadTimeout { get => base.ReadTimeout; set => base.ReadTimeout = value; }

        public sealed override int WriteTimeout { get => base.WriteTimeout; set => base.WriteTimeout = value; }

        public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => base.BeginRead(buffer, offset, count, callback, state);

        public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) => base.BeginWrite(buffer, offset, count, callback, state);

        public sealed override void Close() => base.Close();

        public sealed override void CopyTo(Stream destination, int bufferSize) => base.CopyTo(destination, bufferSize);

        public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken) => base.CopyToAsync(destination, bufferSize, cancellationToken);

        [Obsolete]
        protected sealed override WaitHandle CreateWaitHandle() => base.CreateWaitHandle();

        protected sealed override void Dispose(bool disposing) => base.Dispose(disposing);

        public sealed override ValueTask DisposeAsync() => base.DisposeAsync();

        public sealed override int EndRead(IAsyncResult asyncResult) => base.EndRead(asyncResult);

        public sealed override void EndWrite(IAsyncResult asyncResult) => base.EndWrite(asyncResult);

        public sealed override Task FlushAsync(CancellationToken cancellationToken) => base.FlushAsync(cancellationToken);

        [Obsolete]
        protected sealed override void ObjectInvariant() => base.ObjectInvariant();

        public sealed override int ReadByte() => base.ReadByte();

        public sealed override void Write(ReadOnlySpan<byte> buffer) => base.Write(buffer);

        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => base.WriteAsync(buffer, offset, count, cancellationToken);

        public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => base.WriteAsync(buffer, cancellationToken);

        public sealed override void WriteByte(byte value) => base.WriteByte(value);
    }
}
