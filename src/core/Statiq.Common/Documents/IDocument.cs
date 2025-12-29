using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Contains content and metadata for each item as it propagates through the pipeline.
    /// </summary>
    public interface IDocument : IMetadata, IDisplayable, IContentProviderFactory, ILogger, ICacheCode
    {
        /// <summary>
        /// Provides a stopwatch that can be used by implementations to set the <see cref="Timestamp"/>,
        /// typically by getting <see cref="Stopwatch.ElapsedTicks"/> in the <see cref="Timestamp"/> initializer.
        /// </summary>
        protected static readonly Stopwatch TimestampStopwatch = Stopwatch.StartNew();

        /// <summary>
        /// A timestamp when this document was created that helps ordering documents based on creation.
        /// </summary>
        long Timestamp { get; }

        /// <summary>
        /// An identifier that is generated when the document is created and stays the same after cloning.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// An identifier for the document meant to reflect the source of the data. These should be unique (such as a file name).
        /// This property is always an absolute path. If you want to get a relative path, use <see cref="NormalizedPath.GetRelativeInputPath()"/>.
        /// </summary>
        /// <value>
        /// The source of the document, or <c>null</c> if the document doesn't have a source.
        /// </value>
        NormalizedPath Source { get; }

        /// <summary>
        /// The destination of the document. Can be either relative or absolute.
        /// </summary>
        NormalizedPath Destination { get; }

        /// <summary>
        /// The content provider responsible for creating content streams for the document.
        /// </summary>
        /// <remarks>
        /// This will always return a content provider, even if there is empty or no content.
        /// </remarks>
        IContentProvider ContentProvider { get; }

        /// <summary>
        /// Clones this document.
        /// </summary>
        /// <param name="source">The new source. If this document already contains a source, then it's used and this is ignored.</param>
        /// <param name="destination">The new destination or <c>null</c> to keep the existing destination.</param>
        /// <param name="items">New metadata items or <c>null</c> not to add any new metadata.</param>
        /// <param name="contentProvider">The new content provider or <c>null</c> to keep the existing content provider.</param>
        /// <returns>A new document of the same type as this document.</returns>
        IDocument Clone(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null);

        /// <inheritdoc />
        IContentProvider IContentProviderFactory.GetContentProvider() => ContentProvider;

        /// <inheritdoc />
        IContentProvider IContentProviderFactory.GetContentProvider(string mediaType) => ContentProvider.CloneWithMediaType(mediaType);

        /// <inheritdoc />
        string IDisplayable.ToDisplayString() => Source.IsNull ? "unknown source" : Source.ToDisplayString();

        /// <summary>
        /// A default implementation of <see cref="ICacheCode.GetCacheCodeAsync()"/>.
        /// </summary>
        /// <returns>A hash appropriate for caching.</returns>
        public static async Task<int> GetCacheCodeAsync(IDocument document)
        {
            CacheCode cacheCode = new CacheCode();

            // Add the content hash
            await cacheCode.AddAsync(document.ContentProvider);

            // We exclude ContentProvider from hash as we already added CRC for content above
            // Also exclude settings and IMetadataValue implementations
            // And try to convert to a string and hash that for consistency
            foreach ((string key, object value) in document
                .WithoutSettings()
                .GetRawEnumerable()
                .Where(x => x.Key != nameof(ContentProvider) && !(x.Value is IMetadataValue))
                .Select(x => (x.Key, x.Value is ICacheCode ? x.Value : (TypeHelper.TryConvert(x.Value, out string stringValue) ? stringValue : x.Value)))
                .OrderBy(x => x.Key))
            {
                cacheCode.Add(key);

                // The value is either an ICacheCode, a string, or an object
                switch (value)
                {
                    case ICacheCode cacheCodeValue:
                        await cacheCode.AddAsync(cacheCodeValue);
                        break;
                    case string stringValue:
                        cacheCode.Add(stringValue);
                        break;
                    default:
                        // Take our changes with the normal GetHashCode implementation which may not be deterministic,
                        // but that probably means this thing shouldn't be persisted in a cache anyway
                        cacheCode.Add(value?.GetHashCode() ?? 0);
                        break;
                }
            }
            return cacheCode.ToCacheCode();
        }

        // ILogger default implementation

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            IExecutionContext.Current.Log(logLevel, this, eventId, state, exception, formatter);

        bool ILogger.IsEnabled(LogLevel logLevel) => IExecutionContext.Current.IsEnabled(logLevel);

        IDisposable ILogger.BeginScope<TState>(TState state) => IExecutionContext.Current.BeginScope(state);

        /// <summary>
        /// A hash of the property names in <see cref="IDocument"/> generated using reflection (generally intended for internal use).
        /// </summary>
        public static ImmutableHashSet<string> Properties = ImmutableHashSet.CreateRange(
            StringComparer.OrdinalIgnoreCase,
            typeof(IDocument)
                .GetProperties()
                .Select(x => (x.Name, x.GetGetMethod()))
                .Where(x => x.Item2?.GetParameters().Length == 0)
                .Select(x => x.Name));
    }
}