using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Renders the metadata value with the given key from the current document.
    /// </summary>
    /// <remarks>
    /// The metadata value will be rendered as a string. If no value exists with the
    /// specified key, nothing will be rendered. In addition to using the shortcode
    /// by the <c>Meta</c> name like <c>&lt;?# Meta key /?&gt;</c>, this shortcode can also be used
    /// with a special syntax: <c>&lt;?#= key /?&gt;</c>.
    /// </remarks>
    /// <parameter>The key of the metadata value to render.</parameter>
    public class MetaShortcode : SyncShortcode
    {
        /// <inheritdoc />
        public override ShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
            document.GetString(args.SingleValue());
    }
}
