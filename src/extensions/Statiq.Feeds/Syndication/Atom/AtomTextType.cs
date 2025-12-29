using System.Xml.Serialization;

namespace Statiq.Feeds.Syndication.Atom
{
    public enum AtomTextType
    {
        [XmlEnum("text")]
        Text,

        [XmlEnum("html")]
        Html,

        [XmlEnum("xhtml")]
        Xhtml
    }
}