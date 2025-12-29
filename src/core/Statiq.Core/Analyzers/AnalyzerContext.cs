using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    internal class AnalyzerContext : ExecutionContext, IAnalyzerContext
    {
        // Cache analyzer overrides per document
        private static readonly ConcurrentCache<IDocument, IReadOnlyDictionary<string, LogLevel>> _overrideCache =
            new ConcurrentCache<IDocument, IReadOnlyDictionary<string, LogLevel>>(true);

        private readonly string _analyzerName;
        private readonly LogLevel _logLevel;
        private readonly ConcurrentBag<AnalyzerResult> _results;

        internal AnalyzerContext(ExecutionContextData contextData, ImmutableArray<IDocument> inputs, in KeyValuePair<string, IAnalyzer> analyzerItem, ConcurrentBag<AnalyzerResult> results)
            : base(contextData, null, null, inputs)
        {
            _analyzerName = analyzerItem.Key;
            _logLevel = analyzerItem.Value.LogLevel;
            _results = results;
        }

        /// <inheritdoc/>
        public void AddAnalyzerResult(IDocument document, string message)
        {
            LogLevel logLevel = GetLogLevel(document);
            if (logLevel != LogLevel.None)
            {
                _results.Add(new AnalyzerResult(_analyzerName, logLevel, document, message));
            }
        }

        /// <inheritdoc/>
        public LogLevel GetLogLevel(IDocument document)
        {
            LogLevel logLevel = _logLevel;
            if (document is object && document.ContainsKey(Keys.Analyzers))
            {
                IReadOnlyDictionary<string, LogLevel> overrides = _overrideCache.GetOrAdd(
                    document,
                    doc => SettingsParser.Parse(doc.GetList(Keys.Analyzers, Array.Empty<string>())).ToDictionary(
                        x => x.Key,
                        x =>
                        {
                            if (x.Value.Equals("true", StringComparison.OrdinalIgnoreCase))
                            {
                                throw new Exception($"Analyzer override log level must be provided as \"[analyzer]=[log level]\" for {_analyzerName} in document {document.ToDisplayString()}");
                            }
                            if (!Enum.TryParse(x.Value, out LogLevel documentLevel))
                            {
                                throw new Exception($"Analyzer override log level {x.Value} for {_analyzerName} in document {document.ToDisplayString()} is invalid");
                            }
                            return documentLevel;
                        },
                        StringComparer.OrdinalIgnoreCase));
                if (overrides.TryGetValue(_analyzerName, out LogLevel overrideLevel) || overrides.TryGetValue("All", out overrideLevel))
                {
                    logLevel = overrideLevel;
                }
            }
            return logLevel;
        }

        public override IModule Module => throw new NotSupportedException("Not supported in an analyzer execution context");
    }
}