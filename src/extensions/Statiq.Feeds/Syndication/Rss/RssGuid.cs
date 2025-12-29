using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Statiq.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Guid - http://blogs.law.harvard.edu/tech/rss#ltguidgtSubelementOfLtitemgt.
    /// The value can be any string, not just a URI.
    /// </summary>
    [Serializable]
    public class RssGuid : RssBase, IUriProvider
    {
        private bool _isPermaLink = true;

        public RssGuid()
        {
        }

        public RssGuid(string value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets and sets if the identifier is a permanent URL.
        /// </summary>
        [DefaultValue(true)]
        [XmlAttribute("isPermaLink")]
        public bool IsPermaLink
        {
            get => _isPermaLink && (Value?.StartsWith(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) == true);
            set => _isPermaLink = value;
        }

        /// <summary>
        /// Gets and sets the globally unique identifier, may be a URL or other unique string.
        /// If the value is a URL, this will also set <see cref="IsPermaLink"/> to <c>true</c>.
        /// </summary>
        [DefaultValue(null)]
        [XmlText]
        public string Value { get; set; }

        [XmlIgnore]
        public bool HasValue => !string.IsNullOrEmpty(Value);

        Uri IUriProvider.Uri => ConvertToUri(Value);
    }
}