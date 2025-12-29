using System;

namespace Statiq.Common
{
    /// <summary>
    /// Contains global events and event handlers.
    /// </summary>
    public interface IEventCollection : IReadOnlyEventCollection
    {
        /// <summary>
        /// Subscribes a new handler for the given <typeparamref name="TEvent"/> type.
        /// </summary>
        /// <typeparam name="TEvent">The type of event to subscribe and handler for.</typeparam>
        /// <param name="handler">The handler to subscribe to the event.</param>
        void Subscribe<TEvent>(AsyncEventHandler<TEvent> handler);
    }
}
