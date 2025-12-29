using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// A special shortcode that will output whatever is in it's content.
    /// </summary>
    /// <remarks>
    /// This will not evaluate nested shortcodes and is useful
    /// for escaping shortcode syntax.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;?# Raw ?>&lt;?# ThisWillBeOutputVerbatim ?>&lt;?#/ Raw ?>
    /// </code>
    /// </example>
    public class RawShortcode : SyncShortcode
    {
        public const string RawShortcodeName = "Raw";

        public override ShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) => content;
    }
}
