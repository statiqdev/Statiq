using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Statiq.Common
{
    internal class MetadataTypeConverter<T> : MetadataTypeConverter
    {
        public override IEnumerable ToReadOnlyList(IEnumerable enumerable) =>
            ConvertEnumerable(enumerable).ToImmutableArray();

        public override IEnumerable ToList(IEnumerable enumerable) =>
            ConvertEnumerable(enumerable).ToList();

        public override IEnumerable ToArray(IEnumerable enumerable) =>
            ConvertEnumerable(enumerable).ToArray();

        public override IEnumerable ToEnumerable(IEnumerable enumerable) =>
            ConvertEnumerable(enumerable);

        // This is where the magic happens via https://github.com/t-bruning/UniversalTypeConverter
        public static bool TryConvert(object value, out T result) =>
            UniversalTypeConverter.TryConvertTo(value, out result);

        private static IEnumerable<T> ConvertEnumerable(IEnumerable enumerable)
        {
            foreach (object value in enumerable)
            {
                if (TryConvert(value, out T result))
                {
                    yield return result;
                }
            }
        }
    }
}
