using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public class BeforeModuleExecution
    {
        internal BeforeModuleExecution(IExecutionContext context)
        {
            Context = context;
        }

        public IExecutionContext Context { get; }

        internal IEnumerable<IDocument> OverriddenOutputs { get; private set; }

        public void OverrideOutputs(IEnumerable<IDocument> outputs)
        {
            outputs.ThrowIfNull(nameof(outputs));
            if (OverriddenOutputs is object)
            {
                throw new InvalidOperationException("Only one event may override module results.");
            }
            OverriddenOutputs = outputs;
        }
    }
}
