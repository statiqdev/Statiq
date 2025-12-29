using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IConfiguratorCollectionExtensions
    {
        public static void Add<TConfigurable, TConfigurator>(this IConfiguratorCollection configuratorCollection)
            where TConfigurable : IConfigurable
            where TConfigurator : IConfigurator<TConfigurable> =>
            configuratorCollection.Get<TConfigurable>().Add(Activator.CreateInstance<TConfigurator>());

        public static void Add<TConfigurable>(this IConfiguratorCollection configuratorCollection, Action<TConfigurable> action)
            where TConfigurable : IConfigurable =>
            configuratorCollection.Add(new DelegateConfigurator<TConfigurable>(action));

        public static void Add<TConfigurable>(this IConfiguratorCollection configuratorCollection, IConfigurator<TConfigurable> configurator)
            where TConfigurable : IConfigurable =>
            configuratorCollection.Get<TConfigurable>().Add(configurator);

        /// <summary>
        /// Applies all applicable configurators in the collection to the provided configurable.
        /// </summary>
        /// <typeparam name="TConfigurable">The type of configurable object.</typeparam>
        /// <param name="configuratorCollection">The configurator collection.</param>
        /// <param name="configurable">The object to configure.</param>
        public static void Configure<TConfigurable>(this IConfiguratorCollection configuratorCollection, TConfigurable configurable)
            where TConfigurable : IConfigurable
        {
            if (configuratorCollection.TryGet(out IList<IConfigurator<TConfigurable>> configurators))
            {
                foreach (IConfigurator<TConfigurable> configurator in configurators)
                {
                    configurator?.Configure(configurable);
                }
            }
        }
    }
}
