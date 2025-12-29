using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.App
{
    public class ConfigurableCommands : IConfigurable
    {
        internal ConfigurableCommands(Spectre.Console.Cli.IConfigurator configurator)
        {
            Configurator = configurator;
        }

        public Spectre.Console.Cli.IConfigurator Configurator { get; }
    }
}
