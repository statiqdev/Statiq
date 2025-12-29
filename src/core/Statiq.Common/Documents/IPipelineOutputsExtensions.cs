using System;
using System.Linq;

namespace Statiq.Common
{
    public static class IPipelineOutputsExtensions
    {
        /// <summary>
        /// Gets and concatenates all documents from multiple pipelines.
        /// Note that if a document exists in more than one pipeline it
        /// may appear multiple times in the result.
        /// </summary>
        /// <param name="pipelineOutputs">The pipeline outputs.</param>
        /// <param name="pipelines">The pipeline(s) to get documents from.</param>
        /// <returns>All documents from all specified pipeline(s).</returns>
        public static DocumentList<IDocument> FromPipelines(
            this IPipelineOutputs pipelineOutputs,
            params string[] pipelines)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            return (pipelines ?? Array.Empty<string>())
                .SelectMany(pipelineOutputs.FromPipeline)
                .ToDocumentList();
        }

        /// <summary>
        /// Gets and concatenates all documents from multiple pipelines.
        /// Note that if a document exists in more than one pipeline it
        /// may appear multiple times in the result.
        /// </summary>
        /// <param name="pipelineOutputs">The pipeline outputs.</param>
        /// <param name="pipelines">The pipeline(s) to exclude.</param>
        /// <returns>All documents from all pipeline(s) except those specified.</returns>
        public static DocumentList<IDocument> ExceptPipelines(
            this IPipelineOutputs pipelineOutputs,
            params string[] pipelines)
        {
            pipelineOutputs.ThrowIfNull(nameof(pipelineOutputs));
            if (pipelines is object && pipelines.Length > 0)
            {
                return pipelineOutputs
                    .ByPipeline()
                    .Where(x => pipelines.Contains(x.Key))
                    .SelectMany(x => x.Value)
                    .ToDocumentList();
            }
            return pipelineOutputs.ToDocumentList();
        }
    }
}