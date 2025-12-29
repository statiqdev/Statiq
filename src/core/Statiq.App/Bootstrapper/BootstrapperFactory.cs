using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    /// <summary>
    /// A factory that creates the <see cref="Bootstrapper"/>. Access the singleton
    /// instance through <see cref="Bootstrapper.Factory"/>.
    /// </summary>
    public class BootstrapperFactory
    {
        internal BootstrapperFactory()
        {
        }

        /// <summary>
        /// Creates an empty bootstrapper without any default configuration.
        /// </summary>
        /// <remarks>
        /// Use this method when you want to fully customize the bootstrapper and engine. Otherwise use one of the
        /// <see cref="BootstrapperFactoryExtensions.CreateDefault(BootstrapperFactory, string[], DefaultFeatures)"/>
        /// extensions  to create an initialize a bootstrapper with an initial set of default configurations.
        /// </remarks>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The bootstrapper.</returns>
        public Bootstrapper Create(string[] args) => new Bootstrapper(args);
    }
}
