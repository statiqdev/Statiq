using System;
using System.Collections.Generic;

namespace Statiq.Lunr
{
    internal class DelegateStemmer : global::Lunr.StemmerBase
    {
        private readonly Func<string, string> _stem;

        public DelegateStemmer(Func<string, string> stem)
        {
            _stem = stem;
        }

        public override string Stem(string w) => _stem(w);
    }
}
