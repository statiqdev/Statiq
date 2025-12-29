using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public class StringStream : Stream
    {
        private const int DefaultBufferCharCount = 512;  // The number of characters to encode and buffer at a time

        private readonly Encoding _encoding;
        private readonly int _bufferCharCount;
        private byte[] _outputBuffer;
        private Encoder _encoder;
        private ReadOnlyMemory<char> _pendingString;
        private ReadOnlyMemory<byte> _pendingOutput;

        public StringStream(string str)
            : this(str, Encoding.Default)
        {
        }

        public StringStream(string str, Encoding encoding)
            : this(str, encoding, DefaultBufferCharCount)
        {
        }

        public StringStream(string str, Encoding encoding, int bufferCharCount)
        {
            String = str;
            _pendingString = String?.AsMemory() ?? default;
            _encoding = encoding.ThrowIfNull(nameof(encoding));
            _bufferCharCount = bufferCharCount;
            _outputBuffer = ArrayPool<byte>.Shared.Rent(_encoding.GetMaxByteCount(_bufferCharCount));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ArrayPool<byte>.Shared.Return(_outputBuffer);
            }
        }

#pragma warning disable CA1720
        public string String { get; }
#pragma warning restore CA1720

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _encoding.GetByteCount(String);

        public override long Position
        {
            get => throw new NotSupportedException();
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

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        public override void Flush()
        {
        }

        public virtual void Reset()
        {
            _encoder = null;
            _pendingString = String?.AsMemory() ?? default;
            _pendingOutput = default;
        }

        public sealed override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan().Slice(offset, count));

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => Task.FromResult(Read(new Span<byte>(buffer, offset, count)));

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken)
            => new ValueTask<int>(Read(buffer.Span));

        public sealed override int Read(Span<byte> buffer)
        {
            // If the encoder is null, this is the initial read so write the preamble (if there is one)
            // "Note that the GetBytes method does not prepend a BOM to a sequence of encoded bytes; supplying
            // a BOM at the beginning of an appropriate byte stream is the developer's responsibility."
            // From https://docs.microsoft.com/en-us/dotnet/api/system.text.utf8encoding.preamble?view=net-5.0
            int read = 0;
            if (_encoder is null)
            {
                _encoder = _encoding.GetEncoder();

                ReadOnlySpan<byte> preamble = _encoding.Preamble;
                if (!preamble.IsEmpty)
                {
                    if (buffer.Length < preamble.Length)
                    {
                        // The output buffer isn't big enough to hold the whole preamble, so fill what we can and buffer the rest
                        preamble.Slice(0, buffer.Length).CopyTo(buffer);
                        preamble = preamble.Slice(buffer.Length);
                        if (preamble.Length > _outputBuffer.Length)
                        {
                            // The output buffer isn't big enough for the remaining preamble, so expand it
                            // This should only happen if the bufferCharCount is really small
                            ArrayPool<byte>.Shared.Return(_outputBuffer);
                            _outputBuffer = ArrayPool<byte>.Shared.Rent(preamble.Length);
                        }
                        preamble.CopyTo(_outputBuffer);
                        _pendingOutput = _outputBuffer.AsMemory().Slice(0, preamble.Length);
                        return buffer.Length;
                    }

                    // The output buffer is big enough to hold the whole preamble so copy it and continue
                    preamble.CopyTo(buffer);
                    buffer = buffer.Slice(preamble.Length);
                    read = preamble.Length;
                }
            }

            // Fill the buffer
            while (true)
            {
                // Have we filled the buffer?
                if (buffer.IsEmpty)
                {
                    return read;
                }

                // Do we need to buffer more output data?
                if (_pendingOutput.IsEmpty)
                {
                    // Are we out of data to buffer?
                    if (_pendingString.IsEmpty)
                    {
                        return read;
                    }

                    // We've got more source left so encode and buffer it
                    ReadOnlySpan<char> encodeChars = _pendingString.Length > _bufferCharCount ? _pendingString.Slice(0, _bufferCharCount).Span : _pendingString.Span;
                    _pendingString = _pendingString.Slice(encodeChars.Length);
                    int encodedLength = _encoder.GetBytes(encodeChars, _outputBuffer, false);
                    _pendingOutput = _outputBuffer.AsMemory().Slice(0, encodedLength);
                }

                // Fill the buffer with the pending output
                ReadOnlySpan<byte> pendingSlice = _pendingOutput.Length > buffer.Length ? _pendingOutput.Slice(0, buffer.Length).Span : _pendingOutput.Span;
                pendingSlice.CopyTo(buffer);
                read += pendingSlice.Length;
                buffer = buffer.Slice(pendingSlice.Length);
                _pendingOutput = _pendingOutput.Slice(pendingSlice.Length);
            }
        }
    }
}