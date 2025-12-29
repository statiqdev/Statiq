using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public static class ParentModuleExtensions
    {
        public static TModule WithChildren<TModule>(this TModule module, params IModule[] modules)
            where TModule : ParentModule =>
            WithChildren(module, (IEnumerable<IModule>)modules);

        public static TModule WithChildren<TModule>(this TModule module, IEnumerable<IModule> modules)
            where TModule : ParentModule
        {
            module.ThrowIfNull(nameof(module));
            module.Children.AddRange(modules);
            return module;
        }
    }
}
