using System;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    // Implements IMetadata so we can call .GetMetadata() to get nested settings dictionaries
    internal class SettingsConfigurationDictionary : Dictionary<string, object>, IMetadata, ISettingsConfiguration
    {
        public SettingsConfigurationDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public void ResolveScriptMetadataValues(string key, IExecutionState executionState)
        {
            // We need to enumerate a materialized list since we're changing it during enumeration
            foreach (KeyValuePair<string, object> item in this.ToArray())
            {
                if (ScriptMetadataValue.TryGetScriptMetadataValue(key, item.Value, executionState, out ScriptMetadataValue metadataValue))
                {
                    this[item.Key] = metadataValue;
                }
                else if (item.Value is ISettingsConfiguration configurationSettings)
                {
                    configurationSettings.ResolveScriptMetadataValues(key, executionState);
                }
            }
        }

        IEnumerator<KeyValuePair<string, object>> IMetadata.GetRawEnumerator() => GetEnumerator();

        bool IMetadata.TryGetRaw(string key, out object value) => TryGetValue(key, out value);
    }
}
