using System;
using System.IO;
using System.Threading;

namespace Statiq.Common
{
    /// <summary>
    /// Provides a wrapper around a stream that locks the stream during concurrent access.
    /// </summary>
    public class LockingStreamWrapper : IDisposable
    {
        private readonly AutoResetEvent _resetEvent = new AutoResetEvent(true);

        private readonly bool _disposeStream;

        public LockingStreamWrapper(Stream stream, bool disposeStream)
        {
            Stream = stream;
            if (Stream is object && !Stream.CanSeek)
            {
                throw new ArgumentException("Stream should be seekable", nameof(stream));
            }
            _disposeStream = disposeStream;
        }

#pragma warning disable CA1721
        protected Stream Stream { get; }
#pragma warning restore CA1721

        /// <summary>
        /// Gets the wrapped stream and locks access until it's disposed.
        /// The returned stream should be disposed after use as soon as possible.
        /// </summary>
        /// <returns>The wrapped stream.</returns>
        public Stream GetStream()
        {
            if (Stream is null)
            {
                return Stream.Null;
            }
            _resetEvent.WaitOne();
            Stream.Position = 0; // Reset the position for each read
            return new SignalingStream(Stream, _resetEvent);
        }

        /// <summary>
        /// Gets the length of the wrapped stream without locking it.
        /// </summary>
        /// <returns>The length of the wrapped stream.</returns>
        public long GetLength() => Stream?.Length ?? 0;

        public void Dispose()
        {
            if (_disposeStream)
            {
                Stream.Dispose();
            }
        }
    }
}