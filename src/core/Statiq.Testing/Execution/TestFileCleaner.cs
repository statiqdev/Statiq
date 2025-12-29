using System;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestFileCleaner : IFileCleaner
    {
        public CleanMode CleanMode => CleanMode.None;

        public Task CleanAfterExecutionAsync() => Task.CompletedTask;

        public Task CleanBeforeExecutionAsync() => Task.CompletedTask;

        public void CleanDirectory(IDirectory directory, string name = null)
        {
        }
    }
}
