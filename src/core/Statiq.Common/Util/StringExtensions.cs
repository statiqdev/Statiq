using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Statiq.Common
{
    public static class StringExtensions
    {
        private static readonly Regex RemoveHtmlAndSpecialCharsRegex = new Regex(@"<[^>]+>|&[a-zA-Z]{2,};|&#\d+;|[^a-zA-Z-# ]", RegexOptions.Compiled);

        /// <summary>
        /// Removes HTML and special characters from a string and collapses adjacent spaces to a single space.
        /// </summary>
        /// <param name="str">The string to remove HTML and special characters from.</param>
        /// <param name="replacement">An optional replacement string (a space by default).</param>
        /// <returns>The string with HTML and special characters removed.</returns>
        public static string RemoveHtmlAndSpecialChars(this string str, string replacement = " ") =>
            str is object
                ? string.Join(" ", RemoveHtmlAndSpecialCharsRegex.Replace(str, replacement ?? string.Empty).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                : str;

        public static string RemoveStart(this string str, string value) =>
            str is object && value is object && str.StartsWith(value) ? str.Substring(value.Length) : str;

        public static string RemoveStart(this string str, string value, StringComparison comparisonType) =>
            str is object && value is object && str.StartsWith(value, comparisonType) ? str.Substring(value.Length) : str;

        public static string RemoveEnd(this string str, string value) =>
            str is object && value is object && str.EndsWith(value) ? str.Substring(0, str.Length - value.Length) : str;

        public static string RemoveEnd(this string str, string value, StringComparison comparisonType) =>
            str is object && value is object && str.EndsWith(value, comparisonType) ? str.Substring(0, str.Length - value.Length) : str;

        public static string ToKebab(this string str) =>
            Regex.Replace(
                str,
                "([a-z])([A-Z])",
                "$1-$2")
                .ToLower();

        public static string ToLowerCamelCase(this string str) => new string(ToLowerCamelCaseChars(str).ToArray());

        private static IEnumerable<char> ToLowerCamelCaseChars(string str)
        {
            if (str.Length > 0)
            {
                yield return char.ToLowerInvariant(str[0]);
                for (int c = 1; c < str.Length; c++)
                {
                    yield return str[c];
                }
            }
        }

        /// <summary>
        /// Allocates a <see cref="Span{T}"/> and copies the characters of the string into it.
        /// </summary>
        /// <param name="str">The string to copy to a <see cref="Span{T}"/>.</param>
        /// <returns>A new <see cref="Span{T}"/> with the characters of the string.</returns>
        public static Span<char> ToSpan(this string str)
        {
            Span<char> chars = new char[str.Length];
            str.AsSpan().CopyTo(chars);
            return chars;
        }

        /// <summary>
        /// Allocates a <see cref="Memory{T}"/> and copies the characters of the string into it.
        /// </summary>
        /// <param name="str">The string to copy to a <see cref="Memory{T}"/>.</param>
        /// <returns>A new <see cref="Memory{T}"/> with the characters of the string.</returns>
        public static Memory<char> ToMemory(this string str)
        {
            Memory<char> chars = new char[str.Length];
            str.AsMemory().CopyTo(chars);
            return chars;
        }
    }
}
