using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Statiq.Common
{
    public class AfterModuleExecution
    {
        internal AfterModuleExecution(IExecutionContext context, ImmutableArray<IDocument> outputs, long elapsedMilliseconds)
        {
            Context = context;
            Outputs = outputs;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public IExecutionContext Context { get; }

        public ImmutableArray<IDocument> Outputs { get; }

        public long ElapsedMilliseconds { get; }

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
