using System.IO;
using AngleSharp;

namespace Statiq.Common
{
    public static class IMarkupFormattableExtensions
    {
        /// <summary>
        /// Calls <see cref="IMarkupFormattable.ToHtml"/> using the <see cref="StatiqMarkupFormatter"/>
        /// so that processing instructions and character references are properly handled. This should be
        /// used for serialization when <see cref="IDocumentHtmlExtensions.ParseHtmlAsync"/> is used.
        /// </summary>
        public static string ToFormattedHtml(this IMarkupFormattable markupFormattable) =>
            markupFormattable.ToHtml(StatiqMarkupFormatter.Instance);

        /// <summary>
        /// Calls <see cref="IMarkupFormattable.ToHtml"/> using the <see cref="StatiqMarkupFormatter"/>
        /// so that processing instructions and character references are properly handled. This should be
        /// used for serialization when <see cref="IDocumentHtmlExtensions.ParseHtmlAsync"/> is used.
        /// </summary>
        public static void ToFormattedHtml(this IMarkupFormattable markupFormattable, TextWriter writer) =>
            markupFormattable.ToHtml(writer, StatiqMarkupFormatter.Instance);
    }
}