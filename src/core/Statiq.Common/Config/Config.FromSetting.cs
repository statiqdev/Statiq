using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static partial class Config
    {
        /// <summary>
        /// Creates a config value by getting the string metadata value from the execution context of a specified key.
        /// </summary>
        /// <param name="key">The metadata key to get the string value from.</param>
        /// <returns>A config object.</returns>
        public static Config<string> FromSetting(string key) => FromSetting<string>(key);

        /// <summary>
        /// Creates a config value by getting the string metadata value from the execution context of a specified key.
        /// </summary>
        /// <param name="key">The metadata key to get the string value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found, is null, or cannot be converted to a string.</param>
        /// <returns>A config object.</returns>
        public static Config<string> FromSetting(string key, string defaultValue) => FromSetting<string>(key, defaultValue);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSetting<TValue>(string key) =>
            new Config<TValue>((_, ctx) => Task.FromResult(ctx.Settings.Get<TValue>(key)), false);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found, is null, or cannot be converted to <typeparamref name="TValue"/>.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSetting<TValue>(string key, TValue defaultValue) =>
            new Config<TValue>((_, ctx) => Task.FromResult(ctx.Settings.Get(key, defaultValue)), false);

        /// <summary>
        /// Creates a config value by getting the metadata value from the execution context of a specified key.
        /// </summary>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found or is null.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromSetting(string key, object defaultValue = null) =>
            new Config<object>((_, ctx) => Task.FromResult(ctx.Settings.Get(key, defaultValue)), false);

        /// <summary>
        /// Creates a config value from a delegate that uses the settings.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Func<IReadOnlySettings, TValue> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>((_, ctx) => Task.FromResult(func(ctx.Settings)), false);
        }

        /// <summary>
        /// Creates a config value from a delegate that uses the settings.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Func<IReadOnlySettings, Task<TValue>> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>((_, ctx) => func(ctx.Settings), false);
        }

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Action<IReadOnlySettings> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>((__, ctx) =>
            {
                action(ctx.Settings);
                return Task.FromResult(default(TValue));
            });
        }

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromSettings<TValue>(Func<IReadOnlySettings, Task> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>(async (__, ctx) =>
            {
                await action(ctx.Settings);
                return default;
            });
        }

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromSettings(Action<IReadOnlySettings> action) =>
            FromSettings<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses the settings and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromSettings(Func<IReadOnlySettings, Task> action) =>
            FromSettings<object>(action);

        /// <summary>
        /// Creates a config value that returns <c>true</c> if the settings contains all the specified keys.
        /// </summary>
        /// <param name="keys">The keys to check.</param>
        /// <returns>A config object.</returns>
        public static Config<bool> ContainsSettings(params string[] keys) =>
            FromSettings(settings => keys.All(x => settings.ContainsKey(x)));

        /// <summary>
        /// Creates a config value that returns <c>true</c> if the settings contains any of the specified keys.
        /// </summary>
        /// <param name="keys">The keys to check.</param>
        /// <returns>A config object.</returns>
        public static Config<bool> ContainsAnySettings(params string[] keys) =>
            FromSettings(settings => keys.Any(x => settings.ContainsKey(x)));
    }
}
