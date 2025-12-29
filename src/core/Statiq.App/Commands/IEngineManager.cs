using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    public interface IEngineManager : IConfigurable
    {
        Engine Engine { get; }

        string[] Pipelines { get; set; }

        bool NormalPipelines { get; set; }

        Task<ExitCode> ExecuteAsync(CancellationTokenSource cancellationTokenSource);
    }
}