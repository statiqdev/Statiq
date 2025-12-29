using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Statiq.Core
{
    /// <summary>
    /// Removes the <see cref="SynchronizationContext"/> for all following awaiters.
    /// Roughly equivalent to calling <c>.ConfigureAwait(false)</c> for all nested await calls.
    /// See https://blogs.msdn.microsoft.com/benwilli/2017/02/09/an-alternative-to-configureawaitfalse-everywhere/.
    /// </summary>
    public struct SynchronizationContextRemover : INotifyCompletion
    {
        public bool IsCompleted => SynchronizationContext.Current is null;

        public void OnCompleted(Action continuation)
        {
            SynchronizationContext previousContext = SynchronizationContext.Current;
            try
            {
                SynchronizationContext.SetSynchronizationContext(null);
                continuation();
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        public SynchronizationContextRemover GetAwaiter() => this;

        public void GetResult()
        {
        }
    }
}