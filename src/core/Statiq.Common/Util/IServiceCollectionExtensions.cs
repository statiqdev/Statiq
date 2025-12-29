using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Gets the registered singleton instance for the given type, if and only if an instance was registered.
        /// If the registration is factory-based or for a different type, <c>null</c> will be returned.
        /// </summary>
        public static TService GetImplementationInstance<TService>(this IServiceCollection serviceCollection)
            where TService : class
        {
            serviceCollection.ThrowIfNull(nameof(serviceCollection));
            ServiceDescriptor serviceDescriptor = serviceCollection.FirstOrDefault(x => x.ServiceType == typeof(TService));
            return serviceDescriptor?.ImplementationInstance as TService;
        }

        /// <summary>
        /// Gets the registered singleton instance for the given type, if and only if an instance was registered.
        /// If the registration is factory-based or for a different type, <c>null</c> will be returned.
        /// </summary>
        public static TService GetRequiredImplementationInstance<TService>(this IServiceCollection serviceCollection)
            where TService : class
        {
            TService service = serviceCollection.GetImplementationInstance<TService>();
            if (service is null)
            {
                throw new Exception($"Could not get required service instance of {typeof(TService).Name}");
            }
            return service;
        }
    }
}