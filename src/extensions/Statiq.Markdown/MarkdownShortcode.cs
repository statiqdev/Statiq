using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Markdown
{
    /// <summary>
    /// Renders the shortcode content as Markdown.
    /// </summary>
    /// <parameter name="Configuration">
    /// Includes a set of extensions defined as a string, e.g., "pipetables", "citations",
    /// "mathematics", or "abbreviations". Separate different extensions with a '+'.
    /// </parameter>
    /// <parameter name="PrependLinkRoot">
    /// Specifies if the <see cref="Keys.LinkRoot"/> setting must be used to rewrite root-relative links when rendering markdown.
    /// By default, root-relative links, which are links starting with a '/' are left untouched.
    /// When setting this value to <c>true</c>, the <see cref="Keys.LinkRoot"/> setting value is added before the link.
    /// </parameter>
    public class MarkdownShortcode : SyncShortcode
    {
        private const string Configuration = nameof(Configuration);
        private const string PrependLinkRoot = nameof(PrependLinkRoot);
        private const string PassThroughRawFence = nameof(PassThroughRawFence);

        public override ShortcodeResult Execute(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            IMetadataDictionary dictionary = args.ToDictionary(Configuration, PrependLinkRoot, PassThroughRawFence);
            using (StringWriter writer = new StringWriter())
            {
                MarkdownHelper.RenderMarkdown(
                    context,
                    document,
                    content,
                    writer,
                    dictionary.GetBool(PrependLinkRoot),
                    dictionary.GetBool(PassThroughRawFence, true),
                    false,
                    dictionary.GetString(Configuration, MarkdownHelper.DefaultConfiguration),
                    null);
                return writer.ToString();
            }
        }
    }
}