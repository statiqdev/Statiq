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
            htmlRenderer.Writer = new EscapeAtWriter(htmlRenderer.Writer);
        }
    }
}