using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for synchronous shortcodes.
    /// </summary>
    public abstract class SyncMultiShortcode : IShortcode
    {
        /// <summary>
        /// Executes the shortcode and returns a <see cref="Stream"/> with the shortcode result content.
        /// </summary>
        /// <param name="args">
        /// The arguments declared with the shortcode. This contains a list of key-value pairs in the order
        /// they appeared in the shortcode declaration. If no key was specified, then the <see cref="KeyValuePair{TKey, TValue}.Key"/>
        /// property will be <c>null</c>.
        /// </param>
        /// <param name="content">The content of the shortcode.</param>
        /// <param name="document">The current document (including metadata from previous shortcodes in the same document).</param>
        /// <param name="context">The current execution context.</param>
        /// <returns>
        /// A collection of shortcode results or <c>null</c> to remove the shortcode from the document without adding replacement content.
        /// </returns>
        public abstract IEnumerable<ShortcodeResult> Execute(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context);

        /// <inheritdoc />
        Task<IEnumerable<ShortcodeResult>> IShortcode.ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context) =>
            Task.FromResult(Execute(args, content, document, context));
    }
}
