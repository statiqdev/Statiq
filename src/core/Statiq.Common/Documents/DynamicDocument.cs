using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    internal class DynamicDocument : DynamicObject, IDocument
    {
        private readonly IDocument _document;

        public DynamicDocument(IDocument document)
        {
            _document = document;
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object result) =>
            _document.TryGetValue(binder.Name, out result);

        // IDocument

        /// <inheritdoc />
        public object this[string key] => _document[key];

        /// <inheritdoc />
        public long Timestamp => _document.Timestamp;

        /// <inheritdoc />
        public Guid Id => _document.Id;

        /// <inheritdoc />
        public NormalizedPath Source => _document.Source;

        /// <inheritdoc />
        public NormalizedPath Destination => _document.Destination;

        /// <inheritdoc />
        public IContentProvider ContentProvider => _document.ContentProvider;

        /// <inheritdoc />
        public IEnumerable<string> Keys => _document.Keys;

        /// <inheritdoc />
        public IEnumerable<object> Values => _document.Values;

        /// <inheritdoc />
        public int Count => _document.Count;

        /// <inheritdoc />
        public IDocument Clone(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null) =>
            _document.Clone(source, destination, items, contentProvider);

        /// <inheritdoc />
        public bool ContainsKey(string key) => _document.ContainsKey(key);

        /// <inheritdoc />
        public Task<int> GetCacheCodeAsync() => _document.GetCacheCodeAsync();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _document.GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetRawEnumerator() => _document.GetRawEnumerator();

        /// <inheritdoc />
        public bool TryGetRaw(string key, out object value) => _document.TryGetRaw(key, out value);

        /// <inheritdoc />
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) => _document.TryGetValue(key, out value);

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => _document.GetEnumerator();
    }
}