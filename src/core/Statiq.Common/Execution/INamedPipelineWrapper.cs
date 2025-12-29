namespace Statiq.Common
{
    public interface INamedPipelineWrapper : INamedPipeline
    {
        IPipeline Pipeline { get; }
    }
}