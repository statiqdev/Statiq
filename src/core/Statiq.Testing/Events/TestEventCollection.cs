using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestEventCollection : IEventCollection
    {
        // Stores lists of AsyncEventHandler<TEventArgs> keyed by TEventArgs
        // Don't use a single Delegate with Delegate.Combine() because it
        // results in each subsequent delegate in the chain invoking all previous ones
        private readonly ConcurrentDictionary<Type, List<Delegate>> _events =
            new ConcurrentDictionary<Type, List<Delegate>>();

        /// <inheritdoc />
        public void Subscribe<TEvent>(AsyncEventHandler<TEvent> handler) =>
            _events.AddOrUpdate(
                typeof(TEvent),
                _ => new List<Delegate> { handler.ThrowIfNull(nameof(handler)) },
                (_, handlers) =>
                {
                    handlers.Add(handler.ThrowIfNull(nameof(handler)));
                    return handlers;
                });

        /// <inheritdoc />
        public async Task<bool> RaiseAsync<TEvent>(TEvent evt)
        {
            evt.ThrowIfNull(nameof(evt));
            if (_events.TryGetValue(typeof(TEvent), out List<Delegate> handlers))
            {
                foreach (Delegate handler in handlers)
                {
                    await ((AsyncEventHandler<TEvent>)handler)(evt);
                }
                return true;
            }
            return false;
        }
    }
}
