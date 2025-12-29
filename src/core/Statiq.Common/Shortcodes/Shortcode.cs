using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for simple shortcodes that return string content.
    /// </summary>
    public abstract class Shortcode : IShortcode
    {
        public abstract Task<ShortcodeResult> ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context);

        /// <inheritdoc />
        async Task<IEnumerable<ShortcodeResult>> IShortcode.ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context)
        {
            ShortcodeResult result = await ExecuteAsync(args, content, document, context);
            return result is null ? null : new[] { result };
        }
    }
}
