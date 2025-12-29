using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Parsers.Inlines;

namespace Statiq.Markdown.EscapeAt
{
    /// <summary>
    /// This parser identifies escaped @ symbols and keeps the escape character so that
    /// the <see cref="EscapeAtWriter"/> will see it. Otherwise, the
    /// <see cref="EscapeInlineParser"/> will consume the escape slash and it won't make
    /// it to either this parser or to the output.
    /// </summary>
    internal class EscapeAtParser : InlineParser
    {
        public EscapeAtParser()
        {
            OpeningCharacters = new[] { '\\' };
        }

        public override bool Match(InlineProcessor processor, ref StringSlice slice)
        {
            int startPosition = slice.Start;

            if (slice.NextChar() != '@')
            {
                return false;
            }

            processor.Inline = new EscapeAtInline()
            {
                Span =
                {
                    Start = processor.GetSourcePosition(startPosition, out int line, out int column),
                },
                Line = line,
                Column = column
            };
            processor.Inline.Span.End = processor.Inline.Span.Start + 1;
            slice.SkipChar();
            return true;
        }
    }
}