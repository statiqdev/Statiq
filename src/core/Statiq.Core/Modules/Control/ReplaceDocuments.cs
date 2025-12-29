using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Replaces documents in the current pipeline.
    /// </summary>
    /// <category name="Control" />
    public class ReplaceDocuments : SyncChildDocumentsModule
    {
        public ReplaceDocuments()
            : base(Array.Empty<IModule>())
        {
        }

        public ReplaceDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        public ReplaceDocuments(params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Outputs.FromPipelines(pipelines))))
        {
        }

        public ReplaceDocuments(Config<IEnumerable<IDocument>> documents)
            : base(new ExecuteConfig(documents))
        {
        }

        protected override IEnumerable<IDocument> ExecuteChildren(
            IExecutionContext context,
            ImmutableArray<IDocument> childOutputs) =>
            childOutputs;
    }
}