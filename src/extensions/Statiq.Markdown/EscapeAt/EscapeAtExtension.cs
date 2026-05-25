using Markdig;
using Markdig.Parsers.Inlines;
using Markdig.Renderers;

namespace Statiq.Markdown.EscapeAt
{
    internal class EscapeAtExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            pipeline.InlineParsers.InsertBefore<EscapeInlineParser>(new EscapeAtParser());
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.AddIfNotAlready(new EscapeAtInlineRenderer());
            HtmlRenderer htmlRenderer = (HtmlRenderer)renderer;

             // Setting the Writer for the HTML renderer will reset the new line character to the Markdig default "\n"
            // If we've modified the intended newline character (e.g., via the Markdig ConfigureNewLineExtension),
            // we'll need to preserve that setting.
            string newLine = htmlRenderer.Writer.NewLine;

            htmlRenderer.Writer = new EscapeAtWriter(htmlRenderer.Writer);
            htmlRenderer.Writer.NewLine = newLine;
        }
    }
}