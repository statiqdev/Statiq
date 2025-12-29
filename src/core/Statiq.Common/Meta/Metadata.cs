using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A linked list of metadata items.
    /// </summary>
    public class Metadata : IMetadata
    {
        private readonly IMetadata _previous;

        protected IDictionary<string, object> Dictionary { get; }

        /// <summary>
        /// Creates a new set of metadata.
        /// </summary>
        /// <param name="executionState">The current execution state.</param>
        /// <param name="previous">The previous set of metadata this one should extend.</param>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        public Metadata(IExecutionState executionState, IMetadata previous, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(executionState, items)
        {
            _previous = previous;
        }

        /// <summary>
        /// Creates a new set of metadata.
        /// </summary>
        /// <param name="executionState">The current execution state.</param>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        public Metadata(IExecutionState executionState, IEnumerable<KeyValuePair<string, object>> items = null)
        {
            executionState.ThrowIfNull(nameof(executionState));

            if (items is object)
            {
                Dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

                // If items is an IMetadata, use the raw enumerable so that we don't expand IMetadataValue values
                if (items is IMetadata metadata)
                {
                    items = metadata.GetRawEnumerable();
                }

                // Iterate the items, checking for script values
                foreach (KeyValuePair<string, object> item in items)
                {
                    if (ScriptMetadataValue.TryGetScriptMetadataValue(item.Key, item.Value, executionState, out ScriptMetadataValue metadataValue))
                    {
                        Dictionary[item.Key] = metadataValue;
                    }
                    else
                    {
                        Dictionary[item.Key] = item.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new set of metadata.
        /// </summary>
        /// <remarks>
        /// This is marked as <c>protected internal</c> to discourage public use. When at all possible, metadata
        /// should be created such that it evaluated scripted metadata. If a <see cref="IExecutionState"/> isn't
        /// available for use at the time the metadata is needed, then a derived type should be created for use.
        /// </remarks>
        /// <param name="previous">The previous set of metadata this one should extend.</param>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        public Metadata(IMetadata previous, IEnumerable<KeyValuePair<string, object>> items = null)
            : this(IExecutionContext.Current, previous, items)
        {
        }

        /// <summary>
        /// Creates a new set of metadata.
        /// </summary>
        /// <remarks>
        /// This is marked as <c>protected internal</c> to discourage public use. When at all possible, metadata
        /// should be created such that it evaluated scripted metadata. If a <see cref="IExecutionState"/> isn't
        /// available for use at the time the metadata is needed, then a derived type should be created for use.
        /// </remarks>
        /// <param name="items">The initial set of items. If null, no underlying dictionary will be created.</param>
        public Metadata(IEnumerable<KeyValuePair<string, object>> items = null)
            : this((IExecutionState)IExecutionContext.Current, items)
        {
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            key.ThrowIfNull(nameof(key));
            return (Dictionary?.ContainsKey(key) ?? false) || (_previous?.ContainsKey(key) ?? false);
        }

        /// <inheritdoc/>
        public bool TryGetRaw(string key, out object value)
        {
            key.ThrowIfNull(nameof(key));
            value = default;
            return (Dictionary?.TryGetValue(key, out value) ?? false) || (_previous?.TryGetRaw(key, out value) ?? false);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value) => this.TryGetValue<object>(key, out value);

        /// <inheritdoc/>
        public object this[string key]
        {
            get
            {
                key.ThrowIfNull(nameof(key));
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in metadata, use Get() to provide a default value.");
                }
                return value;
            }
        }

        /// <inheritdoc/>
        // Enumerate the keys separately so we don't evaluate values
        public IEnumerable<string> Keys
        {
            get
            {
                if (Dictionary is object)
                {
                    foreach (string key in Dictionary.Keys)
                    {
                        yield return key;
                    }
                }
                if (_previous is object)
                {
                    foreach (string previousKey in _previous.Keys)
                    {
                        if (Dictionary?.ContainsKey(previousKey) != true)
                        {
                            yield return previousKey;
                        }
                    }
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<object> Values => this.Select(x => x.Value);

        /// <inheritdoc/>
        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        public int Count => this.Select(_ => (object)null).Count();

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            if (Dictionary is object)
            {
                foreach (KeyValuePair<string, object> item in Dictionary)
                {
                    yield return TypeHelper.ExpandKeyValuePair(item, this);
                }
            }
            if (_previous is object)
            {
                foreach (KeyValuePair<string, object> previousItem in _previous)
                {
                    if (Dictionary?.ContainsKey(previousItem.Key) != true)
                    {
                        yield return previousItem;
                    }
                }
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetRawEnumerator()
        {
            if (Dictionary is object)
            {
                foreach (KeyValuePair<string, object> item in Dictionary)
                {
                    yield return item;
                }
            }
            if (_previous is object)
            {
                foreach (KeyValuePair<string, object> previousItem in _previous.GetRawEnumerable())
                {
                    if (Dictionary?.ContainsKey(previousItem.Key) != true)
                    {
                        yield return previousItem;
                    }
                }
            }
        }
    }
}