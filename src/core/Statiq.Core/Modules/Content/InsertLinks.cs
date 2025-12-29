using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces occurrences of specified strings with HTML links.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module is smart enough to only look in specified HTML
    /// elements (p by default). You can supply an alternate query selector to
    /// narrow the search scope to different container elements or to those elements that contain
    /// (or don't contain) a CSS class, etc. It also won't generate an HTML link if the replacement
    /// text is already found in another link.
    /// </para>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <category name="Content" />
    public class InsertLinks : ParallelModule
    {
        private static readonly HtmlParser HtmlParser = new HtmlParser();

        // Key = text to replace, Value = url
        private readonly Config<IDictionary<string, string>> _links;
        private readonly IDictionary<string, string> _extraLinks = new Dictionary<string, string>();
        private readonly List<char> _startWordSeparators = new List<char>();
        private readonly List<char> _endWordSeparators = new List<char>();
        private string _querySelector = "p";
        private bool _matchOnlyWholeWord = false;

        /// <summary>
        /// Creates the module without any initial mappings. Use <c>AddLink(...)</c> to add mappings with fluent methods.
        /// </summary>
        public InsertLinks()
            : this(new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Specifies a dictionary of link mappings given an <see cref="Common.IDocument"/> and <see cref="IExecutionContext"/>. The return
        /// value is expected to be a <c>IDictionary&lt;string, string&gt;</c>. The keys specify strings to search for in the
        /// HTML content and the values specify what should be placed in the <c>href</c> attribute. This allows you
        /// to specify a different mapping for each input document.
        /// </summary>
        /// <param name="links">A delegate that returns a dictionary of link mappings.</param>
        public InsertLinks(Config<IDictionary<string, string>> links)
        {
            _links = links;
        }

        /// <summary>
        /// Allows you to specify an alternate query selector.
        /// </summary>
        /// <param name="querySelector">The query selector to use.</param>
        /// <returns>The current instance.</returns>
        public InsertLinks WithQuerySelector(string querySelector)
        {
            _querySelector = querySelector ?? "p";
            return this;
        }

        /// <summary>
        /// Adds an additional link to the mapping. This can be used whether or not you specify a mapping in the constructor.
        /// </summary>
        /// <param name="text">The text to search for.</param>
        /// <param name="link">The link to insert.</param>
        /// <returns>The current instance.</returns>
        public InsertLinks WithLink(string text, string link)
        {
            _extraLinks[text] = link;
            return this;
        }

        /// <summary>
        /// Forces the string search to only consider whole words (it will not add a link in the middle of a word).
        /// By default whole words are determined by testing for white space.
        /// </summary>
        /// <param name="matchOnlyWholeWord">If set to <c>true</c> the module will only insert links at word boundaries.</param>
        /// <returns>The current instance.</returns>
        public InsertLinks WithMatchOnlyWholeWord(bool matchOnlyWholeWord = true)
        {
            _matchOnlyWholeWord = matchOnlyWholeWord;
            return this;
        }

        /// <summary>
        /// Adds additional word separator characters when limiting matches to whole words only.
        /// These additional characters are in addition to the default of splitting words at white space.
        /// </summary>
        /// <param name="wordSeparators">Additional word separators that should be considered for the start and end of a word.</param>
        /// <returns>The current instance.</returns>
        public InsertLinks WithWordSeparators(params char[] wordSeparators)
        {
            _startWordSeparators.AddRange(wordSeparators);
            _endWordSeparators.AddRange(wordSeparators);
            return this;
        }

        /// <summary>
        /// Adds additional start word separator characters when limiting matches to whole words only.
        /// These additional characters are in addition to the default of splitting words at white space.
        /// </summary>
        /// <param name="startWordSeparators">Additional word separators that should be considered for the start of a word.</param>
        /// <returns>The current instance.</returns>
        public InsertLinks WithStartWordSeparators(params char[] startWordSeparators)
        {
            _startWordSeparators.AddRange(startWordSeparators);
            return this;
        }

        /// <summary>
        /// Adds additional end word separator characters when limiting matches to whole words only.
        /// These additional characters are in addition to the default of splitting words at white space.
        /// </summary>
        /// <param name="endWordSeparators">Additional word separators that should be considered for the end of a word.</param>
        /// <returns>The current instance.</returns>
        public InsertLinks WithEndWordSeparators(params char[] endWordSeparators)
        {
            _endWordSeparators.AddRange(endWordSeparators);
            return this;
        }

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteInputAsync(Common.IDocument input, IExecutionContext context)
        {
            try
            {
                // Get the links and HTML decode the keys (if they're encoded) - we'll also decode HTML
                // for checking and replacement to make sure everything matches, and then (re)encode it all
                IDictionary<string, string> links = await _links.GetValueAsync(input, context, v => _extraLinks
                    .Concat(v.Where(l => !_extraLinks.ContainsKey(l.Key)))
                    .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                    .ToDictionary(z => WebUtility.HtmlDecode(z.Key), z => $"<a href=\"{z.Value}\">{z.Key}</a>"));

                // Enumerate all elements that match the query selector not already in a link element
                List<KeyValuePair<IText, string>> replacements = new List<KeyValuePair<IText, string>>();
                IHtmlDocument htmlDocument = await input.ParseHtmlAsync();
                foreach (IElement element in htmlDocument.QuerySelectorAll(_querySelector).Where(t => !t.Ancestors<IHtmlAnchorElement>().Any()))
                {
                    // Enumerate all descendant text nodes not already in a link element
                    foreach (IText text in element.Descendents().OfType<IText>().Where(t => !t.Ancestors<IHtmlAnchorElement>().Any()))
                    {
                        if (ReplaceStrings(text, links, out string newText))
                        {
                            // Only perform replacement if the text content changed
                            replacements.Add(new KeyValuePair<IText, string>(text, newText));
                        }
                    }
                }

                // Perform the replacements if there were any, otherwise just return the same document
                if (replacements.Count > 0)
                {
                    foreach (KeyValuePair<IText, string> replacement in replacements)
                    {
                        replacement.Key.Replace(HtmlHelper.DefaultHtmlParser.ParseFragment(replacement.Value, replacement.Key.ParentElement).ToArray());
                    }
                    return input.Clone(context.GetContentProvider(htmlDocument)).Yield();
                }
            }
            catch (Exception ex)
            {
                context.LogWarning("Exception while parsing HTML for {0}: {1}", input.ToSafeDisplayString(), ex.Message);
            }

            return input.Yield();
        }

        private bool ReplaceStrings(IText textNode, IDictionary<string, string> map, out string newText)
        {
            // Track where character references occur
            List<int> characterEncodingPositions = new List<int>();
            int cr = textNode.Text.IndexOf("&#");
            int extraChars = 0; // These will be replaced with a single char, so subtract the extra chars of the encoding
            while (cr >= 0)
            {
                characterEncodingPositions.Add(cr - extraChars);
                extraChars += textNode.Text.IndexOf(';', cr) - cr;
                if (cr + 1 < textNode.Text.Length - 1)
                {
                    cr = textNode.Text.IndexOf("&#", cr + 1);
                }
                else
                {
                    cr = -1;
                }
            }

            // Decode any encoded HTML so that it matched the search strings
            string decodedText = WebUtility.HtmlDecode(textNode.Text);
            SubstringSegment originalSegment = new SubstringSegment(0, decodedText.Length);
            List<Segment> segments = new List<Segment>()
            {
                originalSegment
            };

            // Perform replacements
            foreach (KeyValuePair<string, string> kvp in map.OrderByDescending(x => x.Key.Length))
            {
                int c = 0;
                while (c < segments.Count)
                {
                    int index = segments[c].IndexOf(kvp.Key, 0, ref decodedText);
                    while (index >= 0)
                    {
                        if (CheckWordSeparators(
                            ref decodedText,
                            segments[c].StartIndex,
                            segments[c].StartIndex + segments[c].Length - 1,
                            index,
                            index + kvp.Key.Length - 1))
                        {
                            // Insert the new content
                            Segment replacing = segments[c];
                            segments[c] = new ReplacedSegment(kvp.Value);

                            // Insert segment before the match
                            if (index > replacing.StartIndex)
                            {
                                segments.Insert(c, new SubstringSegment(replacing.StartIndex, index - replacing.StartIndex));
                                c++;
                            }

                            // Insert segment after the match
                            int startIndex = index + kvp.Key.Length;
                            int endIndex = replacing.StartIndex + replacing.Length;
                            if (startIndex < endIndex)
                            {
                                Segment segment = new SubstringSegment(startIndex, endIndex - startIndex);
                                if (c + 1 == segments.Count)
                                {
                                    segments.Add(segment);
                                }
                                else
                                {
                                    segments.Insert(c + 1, segment);
                                }
                            }

                            // Adjust character reference positions
                            for (int r = 0; r < characterEncodingPositions.Count; r++)
                            {
                                if (characterEncodingPositions[r] > index - replacing.StartIndex
                                    && characterEncodingPositions[r] <= index)
                                {
                                    // This one was inside the replaced area, so ignore it
                                    characterEncodingPositions[r] = -1;
                                }
                                else if (characterEncodingPositions[r] > index)
                                {
                                    // This comes after the replacement, so adjust it's position
                                    characterEncodingPositions[r] =
                                        characterEncodingPositions[r] + (kvp.Value.Length - kvp.Key.Length);
                                }
                            }

                            // Go to the next segment
                            index = -1;
                        }
                        else
                        {
                            index = segments[c].IndexOf(kvp.Key, index - segments[c].StartIndex + 1, ref decodedText);
                        }
                    }
                    c++;
                }
            }

            // Join and escape non-replaced content
            if (segments.Count > 1 || (segments.Count == 1 && segments[0] != originalSegment))
            {
                newText = string.Concat(segments.Select(x => x.GetText(ref decodedText)));

                // (Re)escape special character encodings
                if (characterEncodingPositions.Count > 0)
                {
                    // Traverse from the back forward so adding extra characters doesn't mess up positions
                    for (int r = characterEncodingPositions.Count - 1; r >= 0; r--)
                    {
                        if (characterEncodingPositions[r] >= 0)
                        {
                            string encoded = $"&#{(int)newText[characterEncodingPositions[r]]};";
                            newText = newText.
                                Remove(characterEncodingPositions[r], 1)
                                .Insert(characterEncodingPositions[r], encoded);
                        }
                    }
                }

                return true;
            }

            newText = null;
            return false;
        }

        private bool CheckWordSeparators(ref string stringToCheck, int substringStartIndex, int substringEndIndex, int matchStartIndex, int matchEndIndex)
        {
            if (_matchOnlyWholeWord)
            {
                return (matchStartIndex <= substringStartIndex || char.IsWhiteSpace(stringToCheck[matchStartIndex - 1]) || _startWordSeparators.Contains(stringToCheck[matchStartIndex - 1]))
                    && (matchEndIndex + 1 > substringEndIndex || char.IsWhiteSpace(stringToCheck[matchEndIndex + 1]) || _endWordSeparators.Contains(stringToCheck[matchEndIndex + 1]));
            }
            return true;
        }

        private abstract class Segment
        {
            public int StartIndex { get; protected set; } = -1;
            public int Length { get; protected set; } = -1;
            public virtual int IndexOf(string value, int startIndex, ref string search) => -1;
            public abstract string GetText(ref string text);
        }

        private class SubstringSegment : Segment
        {
            public SubstringSegment(int startIndex, int length)
            {
                StartIndex = startIndex;
                Length = length;
            }

            public override int IndexOf(string value, int startIndex, ref string search) =>
                search.IndexOf(value, StartIndex + startIndex, Length - startIndex);

            // This re-encodes the segment since encoding is always safe and we don't know if
            // the segment was originally encoded or not since we explicitly decoded it earlier,
            // but special characters don't get encoded here so those would end up as the decoded
            // character unless we manually re-encode them as well (which we do above).
            public override string GetText(ref string text) =>
                WebUtility.HtmlEncode(text.Substring(StartIndex, Length));
        }

        private class ReplacedSegment : Segment
        {
            private readonly string _text;

            public ReplacedSegment(string text)
            {
                _text = text;
            }

            public override string GetText(ref string text) => _text;
        }
    }
}