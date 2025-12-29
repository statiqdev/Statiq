using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for synchronous simple shortcodes that return string content.
    /// </summary>
    public abstract class SyncShortcode : IShortcode
    {
        public abstract ShortcodeResult Execute(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context);

        /// <inheritdoc />
        Task<IEnumerable<ShortcodeResult>> IShortcode.ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context)
        {
            ShortcodeResult result = Execute(args, content, document, context);
            return Task.FromResult<IEnumerable<ShortcodeResult>>(result is null ? null : new[] { result });
        }
    }
}
