using System.Collections.Generic;

namespace Statiq.Common
{
    internal class SettingsConfigurationList : List<object>, ISettingsConfiguration
    {
        public void ResolveScriptMetadataValues(string key, IExecutionState executionState)
        {
            for (int c = 0; c < Count; c++)
            {
                if (ScriptMetadataValue.TryGetScriptMetadataValue(key, this[c], executionState, out ScriptMetadataValue metadataValue))
                {
                    this[c] = metadataValue;
                }
                else if (this[c] is ISettingsConfiguration configurationSettings)
                {
                    configurationSettings.ResolveScriptMetadataValues(key, executionState);
                }
            }
        }
    }
}
