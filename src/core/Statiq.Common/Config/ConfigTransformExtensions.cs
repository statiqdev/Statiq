using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class ConfigTransformExtensions
    {
        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, TValue> transform)
        {
            transform.ThrowIfNull(nameof(transform));

            if (config is null)
            {
                return Config.FromValue(transform(default));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => transform(await config.GetValueAsync(doc, ctx)))
                : Config.FromContext(async ctx => transform(await config.GetValueAsync(null, ctx)));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, Task<TValue>> transform)
        {
            transform.ThrowIfNull(nameof(transform));

            if (config is null)
            {
                return Config.FromValue(transform(default));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => await transform(await config.GetValueAsync(doc, ctx)))
                : Config.FromContext(async ctx => await transform(await config.GetValueAsync(null, ctx)));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IExecutionContext, TValue> transform)
        {
            transform.ThrowIfNull(nameof(transform));

            if (config is null)
            {
                return Config.FromContext(ctx => transform(default, ctx));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => transform(await config.GetValueAsync(doc, ctx), ctx))
                : Config.FromContext(async ctx => transform(await config.GetValueAsync(null, ctx), ctx));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IExecutionContext, Task<TValue>> transform)
        {
            transform.ThrowIfNull(nameof(transform));

            if (config is null)
            {
                return Config.FromContext(ctx => transform(default, ctx));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => await transform(await config.GetValueAsync(doc, ctx), ctx))
                : Config.FromContext(async ctx => await transform(await config.GetValueAsync(null, ctx), ctx));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IDocument, IExecutionContext, TValue> transform)
        {
            transform.ThrowIfNull(nameof(transform));

            if (config is null)
            {
                return Config.FromDocument((doc, ctx) => transform(default, doc, ctx));
            }
            return Config.FromDocument(async (doc, ctx) => transform(await config.GetValueAsync(doc, ctx), doc, ctx));
        }

        public static Config<TValue> Transform<TValue, TSource>(this Config<TSource> config, Func<TSource, IDocument, IExecutionContext, Task<TValue>> transform)
        {
            transform.ThrowIfNull(nameof(transform));

            if (config is null)
            {
                return Config.FromDocument((doc, ctx) => transform(default, doc, ctx));
            }
            return Config.FromDocument(async (doc, ctx) => await transform(await config.GetValueAsync(doc, ctx), doc, ctx));
        }

        /// <summary>
        /// Casts the config delegate to the specified type.
        /// </summary>
        /// <typeparam name="TValue">The type to cast the value to.</typeparam>
        /// <param name="config">The config delegate to cast.</param>
        /// <returns>The config value as the specified type or <c>default</c> if <paramref name="config"/> is null.</returns>
        public static Config<TValue> Cast<TValue>(this IConfig config)
        {
            if (config is null)
            {
                return Config.FromValue(default(TValue));
            }
            return config.RequiresDocument
                ? Config.FromDocument(async (doc, ctx) => (TValue)await config.GetValueAsync(doc, ctx))
                : Config.FromContext(async ctx => (TValue)await config.GetValueAsync(null, ctx));
        }

        public static Config<IEnumerable<TValue>> MakeEnumerable<TValue>(this Config<TValue> config) => config.Transform(YieldValue);

        private static IEnumerable<TValue> YieldValue<TValue>(TValue value)
        {
            yield return value;
        }
    }
}
