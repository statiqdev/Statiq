using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions to make it easier to get typed information from metadata.
    /// </summary>
    public static class IMetadataConversionExtensions
    {
        /// <summary>
        /// Gets the value for the specified key converted to a string. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a string.</param>
        /// <returns>The value for the specified key converted to a string or the specified default value.</returns>
        public static string GetString(this IMetadata metadata, string key, string defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Formats a string value if it exists in the metadata, otherwise returns a default value.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="formatFunc">A formatting function to apply to the string value of the specified key.</param>
        /// <param name="defaultValue">The default value to use if the key is not found.</param>
        /// <returns>The formatted value of the specified key if it exists or the specified default value.</returns>
        public static string GetString(this IMetadata metadata, string key, Func<string, string> formatFunc, string defaultValue = null) =>
            metadata.ContainsKey(key) ? formatFunc(metadata.GetString(key)) : defaultValue;

        /// <summary>
        /// Gets the value for the specified key converted to a bool. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a bool.</param>
        /// <returns>The value for the specified key converted to a bool or the specified default value.</returns>
        public static bool GetBool(this IMetadata metadata, string key, bool defaultValue = false) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to an int. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to an int.</param>
        /// <returns>The value for the specified key converted to an int or the specified default value.</returns>
        public static int GetInt(this IMetadata metadata, string key, int defaultValue = 0) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="DateTime"/>. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="DateTime"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="DateTime"/> or the specified default value.</returns>
        public static DateTime GetDateTime(this IMetadata metadata, string key, in DateTime defaultValue = default) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="DateTimeOffset"/>. This method never throws an exception. It will return the specified
        /// default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a <see cref="DateTimeOffset"/>.</param>
        /// <returns>The value for the specified key converted to a <see cref="DateTimeOffset"/> or the specified default value.</returns>
        public static DateTimeOffset GetDateTimeOffset(this IMetadata metadata, string key, in DateTimeOffset defaultValue = default) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="NormalizedPath"/>. This method never throws an exception.
        /// It will return the specified default value if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found.</param>
        /// <returns>The value for the specified key converted to a <see cref="NormalizedPath"/> or the specified default value.</returns>
        public static NormalizedPath GetPath(this IMetadata metadata, string key, in NormalizedPath defaultValue = default) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IReadOnlyList{T}"/>. This method never throws an exception. It will return the specified
        /// default value if the key is not found. Note that if the value is atomic, the conversion operation will succeed and return a list with one item.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a list.</param>
        /// <returns>The value for the specified key converted to a list or the specified default value.</returns>
        public static IReadOnlyList<T> GetList<T>(this IMetadata metadata, string key, IReadOnlyList<T> defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a nested <see cref="IMetadata"/>. This method never throws an exception.
        /// It will return null if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the nested metadata to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to nested metadata.</param>
        /// <returns>The value for the specified key converted to a nested metadata instance or null.</returns>
        public static IMetadata GetMetadata(this IMetadata metadata, string key, IMetadata defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="IDocument"/>. This method never throws an exception.
        /// It will return null if the key is not found.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the document to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a document.</param>
        /// <returns>The value for the specified key converted to a document or null.</returns>
        public static IDocument GetDocument(this IMetadata metadata, string key, IDocument defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <c>IReadOnlyList&lt;IDocument&gt;</c>. This method never throws an exception.
        /// It will return null if the key is not found and an empty list if the key is found but contains no items that can be converted to <see cref="IDocument"/>.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the documents to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a document list.</param>
        /// <returns>The value for the specified key converted to a list or null.</returns>
        public static IEnumerable<IDocument> GetDocuments(this IMetadata metadata, string key, IReadOnlyList<IDocument> defaultValue = null) => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <c>IReadOnlyList&lt;TDocument&gt;</c>. This method never throws an exception.
        /// It will return null if the key is not found and an empty list if the key is found but contains no items that can be converted to the specified document type.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the documents to get.</param>
        /// <param name="defaultValue">The default value to use if the key is not found or cannot be converted to a document list.</param>
        /// <returns>The value for the specified key converted to a list or null.</returns>
        public static IEnumerable<TDocument> GetDocuments<TDocument>(this IMetadata metadata, string key, IReadOnlyList<TDocument> defaultValue = null)
            where TDocument : IDocument
            => metadata.Get(key, defaultValue);

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="DocumentList{IDocument}"/>. This method never throws an exception.
        /// It will return an empty list if the key is not found or if the key is found but contains no items that can be converted to <see cref="IDocument"/>.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the documents to get.</param>
        /// <returns>The value for the specified key converted to a list or null.</returns>
        public static DocumentList<IDocument> GetDocumentList(this IMetadata metadata, string key) => metadata.GetDocuments(key).ToDocumentList();

        /// <summary>
        /// Gets the value for the specified key converted to a <see cref="DocumentList{TDocument}"/>. This method never throws an exception.
        /// It will return an empty list if the key is not found or if the key is found but contains no items that can be converted to the specified document type.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the documents to get.</param>
        /// <returns>The value for the specified key converted to a list or null.</returns>
        public static DocumentList<TDocument> GetDocumentList<TDocument>(this IMetadata metadata, string key)
            where TDocument : IDocument =>
            metadata.GetDocuments<TDocument>(key).ToDocumentList();

        /// <summary>
        /// Gets the value associated with the specified key as a dynamic object. This is equivalent
        /// to calling <c>as dynamic</c> to cast the value.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="defaultValue">The default value to use if either the key is not found or the
        /// underlying value is null (since the dynamic runtime binder can't bind null values).</param>
        /// <returns>A dynamic value for the specific key or default value.</returns>
        public static dynamic GetDynamic(this IMetadata metadata, string key, object defaultValue = null) => metadata.Get(key, defaultValue) ?? defaultValue;
    }
}
