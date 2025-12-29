using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Razor
{
    public class RazorEngineInitializer : IEngineInitializer
    {
        public void Initialize(IEngine engine)
        {
            RazorService razorService = engine.Services.GetRequiredService<RazorService>();
            engine.Events.Subscribe<BeforeEngineExecution>(async args => await razorService.BeforeEngineExecutionAsync(args));
            engine.Events.Subscribe<AfterEngineExecution>(async args => await razorService.AfterEngineExecutionAsync(args));
        }
    }
}