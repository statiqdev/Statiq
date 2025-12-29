using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Statiq.Common
{
    /// <summary>
    /// A single setting expressed as a configuration section.
    /// </summary>
    internal class SettingsConfigurationSection : IConfigurationSection
    {
        private readonly IConfiguration _configuration;
        private readonly object _value;

        public SettingsConfigurationSection(IConfiguration configuration, string key, string path, object value)
        {
            _configuration = configuration.ThrowIfNull(nameof(configuration));
            Key = key.ThrowIfNull(nameof(key));
            Path = path.ThrowIfNull(nameof(path));
            _value = value;
        }

        public string this[string key]
        {
            get => _configuration[$"{Path}:{key}"];
            set => throw new NotSupportedException();
        }

        public string Key { get; }

        public string Path { get; }

        public string Value
        {
            get
            {
                // Match the logic for GetChildren() and don't return values for items that would return children
                if (!(_value is string)
                    && (_value is null
                    || _value is IDictionary<string, object> _
                    || TypeHelper.TryConvert(_value, out IDictionary<string, object> _)
                    || _value is IList<object> _
                    || (_value is IEnumerable && TypeHelper.TryConvert(_value, out IList<object> _))))
                {
                    return default;
                }

                // Now try a conversion
                if (!TypeHelper.TryExpandAndConvert(Key, _value, null, out string value))
                {
                    return default;
                }
                return !value.IsNullOrEmpty() && value != _value.GetType().ToString() ? value : default;
            }
            set => throw new NotSupportedException();
        }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            if (_value is null || _value is string)
            {
                return Array.Empty<IConfigurationSection>();
            }

            if (_value is IDictionary<string, object> dictionary || TypeHelper.TryConvert(_value, out dictionary))
            {
                return dictionary.Select(x => new SettingsConfigurationSection(_configuration, x.Key, $"{Path}:{x.Key}", x.Value));
            }

            if (_value is IList<object> list || (_value is IEnumerable && TypeHelper.TryConvert(_value, out list)))
            {
                return list.Select((x, i) => new SettingsConfigurationSection(_configuration, i.ToString(), $"{Path}:{i}", x));
            }

            return Array.Empty<IConfigurationSection>();
        }

        public IChangeToken GetReloadToken() => SettingsReloadToken.Instance;

        public IConfigurationSection GetSection(string key) => _configuration.GetSection($"{Path}:{key}");
    }
}
