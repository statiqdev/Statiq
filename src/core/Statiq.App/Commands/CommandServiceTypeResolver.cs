using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Statiq.Common;

namespace Statiq.App
{
    internal class CommandServiceTypeResolver : ITypeResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandServiceTypeResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider.ThrowIfNull(nameof(serviceProvider));
        }

        public object Resolve(Type type) => _serviceProvider.GetService(type);
    }
}