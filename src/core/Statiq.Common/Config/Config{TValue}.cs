using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A union configuration value that can be either a delegate
    /// that uses a document and context or a simple value. Use the factory methods
    /// in the <see cref="Config"/> class to create one. Instances can also be created
    /// through implicit casting from the value type. Note that due to overload ambiguity,
    /// if a value type of object is used, then all overloads should also be <see cref="Config{T}"/>.
    /// </summary>
    /// <typeparam name="TValue">The value type for this config data.</typeparam>
    public class Config<TValue> : IConfig
    {
        private readonly Func<IDocument, IExecutionContext, Task<TValue>> _delegate;

        internal Config(Func<IDocument, IExecutionContext, Task<TValue>> func, bool requiresDocument = true)
        {
            _delegate = func;
            RequiresDocument = requiresDocument;
        }

        public bool RequiresDocument { get; }

#pragma warning disable CS0618 // Type or member is obsolete
        Task<object> IConfig.GetValueAsync(IDocument document, IExecutionContext context) =>
            GetAndTransformValueAsync(document, context).FromDerivedAsync<object, TValue>();
#pragma warning restore CS0618 // Type or member is obsolete

        // This should only be accessed via the extension method(s) that guard against null so that null coalescing operators can be used
        // See the discussion at https://github.com/dotnet/roslyn/issues/7171
        [Obsolete("Use config extension methods instead.")]
        internal async Task<TValue> GetAndTransformValueAsync(IDocument document, IExecutionContext context, Func<TValue, TValue> transform = null)
        {
            TValue value = await _delegate(document, context);
            return transform is null ? value : transform(value);
        }

        public static implicit operator Config<TValue>(TValue value) => new Config<TValue>((_, __) => Task.FromResult(value), false);

        // These special casting operators for object variants ensure we don't accidentally "wrap" an existing ContextConfig/DocumentConfig

        public static implicit operator Config<IEnumerable<object>>(Config<TValue> config)
        {
            if (typeof(IEnumerable).IsAssignableFrom(typeof(TValue)))
            {
                return new Config<IEnumerable<object>>(async (doc, ctx) => ((IEnumerable)await config._delegate(doc, ctx))?.Cast<object>(), config.RequiresDocument);
            }
            return new Config<IEnumerable<object>>(async (doc, ctx) => Yield(await config._delegate(doc, ctx)), config.RequiresDocument);
        }

        private static IEnumerable<object> Yield(object value)
        {
            yield return value;
        }

        public static implicit operator Config<object>(Config<TValue> config) =>
            new Config<object>(async (doc, ctx) => await config._delegate(doc, ctx), config.RequiresDocument);
    }
}
