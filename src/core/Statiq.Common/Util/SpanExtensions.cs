using System;
using System.Text;

namespace Statiq.Common
{
    public static class SpanExtensions
    {
        // See https://github.com/dotnet/runtime/issues/29758#issuecomment-498645607

        public static bool Replace(
            this in Span<char> str,
            char oldChar,
            char newChar)
        {
            bool replaced = false;
            for (int c = 0; c < str.Length; c++)
            {
                if (str[c] == oldChar)
                {
                    str[c] = newChar;
                    replaced = true;
                }
            }
            return replaced;
        }

        public static bool Replace(
            this in Span<char> str,
            char[] oldChars,
            char newChar)
        {
            bool replaced = false;
            for (int c = 0; c < str.Length; c++)
            {
                foreach (char oldChar in oldChars)
                {
                    if (str[c] == oldChar)
                    {
                        str[c] = newChar;
                        replaced = true;
                        break;
                    }
                }
            }
            return replaced;
        }

        public static Span<char> Append(this in Span<char> str, params char[] chars)
        {
            Span<char> appended = new char[str.Length + chars.Length];
            str.CopyTo(appended);
            for (int c = 0; c < chars.Length; c++)
            {
                appended[str.Length + c] = chars[c];
            }
            return appended;
        }

        /// <summary>
        /// Removes characters from a character span by copying over them and returning a slice.
        /// This is destructive and the original span should no longer be used.
        /// </summary>
        public static Span<char> Remove(this in Span<char> str, int startIndex, int length)
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
                str[startIndex + c] = str[startIndex + length + c];
            }
            return str.Slice(0, str.Length - length);
        }
    }
}