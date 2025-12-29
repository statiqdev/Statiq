using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// An asynchronous event handler.
    /// </summary>
    /// <typeparam name="TEvent">The type of event.</typeparam>
    /// <param name="evt">The event instance.</param>
    public delegate void EventHandler<TEvent>(TEvent evt);
}
