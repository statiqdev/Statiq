using System.Xml.Serialization;

namespace Statiq.Feeds.Syndication
{
    public interface INamespaceProvider
    {
        /// <summary>
        /// Adds additional namespace URIs for the feed.
        /// </summary>
        void AddNamespaces(XmlSerializerNamespaces namespaces);
    }
}