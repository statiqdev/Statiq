using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Statiq.Common;

namespace Statiq.App
{
    internal sealed class EmptyCommandSettings : CommandSettings
    {
    }
}
