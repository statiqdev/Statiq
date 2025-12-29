using AngleSharp;
using AngleSharp.Dom;

namespace Statiq.Common
{
    public static class IElementExtensions
    {
        /// <summary>
        /// Calls <see cref="IMarkupFormattable.ToHtml"/> using the <see cref="StatiqMarkupFormatter"/>
        /// so that processing instructions and character references are properly handled. This should be
        /// used for serialization when <see cref="IDocumentHtmlExtensions.ParseHtmlAsync"/> is used.
        /// </summary>
        public static string FormattedInnerHtml(this IElement element) =>
            element.ChildNodes.ToFormattedHtml();

        /// <summary>
        /// Calls <see cref="IMarkupFormattable.ToHtml"/> using the <see cref="StatiqMarkupFormatter"/>
        /// so that processing instructions and character references are properly handled. This should be
        /// used for serialization when <see cref="IDocumentHtmlExtensions.ParseHtmlAsync"/> is used.
        /// </summary>
        public static string FormattedOuterHtml(this IElement element) =>
            element.ToFormattedHtml();
    }
}