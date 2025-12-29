using System.Collections;

namespace Statiq.Common
{
    /// <summary>
    /// These are used by <see cref="TypeHelper"/> for enumerable conversions,
    /// but must be declared outside for easier reflection instantiation.
    /// </summary>
    internal abstract class MetadataTypeConverter
    {
        public abstract IEnumerable ToReadOnlyList(IEnumerable enumerable);
        public abstract IEnumerable ToList(IEnumerable enumerable);
        public abstract IEnumerable ToArray(IEnumerable enumerable);
        public abstract IEnumerable ToEnumerable(IEnumerable enumerable);
    }
}
