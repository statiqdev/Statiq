using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Testing
{
    public class GatherDocuments : SyncModule
    {
        public ConcurrentDictionary<string, Dictionary<Phase, ImmutableArray<IDocument>>> Documents { get; } =
            new ConcurrentDictionary<string, Dictionary<Phase, ImmutableArray<IDocument>>>();

        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context)
        {
            Documents.AddOrUpdate(
                context.PipelineName,
                _ => new Dictionary<Phase, ImmutableArray<IDocument>>
                {
                    { context.Phase, context.Inputs }
                },
                (_, outputs) =>
                {
                    outputs.Add(context.Phase, context.Inputs);
                    return outputs;
                });
            return context.Inputs;
        }
    }
}
