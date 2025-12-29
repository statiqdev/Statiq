using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperExtensions
    {
        public static TBootstrapper SetFailureLogLevel<TBootstrapper>(this TBootstrapper bootstrapper, LogLevel logLevel)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddSetting(Keys.FailureLogLevel, logLevel);
    }
}
