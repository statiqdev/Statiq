using Statiq.Common;

namespace Statiq.Core
{
    public abstract class ScriptFactoryBase
    {
        public abstract ScriptBase GetScript(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext);
    }
}
