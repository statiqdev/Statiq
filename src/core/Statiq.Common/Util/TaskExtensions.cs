using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class TaskExtensions
    {
        // See https://stackoverflow.com/a/15530170
        public static Task<TBase> FromDerivedAsync<TBase, TDerived>(this Task<TDerived> task)
            where TDerived : TBase
        {
            TaskCompletionSource<TBase> tcs = new TaskCompletionSource<TBase>();
#pragma warning disable VSTHRD110 // Observe the awaitable result of this method call by awaiting it, assigning to a variable, or passing it to another method.
            task.ContinueWith(
                t =>
                {
                    if (t.IsFaulted)
                    {
                        tcs.TrySetException(t.Exception.InnerExceptions);
                    }
                    else if (t.IsCanceled)
                    {
                        tcs.TrySetCanceled();
                    }
                    else
                    {
                        tcs.TrySetResult(t.GetAwaiter().GetResult());
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Current);
#pragma warning restore VSTHRD110
            return tcs.Task;
        }
    }
}