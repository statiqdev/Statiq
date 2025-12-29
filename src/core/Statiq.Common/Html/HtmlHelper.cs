using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Statiq.Common
{
    public static class HtmlHelper
    {
        /// <summary>
        /// A default shared <see cref="HtmlParser"/> that does not consume (decode)
        /// character references.
        /// </summary>
        /// <remarks>
        /// The default <see cref="HtmlParser"/> has <see cref="HtmlParserOptions.IsNotConsumingCharacterReferences"/>
        /// set to <c>true</c> so that character references are not decoded when parsing (which is important for passing
        /// encoded characters like <c>@</c> through to engines like Razor that require them to be encoded if literal).
        /// This has the unfortunate side effect of triggering double-encoding on serialization,
        /// see https://github.com/AngleSharp/AngleSharp/issues/396#issuecomment-246106539.
        /// To avoid that, use <see cref="StatiqMarkupFormatter"/> or one of the extensions from
        /// <see cref="IMarkupFormattableExtensions"/> or <see cref="IElementExtensions"/> whenever
        /// serialization needs to be performed from a <see cref="IHtmlDocument"/> obtained from this method.
        /// </remarks>
        public static readonly HtmlParser DefaultHtmlParser = new HtmlParser(
            new HtmlParserOptions
            {
                IsNotConsumingCharacterReferences = true
            });
    }
}