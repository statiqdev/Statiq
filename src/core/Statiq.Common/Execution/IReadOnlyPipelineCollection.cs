using System.Collections.Generic;

namespace Statiq.Common
{
    public interface IReadOnlyPipelineCollection : IReadOnlyDictionary<string, IReadOnlyPipeline>
    {
    }
}
