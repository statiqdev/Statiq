using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps a <see cref="Stream"/> and signals when it's disposed.
    /// </summary>
    public class SignalingStream : DelegatingStream
    {
        private readonly EventWaitHandle _waitHandle;

        public SignalingStream(Stream stream, EventWaitHandle waitHandle)
            : base(stream)
        {
            _waitHandle = waitHandle.ThrowIfNull(nameof(waitHandle));
        }

        protected override void Dispose(bool disposing)
        {
            _waitHandle.Set();
        }

        public override ValueTask DisposeAsync()
        {
            _waitHandle.Set();
            return default;
        }
    }
}
