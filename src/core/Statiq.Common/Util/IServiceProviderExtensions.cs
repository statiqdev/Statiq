using System;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public static class IServiceProviderExtensions
    {
        /// <summary>
        /// Creates an instance from a service provider given a service descriptor.
        /// </summary>
        /// <remarks>
        /// Adapted from https://greatrexpectations.com/2018/10/25/decorators-in-net-core-with-dependency-injection.
        /// </remarks>
        public static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(services);
            }

            return ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType);
        }
    }
}