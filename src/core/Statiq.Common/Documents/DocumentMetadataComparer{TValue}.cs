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
    /// <typeparam name="TValue">The type metadata value type.</typeparam>
    public class DocumentMetadataComparer<TValue> : DocumentMetadataComparer<IDocument, TValue>
    {
        public DocumentMetadataComparer(string key)
            : base(key)
        {
        }

        public DocumentMetadataComparer(string key, TValue defaultValue)
            : base(key, defaultValue)
        {
        }

        public DocumentMetadataComparer(string key, IComparer<TValue> comparer)
            : base(key, comparer)
        {
        }

        public DocumentMetadataComparer(string key, TValue defaultValue, IComparer<TValue> comparer)
            : base(key, defaultValue, comparer)
        {
        }
    }
}
