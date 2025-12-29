using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IShortcodeCollectionExtensions
    {
        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name and removing a trailing "Shortcode" from the type name.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="shortcodeType">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public static void Add(this IShortcodeCollection shortcodes, Type shortcodeType) =>
            shortcodes.Add(shortcodeType?.Name.RemoveEnd("Shortcode", StringComparison.OrdinalIgnoreCase), shortcodeType);

        /// <summary>
        /// Adds a shortcode by type, inferring the name from the type name.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        /// <param name="shortcodes">The shortcodes.</param>
        public static void Add<TShortcode>(this IShortcodeCollection shortcodes)
            where TShortcode : IShortcode =>
            shortcodes.Add<TShortcode>(typeof(TShortcode).Name.RemoveEnd("Shortcode", StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <typeparam name="TShortcode">The type of the shortcode to add.</typeparam>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        public static void Add<TShortcode>(this IShortcodeCollection shortcodes, string name)
            where TShortcode : IShortcode
        {
            shortcodes.ThrowIfNull(nameof(shortcodes));
            shortcodes.Add(name, () => Activator.CreateInstance<TShortcode>());
        }

        /// <summary>
        /// Adds a shortcode by type.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcodeType">The type of the shortcode to add (must implement <see cref="IShortcode"/>).</param>
        public static void Add(this IShortcodeCollection shortcodes, string name, Type shortcodeType)
        {
            shortcodes.ThrowIfNull(nameof(shortcodes));
            shortcodeType.ThrowIfNull(nameof(shortcodeType));
            if (!typeof(IShortcode).IsAssignableFrom(shortcodeType))
            {
                throw new ArgumentException("The type must implement " + nameof(IShortcode), nameof(shortcodeType));
            }
            shortcodes.Add(name, () => (IShortcode)Activator.CreateInstance(shortcodeType));
        }

        /// <summary>
        /// Adds a shortcode and uses a <see cref="Config{ShortcodeResult}"/> to determine
        /// the shortcode result.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">The content of the shortcode.</param>
        public static void Add(this IShortcodeCollection shortcodes, string name, Config<ShortcodeResult> shortcode) =>
            shortcodes.Add(name, async (_, __, doc, ctx) => shortcode is null ? null : await shortcode.GetValueAsync(doc, ctx));

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs and a <see cref="ShortcodeResult"/> as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, ShortcodeResult> shortcode)
        {
            shortcodes.ThrowIfNull(nameof(shortcodes));
            shortcodes.Add(name, (args, content, doc, ctx) => Task.FromResult(shortcode?.Invoke(args, content, doc, ctx)));
        }

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs and a <see cref="ShortcodeResult"/> as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<ShortcodeResult>> shortcode)
        {
            shortcodes.ThrowIfNull(nameof(shortcodes));
            shortcodes.Add(name, () => new FuncShortcode(shortcode));
        }

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs and a <see cref="ShortcodeResult"/> collection as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IEnumerable<ShortcodeResult>> shortcode)
        {
            shortcodes.ThrowIfNull(nameof(shortcodes));
            shortcodes.Add(name, (args, content, doc, ctx) => Task.FromResult(shortcode?.Invoke(args, content, doc, ctx)));
        }

        /// <summary>
        /// Adds a shortcode that determines the result content
        /// using the declared arguments and content and the current document and execution context.
        /// </summary>
        /// <param name="shortcodes">The shortcodes.</param>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcode">
        /// A function that has the declared arguments and content and the current document and execution context as inputs and a <see cref="ShortcodeResult"/> collection as an output.
        /// </param>
        public static void Add(
            this IShortcodeCollection shortcodes,
            string name,
            Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<ShortcodeResult>>> shortcode)
        {
            shortcodes.ThrowIfNull(nameof(shortcodes));
            shortcodes.Add(name, () => new FuncShortcode(shortcode));
        }
    }
}
