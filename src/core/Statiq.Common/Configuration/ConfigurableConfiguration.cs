using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public class ConfigurableConfiguration : IConfigurable
    {
        public ConfigurableConfiguration(IConfigurationBuilder builder)
        {
            Builder = builder;
        }

        public IConfigurationBuilder Builder { get; }
    }
}
