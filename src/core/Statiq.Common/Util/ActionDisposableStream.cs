using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps a <see cref="Stream"/> and calls an action when it's disposed.
    /// Disposing this stream will also dispose the underlying stream before the action is called.
    /// </summary>
    public class ActionDisposableStream : DelegatingStream
    {
        private readonly Action _action;

        public ActionDisposableStream(Stream stream, Action action)
            : base(stream)
        {
            _action = action.ThrowIfNull(nameof(action));
        }

        protected override void Dispose(bool disposing)
        {
            Stream.Dispose();
            _action();
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            _action();
        }
    }
}
