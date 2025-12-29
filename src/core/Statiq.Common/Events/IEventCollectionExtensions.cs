using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IEventCollectionExtensions
    {
        /// <summary>
        /// Subscribes a new handler for the given <typeparamref name="TEvent"/> type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to subscribe and handler for.</typeparam>
        /// <param name="eventCollection">The event collections.</param>
        /// <param name="handler">The handler to subscribe to the event.</param>
        public static void Subscribe<TEvent>(this IEventCollection eventCollection, EventHandler<TEvent> handler)
        {
            eventCollection.ThrowIfNull(nameof(eventCollection));
            handler.ThrowIfNull(nameof(handler));

            eventCollection.Subscribe((AsyncEventHandler<TEvent>)(args =>
            {
                handler(args);
                return Task.CompletedTask;
            }));
        }
    }
}
