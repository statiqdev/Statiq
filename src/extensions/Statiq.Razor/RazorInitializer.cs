using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Razor
{
    public class RazorInitializer : IBootstrapperInitializer
    {
        public void Configure(IBootstrapper bootstrapper) => bootstrapper.ConfigureServices(x => x.AddRazor());
    }
}