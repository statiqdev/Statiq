using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static partial class Config
    {
        /// <summary>
        /// Creates a config value from a delegate that uses the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Func<IExecutionContext, TValue> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>((_, ctx) => Task.FromResult(func(ctx)), false);
        }

        /// <summary>
        /// Creates a config value from a delegate that uses the execution context.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="func">The delegate that produces the config value.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Func<IExecutionContext, Task<TValue>> func)
        {
            func.ThrowIfNull(nameof(func));
            return new Config<TValue>((_, ctx) => func(ctx), false);
        }

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Action<IExecutionContext> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>(
                (__, ctx) =>
                {
                    action(ctx);
                    return Task.FromResult(default(TValue));
                },
                false);
        }

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns
        /// the default value of <typeparamref name="TValue"/>.
        /// </summary>
        /// <typeparam name="TValue">The type of config value.</typeparam>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<TValue> FromContext<TValue>(Func<IExecutionContext, Task> action)
        {
            action.ThrowIfNull(nameof(action));
            return new Config<TValue>(
                async (__, ctx) =>
                {
                    await action(ctx);
                    return default;
                },
                false);
        }

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromContext(Action<IExecutionContext> action) =>
            FromContext<object>(action);

        /// <summary>
        /// Creates a config value from an action that uses the execution context and returns null.
        /// </summary>
        /// <param name="action">A delegate action to evaluate.</param>
        /// <returns>A config object.</returns>
        public static Config<object> FromContext(Func<IExecutionContext, Task> action) =>
            FromContext<object>(action);
    }
}
