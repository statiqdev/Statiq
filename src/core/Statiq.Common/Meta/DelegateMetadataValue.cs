using System;

namespace Statiq.Common
{
    /// <summary>
    /// This class uses a delegate to get a metadata value.
    /// </summary>
    public class DelegateMetadataValue : IMetadataValue
    {
        private readonly Func<string, IMetadata, object> _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateMetadataValue"/> class.
        /// The specified delegate should be thread-safe.
        /// </summary>
        /// <param name="value">The delegate that returns the metadata value.</param>
        public DelegateMetadataValue(Func<IMetadata, object> value)
            : this((_, metadata) => value(metadata))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateMetadataValue"/> class.
        /// The specified delegate should be thread-safe.
        /// </summary>
        /// <param name="value">The delegate that returns the metadata value.</param>
        public DelegateMetadataValue(Func<string, IMetadata, object> value)
        {
            _value = value;
        }

        /// <summary>
        /// Lazily loads a metadata value. This method will be called
        /// for each request and the return object will
        /// be processed like any other metadata value. The implementation
        /// of this method must be thread-safe.
        /// </summary>
        /// <param name="key">The metadata key being requested.</param>
        /// <param name="metadata">The metadata object requesting the value.</param>
        /// <returns>The object to use as the value.</returns>
        public virtual object Get(string key, IMetadata metadata) => _value(key, metadata);
    }
}
