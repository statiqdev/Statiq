using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static partial class Config
    {
        /// <summary>
        /// Creates a config value.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="value">The config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromValue<TValue>(TValue value) =>
            new Config<TValue>((_, __) => Task.FromResult(value), false);

        /// <summary>
        /// Creates a config value.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="value">The config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromValue<TValue>(Task<TValue> value)
        {
#pragma warning disable VSTHRD110 // Observe the awaitable result of this method call by awaiting it, assigning to a variable, or passing it to another method.
            value.ThrowIfNull(nameof(value));
#pragma warning restore VSTHRD110

#pragma warning disable VSTHRD003 // Avoid awaiting or returning a Task representing work that was not started within your context as that can lead to deadlocks.
            return new Config<TValue>((_, __) => value, false);
#pragma warning restore VSTHRD003
        }

        /// <summary>
        /// Creates an enumeration of config values.
        /// </summary>
        /// <typeparam name="TValue">The type of config value items.</typeparam>
        /// <param name="values">The config values.</param>
        /// <returns>A config object.</returns>
        public static Config<IEnumerable<TValue>> FromValues<TValue>(params TValue[] values) =>
            new Config<IEnumerable<TValue>>((_, __) => Task.FromResult<IEnumerable<TValue>>(values), false);
    }
}