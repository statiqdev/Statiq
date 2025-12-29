using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Contains global events and event handlers.
    /// </summary>
    public interface IReadOnlyEventCollection
    {
        /// <summary>
        /// Raises all handlers for a registered event
        /// (as indicated by the <typeparamref name="TEvent"/> type).
        /// </summary>
        /// <typeparam name="TEvent">The type of event to raise an event for.</typeparam>
        /// <param name="evt">The event instance.</param>
        /// <returns><c>true</c> if registered handlers were found, <c>false</c> otherwise.</returns>
        Task<bool> RaiseAsync<TEvent>(TEvent evt);
    }
}
