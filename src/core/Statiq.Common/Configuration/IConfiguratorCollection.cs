using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Contains a collection of configurators that can be used to configure a given type of object.
    /// </summary>
    public interface IConfiguratorCollection
    {
        IList<IConfigurator<TConfigurable>> Get<TConfigurable>()
            where TConfigurable : IConfigurable;

        bool TryGet<TConfigurable>(out IList<IConfigurator<TConfigurable>> configurators)
            where TConfigurable : IConfigurable;
    }
}
