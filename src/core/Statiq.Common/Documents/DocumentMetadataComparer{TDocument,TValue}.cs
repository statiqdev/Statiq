using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// Performs comparisons between documents using a specified metadata key and value type.
    /// </summary>
    /// <typeparam name="TDocument">The document type.</typeparam>
    /// <typeparam name="TValue">The type metadata value type.</typeparam>
    public class DocumentMetadataComparer<TDocument, TValue> : IComparer<TDocument>
        where TDocument : IDocument
    {
        private readonly IComparer<TValue> _comparer;
        private readonly string _key;
        private readonly TValue _defaultValue;

        public DocumentMetadataComparer(string key)
            : this(key, default, Comparer<TValue>.Default)
        {
        }

        public DocumentMetadataComparer(string key, TValue defaultValue)
            : this(key, defaultValue, Comparer<TValue>.Default)
        {
        }

        public DocumentMetadataComparer(string key, IComparer<TValue> comparer)
            : this(key, default, comparer)
        {
        }

        public DocumentMetadataComparer(string key, TValue defaultValue, IComparer<TValue> comparer)
        {
            _key = key.ThrowIfNull(nameof(key));
            _defaultValue = defaultValue;
            _comparer = comparer.ThrowIfNull(nameof(comparer));
        }

        public int Compare([AllowNull] TDocument x, [AllowNull] TDocument y) =>
            _comparer.Compare(
                x is null ? _defaultValue : x.Get(_key, _defaultValue),
                y is null ? _defaultValue : y.Get(_key, _defaultValue));
    }
}
