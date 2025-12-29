using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Clones each input document with the content from each result document.
    /// </summary>
    /// <remarks>
    /// If more than one result document is produced, it will be merged with every input document and the
    /// total number of output documents will be input * result. If you want to maintain a 1-to-1 relationship
    /// between input documents and child module results, wrap with a <see cref="ForEachDocument"/> module
    /// or use the <see cref="IModuleExtensions.ForEachDocument(IModule)"/> extension.
    /// </remarks>
    /// <category name="Control" />
    public class MergeContent : SyncChildDocumentsModule
    {
        private bool _reverse;

        public MergeContent()
            : base(Array.Empty<IModule>())
        {
        }

        public MergeContent(params IModule[] modules)
            : base(modules)
        {
        }

        public MergeContent(params string[] pipelines)
            : base(new ExecuteConfig(Config.FromContext(ctx => ctx.Outputs.FromPipelines(pipelines))))
        {
        }

        public MergeContent(Config<IEnumerable<IDocument>> documents)
            : base(new ExecuteConfig(documents))
        {
        }

        /// <summary>
        /// The default behavior of this module is to clone each input document with the content
        /// from each result document. This method reverses that logic by cloning each result document with the
        /// content and metadata from each input document.
        /// </summary>
        /// <param name="reverse"><c>true</c> to reverse the merge direction, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public MergeContent Reverse(bool reverse = true)
        {
            _reverse = reverse;
            return this;
        }

        protected override IEnumerable<IDocument> ExecuteChildren(
            IExecutionContext context,
            ImmutableArray<IDocument> childOutputs) =>
            _reverse
                ? childOutputs.SelectMany(childOutput => context.Inputs.Select(input => childOutput.Clone(input.ContentProvider)))
                : context.Inputs.SelectMany(input => childOutputs.Select(result => input.Clone(result.ContentProvider)));
    }
}