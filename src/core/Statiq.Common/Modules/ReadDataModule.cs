using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for modules that read documents from data that can be turned into a Dictionary&lt;string,object&gt;.
    /// </summary>
    /// <typeparam name="TModule">
    /// The current module type, allows fluent methods defined in the base class to return the properly typed derived class.
    /// </typeparam>
    /// <typeparam name="TItem">
    /// The type of items this module is designed to process.
    /// </typeparam>
    public abstract class ReadDataModule<TModule, TItem> : SyncModule
        where TModule : ReadDataModule<TModule, TItem>
        where TItem : class
    {
        private readonly Dictionary<string, string> _keyConversions = new Dictionary<string, string>();
        private readonly List<string> _includedKeys = new List<string>();
        private readonly List<string> _excludedKeys = new List<string>();
        private string _contentKey = null;
        private int _limit = int.MaxValue;

        /// <summary>
        /// Specifies which metadata key should be used for the document content.
        /// </summary>
        /// <param name="contentKey">The name of the content property.</param>
        /// <returns>The current module instance.</returns>
        public TModule WithContentKey(string contentKey)
        {
            _contentKey = contentKey;
            return (TModule)this;
        }

        /// <summary>
        /// Allows renaming of keys during document creation: "If you find key X, create it as key Y instead".
        /// </summary>
        /// <param name="originalKeyName">The name of the original key to convert.</param>
        /// <param name="newKeyName">The name you want to convert the key to.</param>
        /// <returns>The current module instance.</returns>
        public TModule AddKeyConversion(string originalKeyName, string newKeyName)
        {
            originalKeyName.ThrowIfNull(nameof(originalKeyName));
            newKeyName.ThrowIfNull(nameof(newKeyName));
            _keyConversions.Add(originalKeyName, newKeyName);
            return (TModule)this;
        }

        /// <summary>
        /// Allows creation of a list of keys to keep. If this list any members, any keys other than those in this list will be discarded.
        /// </summary>
        /// <param name="keys">The keys to keep.</param>
        /// <returns>The current module instance.</returns>
        public TModule IncludeKeys(params string[] keys)
        {
            _includedKeys.AddRange(keys.Where(x => !string.IsNullOrEmpty(x)));
            return (TModule)this;
        }

        /// <summary>
        /// Allows creation of a list of keys to discard.
        /// </summary>
        /// <param name="keys">The keys to discard.</param>
        /// <returns>The current module instance.</returns>
        public TModule ExcludeKeys(params string[] keys)
        {
            _excludedKeys.AddRange(keys.Where(x => !string.IsNullOrEmpty(x)));
            return (TModule)this;
        }

        /// <summary>
        /// Limits the number of created documents.
        /// </summary>
        /// <param name="limit">The number of objects to create documents from.</param>
        /// <returns>The current module instance.</returns>
        public TModule WithLimit(int limit)
        {
            _limit = limit;
            return (TModule)this;
        }

        /// <inheritdoc />
        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context)
        {
            IEnumerable<TItem> items = GetItems(context);
            return items is null
                ? (IEnumerable<IDocument>)Array.Empty<IDocument>()
                : items.Where(x => x is object).Take(_limit).AsParallel().Select(ReadData);

            IDocument ReadData(TItem item)
            {
                string content = string.Empty;
                List<KeyValuePair<string, object>> meta = new List<KeyValuePair<string, object>>();

                // Convert whatever we have into a dictionary
                IDictionary<string, object> dict = GetDictionary(item);

                // If we have an allow list, remove anything not listed
                if (_includedKeys.Count > 0)
                {
                    dict = dict.Where(kvp => _includedKeys.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                // Remove anything in the blacklist
                dict = dict.Where(kvp => !_excludedKeys.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                // Loop all the keys in the dictionary
                foreach (KeyValuePair<string, object> kvp in dict)
                {
                    if (kvp.Key == _contentKey)
                    {
                        // This is the contentPropertyName, so this will be the content of the document
                        content = kvp.Value.ToString();
                    }
                    else
                    {
                        // This is NOT the contentPropertyName, so this will be a simple meta value
                        meta.Add(new KeyValuePair<string, object>(_keyConversions.ContainsKey(kvp.Key) ? _keyConversions[kvp.Key] : kvp.Key, kvp.Value));
                    }
                }

                return context.CreateDocument(meta, context.GetContentProvider(content));
            }
        }

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context) =>
            throw new NotSupportedException();

        /// <summary>
        /// Gets the items to convert to documents. The <see cref="GetDictionary(TItem)"/> method
        /// is used to convert each item into a series of key-value pairs that is then used for
        /// document creation.
        /// </summary>
        /// <param name="context">The current execution context.</param>
        /// <returns>The objects to create documents from.</returns>
        protected abstract IEnumerable<TItem> GetItems(IExecutionContext context);

        /// <summary>
        /// Used to convert each object from <see cref="GetItems(IExecutionContext)"/> into a IDictionary&lt;string, object&gt;.
        /// The base implementation checks if the object implements IDictionary&lt;string, object&gt; and just
        /// performs a cast is if it does. If not, reflection is used to construct a IDictionary&lt;string, object&gt;
        /// from all of the object's properties. Override this method to provide an alternate way of getting
        /// key-value pairs for each object.
        /// </summary>
        /// <param name="item">The object to convert to a IDictionary&lt;string, object&gt;.</param>
        /// <returns>A IDictionary&lt;string, object&gt; containing the data used for document creation.</returns>
        protected virtual IDictionary<string, object> GetDictionary(TItem item)
        {
            // If it's already what we want, then just return it
            IDictionary<string, object> dictionary = item as IDictionary<string, object>;
            return dictionary ?? item.GetType().GetProperties().ToDictionary(prop => prop.Name, prop => prop.GetValue(item));
        }
    }
}