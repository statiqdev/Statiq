using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// An analyzer that runs a delegate.
    /// </summary>
    public class DelegateAnalyzer : Analyzer
    {
        private readonly Func<IAnalyzerContext, Task> _analyzeFunc;

        public DelegateAnalyzer(
            LogLevel logLevel,
            IEnumerable<KeyValuePair<string, Phase>> piplinePhases,
            Func<IAnalyzerContext, Task> analyzeFunc)
        {
            LogLevel = logLevel;
            if (piplinePhases is object)
            {
                foreach (KeyValuePair<string, Phase> pipelinePhase in PipelinePhases)
                {
                    PipelinePhases.Add(pipelinePhase.Key, pipelinePhase.Value);
                }
            }
            _analyzeFunc = analyzeFunc.ThrowIfNull(nameof(analyzeFunc));
        }

        public override async Task AnalyzeAsync(IAnalyzerContext context) => await _analyzeFunc(context);
    }
}
