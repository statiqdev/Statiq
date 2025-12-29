using System;

namespace Statiq.Common
{
    /// <summary>
    /// Raised before deployment pipelines are run or at the end of execution if there are no deployment pipelines
    /// (the event will always be raised regardless of whether there are deployment pipelines).
    /// </summary>
    public class BeforeDeployment
    {
        internal BeforeDeployment(IEngine engine, Guid executionId)
        {
            Engine = engine;
            ExecutionId = executionId;
        }

        public IEngine Engine { get; }

        public Guid ExecutionId { get; }
    }
}
