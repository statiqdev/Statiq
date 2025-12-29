using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    public class ExplicitGrouping<TKey, TValue> : IGrouping<TKey, TValue>
    {
        private readonly IEnumerable<TValue> _values;

        public ExplicitGrouping(TKey key, IEnumerable<TValue> values)
        {
            _values = values;
            Key = key;
        }

        public IEnumerator<TValue> GetEnumerator() => _values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TKey Key { get; }
    }
}