using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class ConfigCombineWithExtensions
    {
        public static Config<bool> CombineWith(this Config<bool> first, Config<bool> second)
        {
            if (first is null && second is null)
            {
                return Config.FromValue(false);
            }
            if (first is null)
            {
                return second;
            }
            if (second is null)
            {
                return first;
            }
            return new Config<bool>(
                async (doc, ctx) => await first.GetValueAsync(doc, ctx) && await second.GetValueAsync(doc, ctx),
                first.RequiresDocument || second.RequiresDocument);
        }

        public static Config<TValue> CombineWith<TValue, TFirst, TSecond>(this Config<TFirst> first, Config<TSecond> second, Func<TFirst, TSecond, TValue> combine)
        {
            combine.ThrowIfNull(nameof(combine));

            // Don't need to check for first is null because Transform() handles that

            if (second is null)
            {
                return first.Transform(value => combine(value, default));
            }
            return second.RequiresDocument
                ? first.Transform(async (value, doc, ctx) => combine(value, await second.GetValueAsync(doc, ctx)))
                : first.Transform(async (value, ctx) => combine(value, await second.GetValueAsync(null, ctx)));
        }

        public static Config<TValue> CombineWith<TValue, TFirst, TSecond>(this Config<TFirst> first, Config<TSecond> second, Func<TFirst, TSecond, Task<TValue>> combine)
        {
            combine.ThrowIfNull(nameof(combine));

            // Don't need to check for first is null because Transform() handles that

            if (second is null)
            {
                return first.Transform(value => combine(value, default));
            }
            return second.RequiresDocument
                ? first.Transform(async (value, doc, ctx) => await combine(value, await second.GetValueAsync(doc, ctx)))
                : first.Transform(async (value, ctx) => await combine(value, await second.GetValueAsync(null, ctx)));
        }

        public static Config<TValue> CombineWith<TValue, TFirst, TSecond>(this Config<TFirst> first, Config<TSecond> second, Func<TFirst, TSecond, IExecutionContext, TValue> combine)
        {
            combine.ThrowIfNull(nameof(combine));

            // Don't need to check for first is null because Transform() handles that

            if (second is null)
            {
                return first.Transform((value, ctx) => combine(value, default, ctx));
            }
            return second.RequiresDocument
                ? first.Transform(async (value, doc, ctx) => combine(value, await second.GetValueAsync(doc, ctx), ctx))
                : first.Transform(async (value, ctx) => combine(value, await second.GetValueAsync(null, ctx), ctx));
        }

        public static Config<TValue> CombineWith<TValue, TFirst, TSecond>(this Config<TFirst> first, Config<TSecond> second, Func<TFirst, TSecond, IExecutionContext, Task<TValue>> combine)
        {
            combine.ThrowIfNull(nameof(combine));

            // Don't need to check for first is null because Transform() handles that

            if (second is null)
            {
                return first.Transform((value, ctx) => combine(value, default, ctx));
            }
            return second.RequiresDocument
                ? first.Transform(async (value, doc, ctx) => await combine(value, await second.GetValueAsync(doc, ctx), ctx))
                : first.Transform(async (value, ctx) => await combine(value, await second.GetValueAsync(null, ctx), ctx));
        }

        public static Config<TValue> CombineWith<TValue, TFirst, TSecond>(this Config<TFirst> first, Config<TSecond> second, Func<TFirst, TSecond, IDocument, IExecutionContext, TValue> combine)
        {
            combine.ThrowIfNull(nameof(combine));

            // Don't need to check for first is null because Transform() handles that

            if (second is null)
            {
                return first.Transform((value, doc, ctx) => combine(value, default, doc, ctx));
            }
            return first.Transform(async (value, doc, ctx) => combine(value, await second.GetValueAsync(doc, ctx), doc, ctx));
        }

        public static Config<TValue> CombineWith<TValue, TFirst, TSecond>(this Config<TFirst> first, Config<TSecond> second, Func<TFirst, TSecond, IDocument, IExecutionContext, Task<TValue>> combine)
        {
            combine.ThrowIfNull(nameof(combine));

            // Don't need to check for first is null because Transform() handles that

            if (second is null)
            {
                return first.Transform((value, doc, ctx) => combine(value, default, doc, ctx));
            }
            return first.Transform(async (value, doc, ctx) => await combine(value, await second.GetValueAsync(doc, ctx), doc, ctx));
        }
    }
}
