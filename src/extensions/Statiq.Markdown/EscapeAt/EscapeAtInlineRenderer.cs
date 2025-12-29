using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Statiq.Markdown.EscapeAt
{
    internal class EscapeAtInlineRenderer : HtmlObjectRenderer<EscapeAtInline>
    {
        private static readonly StringSlice Content = new StringSlice("\\@");

        protected override void Write(HtmlRenderer renderer, EscapeAtInline obj) => renderer.Write(Content);
    }
}