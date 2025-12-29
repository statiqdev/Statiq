using System;
using System.Xml.Linq;

namespace Statiq.Common
{
    public static class IMetadataXmlExtensions
    {
        /// <summary>
        /// Gets an XML attribute for the given metadata key.
        /// The name of the attribute will be the lower-case key name.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="key">The key containing the attribute value.</param>
        /// <returns>The attribute if the key was found, <c>null</c> if not.</returns>
        public static XAttribute XAttribute(this IMetadata metadata, string key) => metadata.XAttribute(key, x => x);

        /// <summary>
        /// Gets an XML attribute for the given metadata key.
        /// </summary>
        /// <param name="metadata">The metadata instance.</param>
        /// <param name="name">The name of the XML attribute.</param>
        /// <param name="key">The key containing the attribute value.</param>
        /// <returns>The attribute if the key was found, <c>null</c> if not.</returns>
        public static XAttribute XAttribute(this IMetadata metadata, string name, string key) => metadata.XAttribute(name, key, x => x);

        public static XAttribute XAttribute(this IMetadata metadata, string key, Func<string, string> valueFunc) =>
            metadata.XAttribute(key.ToLower(), key, valueFunc);

        public static XAttribute XAttribute(this IMetadata metadata, string name, string key, Func<string, string> valueFunc) =>
            metadata.XAttribute(key, x => new XAttribute(name, valueFunc(x)));

        public static XAttribute XAttribute(this IMetadata metadata, string key, Func<string, XAttribute> attributeFunc) =>
            metadata.TryGetValue(key, out string value) ? attributeFunc(value) : null;

        public static XElement XElement(this IMetadata metadata, string key, Func<string, object[]> contentFunc) =>
            metadata.XElement(key.ToLower(), key, contentFunc);

        public static XElement XElement(this IMetadata metadata, string name, string key, Func<string, object[]> contentFunc) =>
            metadata.XElement(key, x => new XElement(name, contentFunc(x)));

        public static XElement XElement(this IMetadata metadata, string key, Func<string, XElement> elementFunc) =>
            metadata.TryGetValue(key, out string value) ? elementFunc(value) : null;
    }
}
