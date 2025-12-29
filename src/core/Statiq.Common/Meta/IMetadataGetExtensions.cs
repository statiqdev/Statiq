using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Statiq.Common
{
    public static class IMetadataGetExtensions
    {
        /// <summary>
        /// Tries to get the value for the specified key.
        /// </summary>
        /// <remarks>
        /// This method will also materialize <see cref="IMetadataValue"/> and
        /// evaluate script strings. A key that starts with "=>" (cached) or "->" (uncached)
        /// will be treated as a script and evaluated (without caching regardless of script prefix).
        /// </remarks>
        /// <param name="metadata">The metadata instance.</param>
        /// <typeparam name="TValue">The desired return type.</typeparam>
        /// <param name="key">The key of the value to get. If the key is <c>null</c>, this will return the default value.</param>
        /// <param name="value">The value of the key if it was found and could be converted to the desired return type.</param>
        /// <returns><c>true</c> if the key was found and the value could be converted to the desired return type, <c>false</c> otherwise.</returns>
        public static bool TryGetValue<TValue>(
            this IMetadata metadata,
            string key,
            out TValue value)
        {
            if (metadata is object && key is object)
            {
                // Script-based key (we don't care if it's cached in this code path, script keys be evaluated every time from here)
                if (IScriptHelper.TryGetScriptString(key, out string script).HasValue)
                {
                    IExecutionContext context = IExecutionContext.Current;
#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks. Use await or JoinableTaskFactory.Run instead.
                    object result = context.ScriptHelper.EvaluateAsync(script, metadata).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
                    return TypeHelper.TryExpandAndConvert(key, result, metadata, out value);
                }

                // Value
                if (metadata.TryGetRaw(key, out object raw))
                {
                    return TypeHelper.TryExpandAndConvert(key, raw, metadata, out value);
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value or null if the key is not found or is <c>null</c>.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or is <c>null</c>.</param>
        /// <returns>The value for the specified key or the specified default value.</returns>
        public static object Get(
            this IMetadata metadata,
            string key,
            object defaultValue = null) =>
            metadata.TryGetValue(key, out object value) ? value : defaultValue;

        /// <summary>
        /// Gets the value for the specified key converted to the specified type.
        /// This method never throws an exception. It will return default(T) if the key is not found,
        /// is <c>null</c>, or the value cannot be converted to T.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The value for the specified key converted to type T or default(T) if the key is not found, is <n>null</n>, or cannot be converted to type T.</returns>
        public static T Get<T>(this IMetadata metadata, string key) => metadata.TryGetValue(key, out T value) ? value : default;

        /// <summary>
        /// Gets the value for the specified key. This method never throws an exception. It will return the specified
        /// default value if the key is not found or is <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The desired return type.</typeparam>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found, is <c>null</c>, or cannot be converted to type T.</param>
        /// <returns>The value for the specified key converted to type T or the specified default value.</returns>
        public static T Get<T>(this IMetadata metadata, string key, T defaultValue) => metadata.TryGetValue(key, out T value) ? value : defaultValue;

        /// <summary>
        /// Gets the raw value for the specified key. This method will not materialize <see cref="IMetadataValue"/>
        /// values the way other get methods will. A <see cref="KeyNotFoundException"/> will be thrown
        /// for missing keys.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>The raw value for the specified key.</returns>
        public static object GetRaw(this IMetadata metadata, string key) => metadata.TryGetRaw(key, out object value) ? value : throw new KeyNotFoundException(nameof(key));

        /// <summary>
        /// Gets a new filtered <see cref="IMetadata"/> containing only the specified keys and their values. If a key is not present in the current
        /// metadata, it will be ignored and will not be copied to the new metadata object.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="keys">The keys to include in the new filtered metadata object.</param>
        /// <returns>A new filtered <see cref="IMetadata"/> containing the specified keys and their values.</returns>
        public static IMetadata FilterMetadata(this IMetadata metadata, params string[] keys) => new FilteredMetadata(metadata, keys);

        /// <summary>
        /// Gets an enumerable that enumerates key-value pairs.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <returns>An enumerable over key-value pairs.</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetEnumerable(this IMetadata metadata)
        {
            metadata.ThrowIfNull(nameof(metadata));
            return new EnumerableEnumerator<KeyValuePair<string, object>>(() => metadata.GetEnumerator());
        }

        /// <summary>
        /// Gets an enumerable that enumerates raw key-value pairs
        /// (I.e., the values have not been expanded similar to <see cref="IMetadata.TryGetRaw(string, out object)"/>).
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <returns>An enumerable over raw key-value pairs.</returns>
        public static IEnumerable<KeyValuePair<string, object>> GetRawEnumerable(this IMetadata metadata)
        {
            metadata.ThrowIfNull(nameof(metadata));
            return new EnumerableEnumerator<KeyValuePair<string, object>>(() => metadata.GetRawEnumerator());
        }

        public static IMetadata WithoutSettings(this IMetadata metadata)
        {
            metadata.ThrowIfNull(nameof(metadata));
            return new Metadata(metadata.GetRawEnumerable().Where(x => !(x.Value is SettingsValue)));
        }
    }
}