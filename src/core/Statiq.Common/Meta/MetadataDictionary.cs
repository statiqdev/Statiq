using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public class MetadataDictionary : Metadata, IMetadataDictionary
    {
        // Ensure items is not null when calling the base ctor so the dictionary gets instantiated
        public MetadataDictionary(IEnumerable<KeyValuePair<string, object>> items = null)
            : base(Array.Empty<KeyValuePair<string, object>>())
        {
            // Add the items directly to the dictionary instead of through the constructor so raw values won't get interpreted
            if (items is object)
            {
                Dictionary.AddRange(items);
            }
        }

        public void Add(KeyValuePair<string, object> item) => Dictionary.Add(item);

        public void Clear() => Dictionary.Clear();

        public bool IsReadOnly { get; } = false;

        public void Add(string key, object value) => Dictionary.Add(key, value);

        public bool Remove(string key) => Dictionary.Remove(key);

        object IDictionary<string, object>.this[string key]
        {
            get { return Dictionary[key]; }
            set { Dictionary[key] = value; }
        }

        public new object this[string key]
        {
            get { return ((IDictionary<string, object>)this)[key]; }
            set { ((IDictionary<string, object>)this)[key] = value; }
        }

        ICollection<string> IDictionary<string, object>.Keys => Dictionary.Keys;

        ICollection<object> IDictionary<string, object>.Values => Dictionary.Values;

        ICollection<string> IMetadataDictionary.Keys => ((IDictionary<string, object>)this).Keys;

        ICollection<object> IMetadataDictionary.Values => ((IDictionary<string, object>)this).Values;

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public bool Contains(KeyValuePair<string, object> item) => throw new NotSupportedException();

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex) => throw new NotSupportedException();

        // Not supported because the value of the item is raw vs. the expanded values presented by the dictionary
        public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();
    }
}