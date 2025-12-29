using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Statiq.Common
{
    internal interface ISettingsConfiguration
    {
        public abstract void ResolveScriptMetadataValues(string key, IExecutionState executionState);
    }
}
