using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class ConfigExtensions
    {
#pragma warning disable CS0618 // Type or member is obsolete, suppress since this is the only place we should call .GetAndTransformValueAsync()
        public static Task<TValue> GetValueAsync<TValue>(
            this Config<TValue> config,
            IDocument document,
            IExecutionContext context,
            Func<TValue, TValue> transform = null) =>
            config?.GetAndTransformValueAsync(document, context, transform) ?? Task.FromResult(default(TValue));

        public static async Task<TValue> GetValueAsync<TValue>(
            this Config<object> config,
            IDocument document,
            IExecutionContext context,
            string errorDetails = null)
        {
            if (config is null)
            {
                return default;
            }

            object value = await config.GetAndTransformValueAsync(document, context);
            if (!TypeHelper.TryConvert(value, out TValue result))
            {
                throw new InvalidOperationException(
                    $"Could not convert from type {value?.GetType().Name ?? "null"} to type {typeof(TValue).Name}{Config.GetErrorDetails(errorDetails)}");
            }
            return result;
        }

        public static async Task<TValue> TryGetValueAsync<TValue>(
            this Config<object> config,
            IDocument document,
            IExecutionContext context)
        {
            if (config is null)
            {
                return default;
            }

            object value = await config.GetAndTransformValueAsync(document, context);
            return TypeHelper.TryConvert(value, out TValue result) ? result : default;
        }

        /// <summary>
        /// Filters the documents.
        /// </summary>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="predicate">The predicate.</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>Filtered documents where the provided predicate is true.</returns>
        public static IAsyncEnumerable<IDocument> FilterAsync(this IEnumerable<IDocument> documents, Config<bool> predicate, IExecutionContext context)
        {
            predicate.ThrowIfNull(nameof(predicate));
            return documents.FilterAsync(new Config<bool>[] { predicate }, context);
        }

        /// <summary>
        /// Filters the documents using "or" logic on multiple predicates.
        /// </summary>
        /// <param name="documents">The documents to filter.</param>
        /// <param name="predicates">The predicates to combine.</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>Filtered documents where at least one of the provided predicates is true.</returns>
        public static async IAsyncEnumerable<IDocument> FilterAsync(this IEnumerable<IDocument> documents, ICollection<Config<bool>> predicates, IExecutionContext context)
        {
            documents.ThrowIfNull(nameof(documents));
            predicates.ThrowIfNull(nameof(predicates));

            foreach (IDocument document in documents)
            {
                context.CancellationToken.ThrowIfCancellationRequested();
                foreach (Config<bool> predicate in predicates)
                {
                    _ = predicate ?? throw new ArgumentException("Null predicates are not supported", nameof(predicates));
                    if (await predicate.GetAndTransformValueAsync(document, context))
                    {
                        yield return document;
                        break;
                    }
                }
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Ensures that the config is not null and throws <see cref="ArgumentNullException"/> if it is.
        /// </summary>
        /// <typeparam name="TConfig">The config type.</typeparam>
        /// <param name="config">The config.</param>
        /// <param name="paramName">The name of the config parameter.</param>
        /// <returns>The config if non-null.</returns>
        public static TConfig EnsureNonNull<TConfig>(this TConfig config, string paramName = null)
            where TConfig : IConfig =>
            config.ThrowIfNull(paramName);

        /// <summary>
        /// Ensures that the config is not null and doesn't require a document and throws
        /// <see cref="ArgumentNullException"/> or <see cref="ArgumentException"/> if it is.
        /// </summary>
        /// <typeparam name="TConfig">The config type.</typeparam>
        /// <param name="config">The config.</param>
        /// <param name="paramName">The name of the config parameter.</param>
        /// <returns>The config if non-null and non-document.</returns>
        public static TConfig EnsureNonDocument<TConfig>(this TConfig config, string paramName = null)
            where TConfig : IConfig =>
            config.EnsureNonNull(paramName).RequiresDocument
                ? throw new ArgumentException("Config must not require a document", paramName)
                : config;

        /// <summary>
        /// Ensures that the config doesn't require a document, but only if not null and throws
        /// <see cref="ArgumentException"/> if it does.
        /// </summary>
        /// <typeparam name="TConfig">The config type.</typeparam>
        /// <param name="config">The config.</param>
        /// <param name="paramName">The name of the config parameter.</param>
        /// <returns>The config if non-document (or null if the config is null).</returns>
        public static TConfig EnsureNonDocumentIfNonNull<TConfig>(this TConfig config, string paramName = null)
            where TConfig : IConfig =>
            config is object
                ? config.EnsureNonDocument(paramName)
                : config;

        public static Config<bool> IsFalse(this Config<bool> config) => config.Transform(x => !x);
    }
}
