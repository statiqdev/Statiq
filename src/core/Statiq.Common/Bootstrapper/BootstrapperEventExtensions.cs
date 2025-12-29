namespace Statiq.Common
{
    public static class BootstrapperEventExtensions
    {
        public static IBootstrapper SubscribeEvent<TEvent>(this IBootstrapper bootstrapper, AsyncEventHandler<TEvent> handler) =>
            bootstrapper.ConfigureEngine(x => x.Events.Subscribe(handler));

        public static IBootstrapper SubscribeEvent<TEvent>(this IBootstrapper bootstrapper, EventHandler<TEvent> handler) =>
            bootstrapper.ConfigureEngine(x => x.Events.Subscribe(handler));
    }
}
