using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public interface IScriptHelper
    {
        /// <summary>
        /// The script prefix to use for script strings that cache their
        /// values after the first evaluation.
        /// </summary>
        public const string CachedValueScriptStringPrefix = "=>";

        /// <summary>
        /// The script prefix to use for script strings that do not
        /// cache their values after the first evaluation and re-evaluate
        /// the value every time it's fetched.
        /// </summary>
        public const string UncachedValueScriptStringPrefix = "->";

        /// <summary>
        /// Compiles, caches, and evaluates a script.
        /// </summary>
        /// <remarks>
        /// The script compilation is cached, so as long as the script remains the same
        /// it will not need to be recompiled.
        /// </remarks>
        /// <param name="code">The code to compile.</param>
        /// <param name="metadata">
        /// The metadata used to construct the script. Metadata items are exposed a properties with
        /// the name of the key and can be used directly in the script.
        /// </param>
        /// <returns>Raw assembly bytes.</returns>
        Task<object> EvaluateAsync(string code, IMetadata metadata);

        /// <summary>
        /// Checks if the string is a "script" string (it starts with <c>=></c>)
        /// and trims the string if it is.
        /// </summary>
        /// <param name="str">The candidate string.</param>
        /// <param name="script">The trimmed script.</param>
        /// <returns>
        /// <c>true</c> if the candidate string is a script string that should be cached,
        /// <c>false</c> if the candidate string is a script string that should not be cached,
        /// and null otherwise.</returns>
        public static bool? TryGetScriptString(string str, out string script)
        {
            if (TryGetScriptString(str, true, out script))
            {
                return true;
            }

            if (TryGetScriptString(str, false, out script))
            {
                return false;
            }

            script = null;
            return null;
        }

        private static bool TryGetScriptString(string str, bool cacheValue, out string script)
        {
            script = null;
            string prefix = cacheValue ? CachedValueScriptStringPrefix : UncachedValueScriptStringPrefix;
            int c = 0;
            int s = 0;
            for (; c < str.Length; c++)
            {
                if (s < prefix.Length)
                {
                    if (s == 0 && char.IsWhiteSpace(str[c]))
                    {
                        continue;
                    }
                    if (str[c] == prefix[s])
                    {
                        s++;
                        continue;
                    }
                }
                break;
            }
            if (s == prefix.Length)
            {
                script = str[c..];
                return true;
            }
            return false;
        }

        IEnumerable<Assembly> GetScriptReferences();

        IEnumerable<string> GetScriptNamespaces();
    }
}