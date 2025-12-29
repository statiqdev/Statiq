using System.Collections.Generic;

namespace Statiq.Lunr
{
    internal class StopWordFilter : global::Lunr.StopWordFilterBase
    {
        private readonly global::Lunr.ISet<string> _stopWords;

        public StopWordFilter(IEnumerable<string> stopWords)
        {
            _stopWords = new global::Lunr.Set<string>(stopWords /* TODO: on next lunr-core update StringComparer.OrdinalIgnoreCase */);
        }

        protected override global::Lunr.ISet<string> StopWords => _stopWords;
    }
}
