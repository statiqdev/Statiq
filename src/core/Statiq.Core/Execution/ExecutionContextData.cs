using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ExecutionContextData
    {
        public ExecutionContextData(
            PipelinePhase pipelinePhase,
            Engine engine,
            IReadOnlyDictionary<string, PhaseResult[]> phaseResults,
            IServiceProvider services)
        {
            Engine = engine.ThrowIfNull(nameof(engine));
            PipelinePhase = pipelinePhase.ThrowIfNull(nameof(pipelinePhase));
            Services = services.ThrowIfNull(nameof(services));
            Outputs = new PhaseOutputs(phaseResults, pipelinePhase, engine.Pipelines);
        }

        public PipelinePhase PipelinePhase { get; }
        public Engine Engine { get; }
        public IServiceProvider Services { get; }
        public IPipelineOutputs Outputs { get; }
    }
}
