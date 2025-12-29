using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Clones each input document with the content and metadata from each result document.
    /// </summary>
    /// <remarks>
    /// If more than one result document is produced, it will be merged with every input document and the
    /// total number of output documents will be input * result. If you want to maintain a 1-to-1 relationship
    /// between input documents and child module results, wrap with a <see cref="ForEachDocument"/> module
    /// or use the <see cref="IModuleExtensions.ForEachDocument(IModule)"/> extension.
    /// </remarks>
    /// <category name="Control" />
    public class MergeDocuments : SyncChildDocumentsModule
    {
        private bool _reverse;
        private bool _keepExistingMetadata;

        public MergeDocuments()
            : base(Array.Empty<IModule>())
        {
        }

        public MergeDocuments(params IModule[] modules)
            : base(modules)
        {
        }

        public MergeDocuments(params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Outputs.FromPipelines(pipelines))))
        {
        }

        public MergeDocuments(Config<IEnumerable<IDocument>> documents)
            : base(new ExecuteConfig(documents))
        {
        }

        /// <summary>
        /// The default behavior of this module is to clone each input document with the content and metadata
        /// from each result document. This method reverses that logic by cloning each result document with the
        /// content and metadata from each input document.
        /// </summary>
        /// <param name="reverse"><c>true</c> to reverse the merge direction, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public MergeDocuments Reverse(bool reverse = true)
        {
            _reverse = reverse;
            return this;
        }

        /// <summary>
        /// The default behavior of this module is to replace all existing metadata with metadata from
        /// the merged document. This method ensures that any existing metadata values are kept and only
        /// non-existing values are merged.
        /// </summary>
        /// <param name="keepExistingMetadata"><c>true</c> to keep existing metadata values, <c>false</c> to allow overwriting metadata with values from the merged document.</param>
        /// <returns>The current module instance.</returns>
        public MergeDocuments KeepExistingMetadata(bool keepExistingMetadata = true)
        {
            _keepExistingMetadata = keepExistingMetadata;
            return this;
        }

        protected override IEnumerable<IDocument> ExecuteChildren(
            IExecutionContext context,
            ImmutableArray<IDocument> childOutputs) =>
            _reverse
                ? childOutputs.SelectMany(childOutput =>
                {
                    IMetadata childOutputWithoutSettings = childOutput.WithoutSettings();
                    return context.Inputs.Select(input =>
                        childOutput.Clone(_keepExistingMetadata ? input.WithoutSettings().GetRawEnumerable().Where(x => !childOutputWithoutSettings.ContainsKey(x.Key)) : input, input.ContentProvider));
                })
                : context.Inputs.SelectMany(input =>
                {
                    IMetadata inputWithoutSettings = input.WithoutSettings();
                    return childOutputs.Select(result =>
                        input.Clone(_keepExistingMetadata ? result.WithoutSettings().GetRawEnumerable().Where(x => !inputWithoutSettings.ContainsKey(x.Key)) : result, result.ContentProvider));
                });
    }
}