using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Adds the result document(s) to metadata of the input documents.
    /// </summary>
    /// <remarks>
    /// If more than one result document is produced, it will be added as a
    /// <see cref="IReadOnlyList{IDocument}"/> to the specified metadata key.
    /// </remarks>
    /// <category name="Metadata" />
    public class AddDocumentsToMetadata : SyncChildDocumentsModule
    {
        private readonly string _key;

        public AddDocumentsToMetadata(string key)
            : base(Array.Empty<IModule>())
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        public AddDocumentsToMetadata(string key, params IModule[] modules)
            : base(modules)
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        public AddDocumentsToMetadata(string key, params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Outputs.FromPipelines(pipelines))))
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        public AddDocumentsToMetadata(string key, Config<IEnumerable<IDocument>> documents)
            : base(new ExecuteConfig(documents))
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        protected override IEnumerable<IDocument> ExecuteChildren(
            IExecutionContext context,
            ImmutableArray<IDocument> childOutputs) =>
            childOutputs.Length == 0
                ? context.Inputs
                : context.Inputs.Select(input => input.Clone(new MetadataItems
                {
                    { _key, childOutputs.Length == 1 ? (object)childOutputs[0] : childOutputs }
                }));
    }
}