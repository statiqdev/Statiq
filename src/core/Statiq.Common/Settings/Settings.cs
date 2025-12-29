using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Statiq.Common
{
    public class Settings : ISettings, IConfiguration
    {
        private readonly ConcurrentDictionary<string, object> _settings;

        private readonly IExecutionState _executionState;

        public Settings()
        {
            _settings = new ConcurrentDictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public Settings(IConfigurationRoot configuration)
        {
            // Copy over the configuration, resolving nested sections and arrays
            _settings = new ConcurrentDictionary<string, object>(
                BuildConfigurationObject(configuration, null) as IDictionary<string, object>,
                StringComparer.OrdinalIgnoreCase);
        }

        private Settings(IExecutionState executionState, IEnumerable<KeyValuePair<string, object>> settings)
        {
            _executionState = executionState.ThrowIfNull(nameof(executionState));

            _settings = new ConcurrentDictionary<string, object>(
                settings.Select(setting =>
                {
                    if (ScriptMetadataValue.TryGetScriptMetadataValue(setting.Key, setting.Value, executionState, out ScriptMetadataValue metadataValue))
                    {
                        return new KeyValuePair<string, object>(setting.Key, metadataValue);
                    }

                    if (setting.Value is ISettingsConfiguration configurationSettings)
                    {
                        configurationSettings.ResolveScriptMetadataValues(setting.Key, executionState);
                    }
                    return setting;
                }),
                StringComparer.OrdinalIgnoreCase);
        }

        public Settings WithExecutionState(IExecutionState executionState) => _executionState is object ? this : new Settings(executionState, _settings);

        // Internal for testing, if path is null this is the root dictionary
        internal static object BuildConfigurationObject(IConfiguration configuration, string path)
        {
            // Build up both a dictionary and a list until we know it's not a list
            // If this is the root dictionary (path is null) use a normal dictionary
            IDictionary<string, object> dictionary = new SettingsConfigurationDictionary();
            SettingsConfigurationList list = path is null ? null : new SettingsConfigurationList();
            int index = 0;
            foreach (IConfigurationSection section in configuration.GetChildren())
            {
                // Are we continuing the list?
                if (list != null && (!int.TryParse(section.Key, out int indexKey) || indexKey != index++))
                {
                    list = null;
                }

                if (section.Value is null)
                {
                    object value = BuildConfigurationObject(section, section.Path);
                    list?.Add(value);
                    dictionary[section.Key] = value;
                }
                else
                {
                    list?.Add(section.Value);
                    dictionary.Add(section.Key, section.Value);
                }
            }

            return (object)list ?? dictionary;
        }

        public bool ContainsKey(string key) => _settings.ContainsKey(key);

        public bool TryGetRaw(string key, out object value)
        {
            key.ThrowIfNull(nameof(key));
            if (_settings.TryGetValue(key, out value))
            {
                value = SettingsValue.Get(value);
                return true;
            }
            return false;
        }

        public IEnumerator<KeyValuePair<string, object>> GetRawEnumerator() =>
            _settings.Select(x => SettingsValue.Get(x)).GetEnumerator();

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value) =>
            this.TryGetValue<object>(key, out value);

        public void Add(string key, object value)
        {
            key.ThrowIfNull(nameof(key));

            // Defer to the indexer set method which attempts to resolve scripted values
            if (ContainsKey(key))
            {
                throw new ArgumentException($"The key {key} already exists");
            }
            this[key] = value;
        }

        public int Count => _settings.Count;

        public ICollection<string> Keys => ((IReadOnlyDictionary<string, object>)this).Keys.ToArray();

        public ICollection<object> Values => ((IReadOnlyDictionary<string, object>)this).Values.ToArray();

        IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => _settings.Keys;

        // Enumerate the values so we expand values
        IEnumerable<object> IReadOnlyDictionary<string, object>.Values => this.Select(x => x.Value);

        public bool IsReadOnly => false;

        object IReadOnlyDictionary<string, object>.this[string key] => this[key];

        public object this[string key]
        {
            get
            {
                key.ThrowIfNull(nameof(key));
                if (!TryGetValue(key, out object value))
                {
                    throw new KeyNotFoundException($"The key {key} was not found in metadata, use {nameof(IMetadataGetExtensions.Get)} to provide a default value.");
                }
                return value;
            }

            set
            {
                key.ThrowIfNull(nameof(key));
                _settings[key] =
                    _executionState is object
                        && ScriptMetadataValue.TryGetScriptMetadataValue(key, value, _executionState, out ScriptMetadataValue metadataValue)
                    ? metadataValue
                    : value;
            }
        }

        public bool Remove(string key) => _settings.TryRemove(key, out object _);

        public void Add(KeyValuePair<string, object> item) => Add(item.Key, item.Value);

        public void Clear() => _settings.Clear();

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            int index = 0;
            foreach (KeyValuePair<string, object> item in this)
            {
                array[index++] = item;
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
            _settings.Select(x => TypeHelper.ExpandKeyValuePair(x, this)).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool Remove(KeyValuePair<string, object> item) => throw new NotSupportedException();

        public bool Contains(KeyValuePair<string, object> item) => throw new NotSupportedException();

        // IConfiguration

        string IConfiguration.this[string key]
        {
            get => ((IConfiguration)this).GetSection(key).Value;
            set => throw new NotSupportedException();
        }

        IConfigurationSection IConfiguration.GetSection(string key)
        {
            key.ThrowIfNull(nameof(key));

            string[] keys = key.Split(':');
            object item = this;
            for (int c = 0; c < keys.Length; c++)
            {
                if (item is null || item is string)
                {
                    return new SettingsConfigurationSection(this, keys[^1], key, default);
                }
                else if (item is IDictionary<string, object> dictionary || TypeHelper.TryConvert(item, out dictionary))
                {
                    if (!dictionary.TryGetValue(keys[c], out item))
                    {
                        return new SettingsConfigurationSection(this, keys[^1], key, default);
                    }
                }
                else if (item is IList<object> list || (!(item is string) && item is IEnumerable && TypeHelper.TryConvert(item, out list)))
                {
                    if (!int.TryParse(keys[c], out int index) || index >= list.Count)
                    {
                        return new SettingsConfigurationSection(this, keys[^1], key, default);
                    }
                    item = list[index];
                }
                else
                {
                    return new SettingsConfigurationSection(this, keys[^1], key, default);
                }
            }
            return new SettingsConfigurationSection(this, keys[^1], key, item);
        }

        IEnumerable<IConfigurationSection> IConfiguration.GetChildren() =>
            this.Select(x => new SettingsConfigurationSection(this, x.Key, x.Key, x.Value));

        IChangeToken IConfiguration.GetReloadToken() => SettingsReloadToken.Instance;
    }
}
