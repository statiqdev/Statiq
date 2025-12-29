using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Core
{
    public class IfCondition : ModuleList
    {
        public Config<bool> Predicate { get; set; }

        internal IfCondition(Config<bool> predicate, IEnumerable<IModule> modules)
            : base(modules)
        {
            Predicate = predicate;
        }

        internal IfCondition(IEnumerable<IModule> modules)
            : base(modules)
        {
        }
    }
}
