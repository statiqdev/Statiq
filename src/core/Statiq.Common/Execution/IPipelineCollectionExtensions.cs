using System;

namespace Statiq.Common
{
    /// <summary>
    /// Extensions for working with pipeline collections.
    /// </summary>
    public static class IPipelineCollectionExtensions
    {
        public static void Add(this IPipelineCollection pipelineCollection, IPipeline pipeline) =>
            pipelineCollection.Add(
                (pipeline as INamedPipeline)?.PipelineName ?? pipeline?.GetType().Name,
                (pipeline as INamedPipelineWrapper)?.Pipeline ?? pipeline);

        public static void AddIfNonExisting(this IPipelineCollection pipelineCollection, IPipeline pipeline)
        {
            string name = (pipeline as INamedPipeline)?.PipelineName ?? pipeline?.GetType().Name;
            if (!pipelineCollection.ContainsKey(name))
            {
                pipelineCollection.Add(
                    name,
                    (pipeline as INamedPipelineWrapper)?.Pipeline ?? pipeline);
            }
        }
    }
}