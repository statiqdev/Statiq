using System;

namespace Statiq.Common
{
    /// <summary>
    /// A simple disposable that calls an action on disposal. This class
    /// will also throw an exception on subsequent disposals.
    /// </summary>
    public class ActionDisposable : IDisposable
    {
        private readonly Action _action;
        private bool _disposed;

        /// <summary>
        /// Create a disposable instance.
        /// </summary>
        /// <param name="action">The action to call on disposal.</param>
        public ActionDisposable(Action action)
        {
            _action = action.ThrowIfNull(nameof(action));
        }

        /// <summary>
        /// Calls the action.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ActionDisposable));
            }
            _disposed = true;
            _action();
        }
    }
}