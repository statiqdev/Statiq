using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Statiq.Feeds.Syndication.Rss
{
    /// <summary>
    /// RSS 2.0 Category,
    /// see http://blogs.law.harvard.edu/tech/rss#ltcategorygtSubelementOfLtitemgt
    /// and http://blogs.law.harvard.edu/tech/rss#syndic8.
    /// </summary>
    [Serializable]
    public class RssCategory : RssBase
    {
        private string _domain = null;
        private string _value = null;

        public RssCategory()
        {
        }

        public RssCategory(string value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets and sets a string that identifies a categorization taxonomy (url).
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute("domain")]
        public string Domain
        {
            get { return _domain; }
            set { _domain = string.IsNullOrEmpty(value) ? null : value; }
        }

        /// <summary>
        /// Gets and sets a slash-delimited string which identifies a hierarchic location in the indicated taxonomy.
        /// </summary>
        [DefaultValue(null)]
        [XmlText]
        public string Value
        {
            get { return _value; }
            set { _value = string.IsNullOrEmpty(value) ? null : value; }
        }

        public static implicit operator RssCategory(string value)
        {
            return new RssCategory(value);
        }

        public static explicit operator string(RssCategory value)
        {
            return value.Value;
        }
    }
}