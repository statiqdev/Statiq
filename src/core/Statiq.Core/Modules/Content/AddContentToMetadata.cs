using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Adds the result document(s) content to metadata of the input documents.
    /// </summary>
    /// <remarks>
    /// The content of each child result document will be converted to a string.
    /// If more than one result document is produced, the content of each will be added as an array
    /// to the specified metadata key.
    /// </remarks>
    /// <category name="Content" />
    public class AddContentToMetadata : ChildDocumentsModule
    {
        private readonly string _key;

        public AddContentToMetadata(string key)
            : base(Array.Empty<IModule>())
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        public AddContentToMetadata(string key, params IModule[] modules)
            : base(modules)
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        public AddContentToMetadata(string key, params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Outputs.FromPipelines(pipelines))))
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        public AddContentToMetadata(string key, Config<IEnumerable<IDocument>> documents)
            : base(new ExecuteConfig(documents))
        {
            _key = key.ThrowIfNull(nameof(key));
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteChildrenAsync(
            IExecutionContext context,
            ImmutableArray<IDocument> childOutputs)
        {
            if (childOutputs.Length == 0)
            {
                return context.Inputs;
            }

            object content = childOutputs.Length == 1
                ? (object)await childOutputs[0].GetContentStringAsync()
                : await childOutputs.ToAsyncEnumerable().Select(async (IDocument x, CancellationToken _) => await x.GetContentStringAsync()).ToArrayAsync();

            return context.Inputs.Select(input => input.Clone(new MetadataItems { { _key, content } }));
        }
    }
}