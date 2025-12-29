using AngleSharp.Dom;
using AngleSharp.Html;
using AngleSharp;

namespace Statiq.Common
{
    /// <summary>
    /// This uncomments shortcode processing instructions which currently get parsed as comments
    /// in AngleSharp (see https://github.com/Wyamio/Statiq/issues/784 and
    /// https://github.com/AngleSharp/AngleSharp/pull/762). It also ensures raw text
    /// content isn't escaped so that character escapes like &#64; (for the at symbol) don't end
    /// up getting encoded back to the original symbol, which in that case would break Razor processing.
    /// </summary>
    public class StatiqMarkupFormatter : IMarkupFormatter
    {
        private static readonly IMarkupFormatter Formatter = HtmlMarkupFormatter.Instance;

        public static readonly IMarkupFormatter Instance = new StatiqMarkupFormatter();

        public string CloseTag(IElement element, bool selfClosing) => Formatter.CloseTag(element, selfClosing);

        public string Doctype(IDocumentType doctype) => Formatter.Doctype(doctype);

        public string OpenTag(IElement element, bool selfClosing) => Formatter.OpenTag(element, selfClosing);

        // Prevent escaping by returning the raw text
        public string Text(ICharacterData text) => text.Data;

        public string Processing(IProcessingInstruction processing) => Formatter.Processing(processing);

        public string LiteralText(ICharacterData text) => Formatter.LiteralText(text);

        public string Comment(IComment comment)
        {
            if (comment.Data.StartsWith("?") && comment.Data.EndsWith("?"))
            {
                // This was probably a shortcode, so uncomment it
                return $"<{comment.Data}>";
            }
            return Formatter.Comment(comment);
        }
    }
}