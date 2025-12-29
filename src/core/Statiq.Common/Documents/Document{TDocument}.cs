using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A base class for custom document types.
    /// </summary>
    /// <remarks>
    /// Create a derived class to use a custom document type.
    /// </remarks>
    /// <remarks>
    /// Document implementations will consolidate metadata
    /// from explicit metadata, implementation properties,
    /// and finally default metadata (usually the engine
    /// settings).
    /// </remarks>
    /// <typeparam name="TDocument">
    /// The type of this document class (for example, declare
    /// the base class of your custom document type as
    /// <c>public class MyDoc : Document&lt;MyDoc&gt;</c>.
    /// </typeparam>
    public abstract class Document<TDocument> : FactoryDocument, IDocument
        where TDocument : Document<TDocument>, new()
    {
        private bool _initialized;
        private IMetadata _metadata;
        private IReadOnlySettings _settings;
        private IContentProvider _contentProvider;
        private NormalizedPath _destination;
        private NormalizedPath _source;

        /// <summary>
        /// Every derived document type should implement an empty default constructor for the
        /// document factory to use. The empty constructor _should not_ call other constructors
        /// (otherwise the document will be incorrectly marked as fully initialized).
        /// If properties like <see cref="Source"/> or <see cref="Metadata"/> need to be initialized
        /// to default values, protected setters are provided that can be called until the document
        /// has been accessed or fully initialized by the factory (at which point the setters will throw).
        /// </summary>
        protected Document()
        {
            // Don't call the other constructors from the empty constructor because it's a special case
            // If we do, that'll trigger initialization, but the empty constructor is also used
            // by the document factory to instantiate a document _then_ initialize it explicitly
            // and we can't initialize twice or it'll throw
        }

        protected Document(
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(null, null, destination, items, contentProvider)
        {
        }

        protected Document(
            in NormalizedPath source,
            in NormalizedPath destination,
            IContentProvider contentProvider = null)
            : this(null, source, destination, null, contentProvider)
        {
        }

        protected Document(
            in NormalizedPath destination,
            IContentProvider contentProvider = null)
            : this(null, null, destination, null, contentProvider)
        {
        }

        protected Document(
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(null, null, null, items, contentProvider)
        {
        }

        protected Document(IContentProvider contentProvider)
            : this(null, null, null, null, contentProvider)
        {
        }

        protected Document(
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(null, source, destination, items, contentProvider)
        {
        }

        protected Document(
            IReadOnlySettings settings,
            in NormalizedPath source,
            in NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            : this(settings, source, destination, items is null ? null : new Metadata(items), contentProvider)
        {
        }

        protected Document(
            IReadOnlySettings settings,
            in NormalizedPath source,
            in NormalizedPath destination,
            IMetadata metadata,
            IContentProvider contentProvider = null)
        {
            Initialize(settings, source, destination, metadata, contentProvider);
        }

        // Initialization has to be performed separately from construction to maintain the illusion of immutability
        // since clone operations perform a member-wise clone by default (and possibly even not this since it can be
        // overridden) which results in a new document instance before we have the chance to set properties like
        // metadata, source, and destination in the constructor
        internal override IDocument Initialize(
            IReadOnlySettings settings,
            NormalizedPath source,
            NormalizedPath destination,
            IMetadata metadata,
            IContentProvider contentProvider)
        {
            CheckInitialized();
            if (!source.IsNull && !source.IsAbsolute)
            {
                throw new ArgumentException($"Document sources must be absolute ({source})", nameof(source));
            }
            if (!settings.GetBool(Statiq.Common.Keys.IgnoreExternalDestinations) && !destination.IsNull && !destination.IsRelative)
            {
                throw new ArgumentException($"Document destinations must be relative to the output path ({destination})", nameof(destination));
            }

            _settings = settings;
            _metadata = metadata;
            _source = source;
            _destination = destination;
            _contentProvider = contentProvider ?? new NullContent();

            _initialized = true;
            return this;
        }

        /// <inheritdoc />
        public IDocument Clone(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider)
        {
            _initialized = true;
            TDocument document = Clone();
            document._initialized = false;  // The newly cloned document would have copied over our value for initialized
            document.Id = Id;  // Make sure it's got the same ID in case implementation overrode Clone()
            return document.Initialize(
                _settings,
                _source.IsNull ? source : _source,
                destination.IsNull ? _destination : destination,
                items is null ? _metadata : new Metadata(_metadata, items),
                contentProvider ?? _contentProvider);
        }

        protected virtual TDocument Clone() => (TDocument)MemberwiseClone();

        private void CheckInitialized()
        {
            if (_initialized)
            {
                throw new InvalidOperationException($"Document with ID {Id} was previously initialized");
            }
        }

        /// <inheritdoc />
        [PropertyMetadata(null)]
        public long Timestamp { get; } = IDocument.TimestampStopwatch.ElapsedTicks;

        /// <inheritdoc />
        [PropertyMetadata(null)]
        public Guid Id { get; private set; } = Guid.NewGuid();

        protected IReadOnlySettings Settings
        {
            get
            {
                _initialized = true;
                return _settings;
            }
            set
            {
                CheckInitialized();
                _settings = value;
            }
        }

        protected IMetadata Metadata
        {
            get
            {
                _initialized = true;
                return _metadata;
            }
            set
            {
                CheckInitialized();
                _metadata = value;
            }
        }

        /// <inheritdoc />
        public NormalizedPath Source
        {
            get
            {
                _initialized = true;
                return _source;
            }
            protected set
            {
                CheckInitialized();
                _source = value;
            }
        }

        /// <inheritdoc />
        public NormalizedPath Destination
        {
            get
            {
                _initialized = true;
                return _destination;
            }
            protected set
            {
                CheckInitialized();
                _destination = value;
            }
        }

        /// <inheritdoc />
        public IContentProvider ContentProvider
        {
            get
            {
                _initialized = true;
                return _contentProvider;
            }
            protected set
            {
                CheckInitialized();
                _contentProvider = value ?? new NullContent();
            }
        }

        /// <inheritdoc />
        public virtual async Task<int> GetCacheCodeAsync() => await IDocument.GetCacheCodeAsync(this);

        /// <inheritdoc />
        public virtual string ToDisplayString()
        {
            string sourceString = Source.ToDisplayString();
            string destinationString = Destination.ToDisplayString();
            if (!destinationString.IsNullOrEmpty())
            {
                destinationString = "=> " + destinationString;
            }
            if (sourceString.IsNullOrEmpty() && destinationString.IsNullOrEmpty())
            {
                return $"{GetType().Name} ID: {Id}";
            }
            if (!sourceString.IsNullOrEmpty() && !destinationString.IsNullOrEmpty())
            {
                return $"{sourceString} {destinationString}";
            }
            return sourceString.IsNullOrEmpty() ? destinationString : sourceString;
        }

        /// <inheritdoc />
        public override string ToString() => Source.IsNull ? string.Empty : Source.FullPath;

        // IMetadata

        /// <inheritdoc />
        public bool ContainsKey(string key) =>
            (!IDocument.Properties.Contains(key) && Metadata?.ContainsKey(key) == true)
            || PropertyMetadata<TDocument>.For((TDocument)this).ContainsKey(key)
            || (Settings?.ContainsKey(key) ?? false);

        /// <inheritdoc />
        public object this[string key]
        {
            get
            {
                if (!TryGetValue(key.ThrowIfNull(nameof(key)), out object value))
                {
                    throw new KeyNotFoundException("The key " + key + " was not found in the document, use Get() to provide a default value.");
                }
                return value;
            }
        }

        /// <inheritdoc />
        // Enumerate the keys separately so we don't evaluate values
        [PropertyMetadata(null)]
        public IEnumerable<string> Keys
        {
            get
            {
                HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                if (Metadata is object)
                {
                    foreach (string key in Metadata.Keys.Where(x => !IDocument.Properties.Contains(x)))
                    {
                        if (keys.Add(key))
                        {
                            yield return key;
                        }
                    }
                }

                foreach (string key in PropertyMetadata<TDocument>.For((TDocument)this).Keys)
                {
                    if (keys.Add(key))
                    {
                        yield return key;
                    }
                }

                if (Settings is object)
                {
                    foreach (string key in Settings.Keys)
                    {
                        if (keys.Add(key))
                        {
                            yield return key;
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        [PropertyMetadata(null)]
        public IEnumerable<object> Values => this.Select(x => x.Value);

        /// <inheritdoc />
        public bool TryGetRaw(string key, out object value)
        {
            value = default;
            return (!IDocument.Properties.Contains(key) && Metadata?.TryGetRaw(key, out value) == true)
                || PropertyMetadata<TDocument>.For((TDocument)this).TryGetRaw(key, out value)
                || (Settings?.TryGetRaw(key, out value) ?? false);
        }

        /// <inheritdoc />
        public bool TryGetValue(string key, out object value) => this.TryGetValue<object>(key, out value);

        /// <inheritdoc />
        // We have to exclude properties that use the enumerator because they cause infinite recursion
        // The Select ensures LINQ optimizations won't turn this into a recursive call to Count
        [PropertyMetadata(null)]
        public int Count => this.Select(_ => (object)null).Count();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Metadata is object)
            {
                foreach (KeyValuePair<string, object> item in Metadata.Where(x => !IDocument.Properties.Contains(x.Key)))
                {
                    if (keys.Add(item.Key))
                    {
                        yield return item;
                    }
                }
            }

            foreach (KeyValuePair<string, object> item in PropertyMetadata<TDocument>.For((TDocument)this))
            {
                if (keys.Add(item.Key))
                {
                    yield return item;
                }
            }

            if (Settings is object)
            {
                foreach (KeyValuePair<string, object> item in Settings)
                {
                    if (keys.Add(item.Key))
                    {
                        yield return item;
                    }
                }
            }
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, object>> GetRawEnumerator() => GetRawEnumerator(true);

        private IEnumerator<KeyValuePair<string, object>> GetRawEnumerator(bool withSettings)
        {
            HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Metadata is object)
            {
                foreach (KeyValuePair<string, object> item in Metadata.GetRawEnumerable().Where(x => !IDocument.Properties.Contains(x.Key)))
                {
                    if (keys.Add(item.Key))
                    {
                        yield return item;
                    }
                }
            }

            foreach (KeyValuePair<string, object> item in PropertyMetadata<TDocument>.For((TDocument)this).GetRawEnumerable())
            {
                if (keys.Add(item.Key))
                {
                    yield return item;
                }
            }

            if (withSettings && Settings is object)
            {
                foreach (KeyValuePair<string, object> item in Settings.GetRawEnumerable())
                {
                    if (keys.Add(item.Key))
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}