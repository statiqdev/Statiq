using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static partial class Config
    {
        /// <summary>
        /// Creates a config value from a delegate that uses a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, TValue> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>((doc, _) => Task.FromResult(func(doc)));
        }

        /// <summary>
        /// Creates a config value from a delegate that uses a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, Task<TValue>> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>((doc, _) => func(doc));
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Action<IDocument> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>((doc, __) =>
            {
                action(doc);
                return Task.FromResult(default(TValue));
            });
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, Task> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>(async (doc, __) =>
            {
                await action(doc);
                return default;
            });
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Action<IDocument> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses a document and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Func<IDocument, Task> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value from a delegate that uses a document and the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, TValue> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>((doc, ctx) => Task.FromResult(func(doc, ctx)));
        }

        /// <summary>
        /// Creates a config value from a delegate that uses a document and the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, Task<TValue>> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>(func);
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Action<IDocument, IExecutionContext> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>((doc, ctx) =>
            {
                action(doc, ctx);
                return Task.FromResult(default(TValue));
            });
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(Func<IDocument, IExecutionContext, Task> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>(async (doc, ctx) =>
            {
                await action(doc, ctx);
                return default;
            });
        }

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Action<IDocument, IExecutionContext> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses a document and the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(Func<IDocument, IExecutionContext, Task> action) =>
            FromDocument<object>(action);

        /// <summary>
        /// Creates a config value by getting the metadata value of a specified key from a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(string key) => new Config<TValue>((doc, _) =>
        Task.FromResult(doc.Get<TValue>(key)));

        /// <summary>
        /// Creates a config value by getting the metadata value of a specified key from a document.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found, is null, or cannot be converted to <typeparamref name="TValue"/>.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromDocument<TValue>(string key, TValue defaultValue) => new Config<TValue>((doc, _) =>
        Task.FromResult(doc.Get(key, defaultValue)));

        /// <summary>
        /// Creates a config value by getting the metadata value of a specified key from a document.
        /// </summary>
        /// <param name="key">The metadata key to get the value from.</param>
        /// <param name="defaultValue">The default value to use if the key cannot be found or is null.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromDocument(string key, object defaultValue = null) => new Config<object>((doc, _) =>
        Task.FromResult(doc.Get(key, defaultValue)));
    }
}
