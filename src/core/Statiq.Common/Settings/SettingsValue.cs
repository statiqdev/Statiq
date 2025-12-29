using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A simple wrapper for metadata values from settings so that they can be
    /// identified (for example, to support <see cref="IMetadataGetExtensions.WithoutSettings(IMetadata)"/>.
    /// </summary>
    internal sealed class SettingsValue : IMetadataValue
    {
        private readonly object _value;

        private SettingsValue(object value)
        {
            _value = value;
        }

        public object Get(string key, IMetadata metadata) => _value;

        public static object Get(object value) =>
            value is SettingsValue ? value : new SettingsValue(value);

        public static KeyValuePair<string, object> Get(in KeyValuePair<string, object> item) =>
            new KeyValuePair<string, object>(item.Key, item.Value is SettingsValue ? item.Value : new SettingsValue(item.Value));
    }
}
