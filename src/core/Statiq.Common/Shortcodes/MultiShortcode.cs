using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for shortcodes.
    /// </summary>
    public abstract class MultiShortcode : IShortcode
    {
        /// <inheritdoc />
        public abstract Task<IEnumerable<ShortcodeResult>> ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context);
    }
}
