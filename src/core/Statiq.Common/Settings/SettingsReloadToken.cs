using System;
using Microsoft.Extensions.Primitives;

namespace Statiq.Common
{
    internal sealed class SettingsReloadToken : IChangeToken
    {
#pragma warning disable SA1401 // Fields must be private
        /// <summary>
        /// A singleton instance of the <see cref="EmptyDisposable"/>.
        /// </summary>
        public static SettingsReloadToken Instance = new SettingsReloadToken();
#pragma warning restore SA1401 // Fields must be private

        private SettingsReloadToken()
        {
        }

        public bool HasChanged => false;

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state) => EmptyDisposable.Instance;
    }
}
