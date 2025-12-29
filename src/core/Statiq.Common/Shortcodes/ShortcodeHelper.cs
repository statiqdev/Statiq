using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Utility methods to help with shortcodes.
    /// </summary>
    public static class ShortcodeHelper
    {
        /// <summary>
        /// Splits shortcode arguments into key/value pairs.
        /// </summary>
        /// <param name="arguments">The string containing the arguments to split.</param>
        /// <param name="start">The index at which to start processing arguments.</param>
        /// <returns>A sequence of argument key/value pairs.</returns>
        public static IEnumerable<KeyValuePair<string, string>> SplitArguments(string arguments, int start)
        {
            int valueStart = -1;
            int valueLength = 0;
            int keyStart = -1;
            int keyLength = 0;
            bool inQuotes = false;
            for (int i = start; i < arguments.Length; i++)
            {
                char c = arguments[i];

                if (c == '\"' && (i == 0 || arguments[i - 1] != '\\'))
                {
                    inQuotes = !inQuotes;

                    if (!inQuotes && valueLength > 0 && (i == arguments.Length - 1 || arguments[i + 1] != '='))
                    {
                        yield return new KeyValuePair<string, string>(
                            keyStart != -1 ? arguments.Substring(keyStart, keyLength).Replace("\\\"", "\"") : null,
                            valueStart != -1 ? arguments.Substring(valueStart, valueLength).Replace("\\\"", "\"") : null);
                        valueStart = -1;
                        valueLength = 0;
                        keyStart = -1;
                        keyLength = 0;
                        continue;
                    }

                    if (inQuotes && valueStart == -1 && i < arguments.Length - 1)
                    {
                        valueStart = i + 1;
                        continue;
                    }

                    // If it's a quote at the end of the string, treat as a normal char
                }

                if (inQuotes)
                {
                    valueLength++;
                }
                else if (c == '=')
                {
                    keyStart = valueStart;
                    keyLength = i > 0 && arguments[i - 1] == '\"' ? valueLength - 1 : valueLength;
                    valueStart = -1;
                    valueLength = 0;
                }
                else if (char.IsWhiteSpace(c))
                {
                    if (valueLength > 0)
                    {
                        yield return new KeyValuePair<string, string>(
                            keyStart != -1 ? arguments.Substring(keyStart, keyLength).Replace("\\\"", "\"") : null,
                            valueStart != -1 ? arguments.Substring(valueStart, valueLength).Replace("\\\"", "\"") : null);
                    }
                    valueLength = 0;
                    valueStart = -1;
                    keyLength = 0;
                    keyStart = -1;
                }
                else
                {
                    if (valueStart == -1)
                    {
                        valueStart = i;
                        valueLength = 1;
                    }
                    else
                    {
                        valueLength++;
                    }
                }
            }

            if (valueLength > 0)
            {
                yield return new KeyValuePair<string, string>(
                    keyStart != -1 ? arguments.Substring(keyStart, keyLength).Replace("\\\"", "\"") : null,
                    valueStart != -1 ? arguments.Substring(valueStart, valueLength).Replace("\\\"", "\"") : null);
            }
        }
    }
}
