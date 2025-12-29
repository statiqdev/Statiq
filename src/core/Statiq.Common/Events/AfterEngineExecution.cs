using System;

namespace Statiq.Common
{
    public class AfterEngineExecution
    {
        internal AfterEngineExecution(IEngine engine, Guid executionId, IPipelineOutputs outputs, long elapsedMilliseconds)
        {
            Engine = engine;
            ExecutionId = executionId;
            Outputs = outputs;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public IEngine Engine { get; }

        public Guid ExecutionId { get; }

        public IPipelineOutputs Outputs { get; }

        public long ElapsedMilliseconds { get; }
    }
}
