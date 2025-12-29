using System.Collections.Generic;

namespace Statiq.Lunr
{
    internal class NoStemmer : global::Lunr.StemmerBase
    {
        public override string Stem(string w) => w;
    }
}
