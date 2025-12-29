using System;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Testing
{
    public class TestServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public TestServiceScopeFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IServiceScope CreateScope() => new TestServiceScope(_serviceProvider);
    }
}
