using System;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    public static class MemoryExtensions
    {
        public static bool StartsWith(this in ReadOnlyMemory<char> item, in ReadOnlyMemory<char> value) =>
            value.Length > item.Length ? false : item.Slice(0, value.Length).Span.SequenceEqual(value.Span);

        public static bool StartsWith(this IEnumerable<ReadOnlyMemory<char>> items, IEnumerable<ReadOnlyMemory<char>> values)
        {
            IEnumerator<ReadOnlyMemory<char>> valueEnumerator = values.GetEnumerator();
            foreach (ReadOnlyMemory<char> item in items)
            {
                if (valueEnumerator.MoveNext())
                {
                    if (!item.SequenceEqual(valueEnumerator.Current))
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            return !valueEnumerator.MoveNext();
        }

        public static bool SequenceEqual(this in ReadOnlyMemory<char> item, in ReadOnlyMemory<char> value) => item.Span.SequenceEqual(value.Span);

        public static IEnumerable<string> ToStrings(this IEnumerable<ReadOnlyMemory<char>> items) => items.Select(x => x.ToString());

        // Below extensions copied from SpanExtensions

        public static bool Replace(
            this in Memory<char> str,
            char oldChar,
            char newChar)
        {
            bool replaced = false;
            for (int c = 0; c < str.Length; c++)
            {
                if (str.Span[c] == oldChar)
                {
                    str.Span[c] = newChar;
                    replaced = true;
                }
            }
            return replaced;
        }

        public static bool Replace(
            this in Memory<char> str,
            char[] oldChars,
            char newChar)
        {
            bool replaced = false;
            for (int c = 0; c < str.Length; c++)
            {
                foreach (char oldChar in oldChars)
                {
                    if (str.Span[c] == oldChar)
                    {
                        str.Span[c] = newChar;
                        replaced = true;
                        break;
                    }
                }
            }
            return replaced;
        }

        public static Memory<char> Append(this in Memory<char> str, params char[] chars)
        {
            Memory<char> appended = new char[str.Length + chars.Length];
            str.CopyTo(appended);
            for (int c = 0; c < chars.Length; c++)
            {
                appended.Span[str.Length + c] = chars[c];
            }
            return appended;
        }

        /// <summary>
        /// Removes characters from a character span by copying over them and returning a slice.
        /// This is destructive and the original span should no longer be used.
        /// </summary>
        public static Memory<char> Remove(this in Memory<char> str, int startIndex, int length)
        {
            if (length == 0)
            {
                return str;
            }
            if (startIndex < 0 || startIndex > str.Length - 1)
            {
                throw new ArgumentException(nameof(startIndex));
            }
            if (length < 0 || startIndex + length > str.Length)
            {
                throw new ArgumentException(nameof(length));
            }

            // Optimization to slice off the beginning or end without copying
            if (startIndex == 0)
            {
                return str.Slice(length);
            }
            if (startIndex + length == str.Length)
            {
                return str.Slice(0, startIndex);
            }

            for (int c = 0; startIndex + length + c < str.Length; c++)
            {
                str.Span[startIndex + c] = str.Span[startIndex + length + c];
            }
            return str.Slice(0, str.Length - length);
        }
    }
}
