namespace Statiq.Common
{
    /// <summary>
    /// Represents a pipeline with a name.
    /// </summary>
    /// <remarks>
    /// Use this when creating pipeline classes and registering through the DI container
    /// to give the pipeline a name other than it's class name.
    /// </remarks>
    public interface INamedPipeline : IPipeline
    {
        /// <summary>
        /// The name of the pipeline.
        /// </summary>
        string PipelineName { get; }
    }
}