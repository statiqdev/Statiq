using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Statiq.Feeds.Syndication.Atom
{
    /// <summary>
    /// See http://tools.ietf.org/html/rfc4287#section-4.2.11.
    /// </summary>
    public class AtomSource : AtomBase
    {
        private Uri _icon = null;
        private Uri _logo = null;

        [DefaultValue(null)]
        [XmlElement("generator")]
        public AtomGenerator Generator { get; set; } = null;

        [DefaultValue(null)]
        [XmlElement("icon")]
        public string Icon
        {
            get { return ConvertToString(_icon); }
            set { _icon = ConvertToUri(value); }
        }

        [XmlIgnore]
        protected Uri IconUri => _icon;

        [DefaultValue(null)]
        [XmlElement("logo")]
        public string Logo
        {
            get { return ConvertToString(_logo); }
            set { _logo = ConvertToUri(value); }
        }

        [XmlIgnore]
        protected Uri LogoUri => _logo;

        [DefaultValue(null)]
        [XmlElement("subtitle")]
        public AtomText SubTitle { get; set; } = null;

        [XmlIgnore]
        public virtual bool SubTitleSpecified
        {
            get { return true; }
            set { }
        }
    }
}