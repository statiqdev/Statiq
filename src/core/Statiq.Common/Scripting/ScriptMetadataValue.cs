using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// A lazily evaluated metadata value based on script code.
    /// </summary>
    public sealed class ScriptMetadataValue : IMetadataValue
    {
        private readonly string _key;
        private readonly string _originalPrefix;
        private readonly string _script;
        private readonly ConcurrentCache<(string, IMetadata), object> _cache;
        private readonly IExecutionState _executionState;

        private ScriptMetadataValue(
            string key, string originalPrefix, string script, bool cacheValue, IExecutionState executionState)
        {
            _key = key.ThrowIfNull(nameof(key));
            _originalPrefix = originalPrefix.ThrowIfNull(nameof(originalPrefix));
            _script = script.ThrowIfNull(nameof(script));
            _cache = cacheValue ? new ConcurrentCache<(string, IMetadata), object>(false) : null;
            _executionState = executionState.ThrowIfNull(nameof(executionState));
        }

        public object Get(string key, IMetadata metadata)
        {
            // Check if we're excluded from evaluation
            if (metadata is object
                && metadata.TryGetValue(Keys.ExcludeFromEvaluation, out object excludeObject)
                && ((excludeObject is bool excludeBool && excludeBool)
                    || metadata.GetList<string>(Keys.ExcludeFromEvaluation).Contains(_key, StringComparer.OrdinalIgnoreCase)))
            {
                return _originalPrefix + _script;
            }

#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks. Use await or JoinableTaskFactory.Run instead.

            // Get the cached value if this is a cached script
            if (_cache is object)
            {
                return _cache.GetOrAdd(
                    (key, metadata),
                    (x, self) => self._executionState.ScriptHelper.EvaluateAsync(self._script, x.Item2).GetAwaiter().GetResult(),
                    this);
            }

            // Otherwise, evaluate the script each time
            return _executionState.ScriptHelper.EvaluateAsync(_script, metadata).GetAwaiter().GetResult();

#pragma warning restore VSTHRD002
        }

        public static bool TryGetScriptMetadataValue(
            string key, object value, IExecutionState executionState, out ScriptMetadataValue scriptMetadataValue)
        {
            scriptMetadataValue = default;
            if (value is string stringValue)
            {
                bool? isScriptStringCached = IScriptHelper.TryGetScriptString(stringValue, out string script);
                if (isScriptStringCached.HasValue)
                {
                    scriptMetadataValue = new ScriptMetadataValue(
                        key,
                        stringValue.Substring(0, stringValue.Length - script.Length),
                        script,
                        isScriptStringCached.Value,
                        executionState);
                    return true;
                }
            }
            return false;
        }
    }
}