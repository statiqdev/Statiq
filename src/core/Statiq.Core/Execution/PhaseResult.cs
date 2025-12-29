using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    internal class PhaseResult
    {
        public PhaseResult(
            string pipelineName,
            Phase phase,
            ImmutableArray<IDocument> outputs,
            in DateTimeOffset startTime,
            long elapsedMilliseconds)
        {
            PipelineName = pipelineName;
            Phase = phase;
            Outputs = outputs;
            StartTime = startTime;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public string PipelineName { get; }

        public Phase Phase { get; }

        public ImmutableArray<IDocument> Outputs { get; }

        public DateTimeOffset StartTime { get; }

        public long ElapsedMilliseconds { get; }
    }
}
