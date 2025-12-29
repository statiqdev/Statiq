using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Statiq.App
{
    // Wraps a standalone service collection just for command use
    // Singleton resolutions must *only* be done on types that were registered as instances, not constructed by the service provider
    internal class CommandServiceTypeRegistrar : ITypeRegistrar
    {
        private readonly IServiceCollection _serviceCollection = new ServiceCollection();

        public void Register(Type serviceType, Type implementation) =>
            _serviceCollection.AddScoped(serviceType, implementation);

        public void RegisterInstance(Type serviceType, object implementation) =>
            _serviceCollection.AddScoped(serviceType, _ => implementation);

        public void RegisterLazy(Type serviceType, Func<object> factory) =>
            _serviceCollection.AddScoped(serviceType, _ => factory());

        public ITypeResolver Build() => new CommandServiceTypeResolver(_serviceCollection.BuildServiceProvider());
    }
}