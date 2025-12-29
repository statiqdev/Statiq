using System;

namespace Statiq.Common
{
    public class BeforeEngineExecution
    {
        internal BeforeEngineExecution(IEngine engine, Guid executionId)
        {
            Engine = engine;
            ExecutionId = executionId;
        }

        public IEngine Engine { get; }

        public Guid ExecutionId { get; }
    }
}
