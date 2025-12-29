namespace Statiq.App
{
    public static class BootstrapperFactoryExtensions
    {
        /// <summary>
        /// Creates a bootstrapper with a default configuration including logging, commands,
        /// shortcodes, and assembly scanning.
        /// </summary>
        /// <param name="factory">The bootstrapper factory.</param>
        /// <param name="args">The command line arguments.</param>
        /// <param name="features">The default configurations to add to the bootstrapper.</param>
        /// <returns>The bootstrapper.</returns>
        public static Bootstrapper CreateDefault(this BootstrapperFactory factory, string[] args, DefaultFeatures features = DefaultFeatures.All) =>
            factory.Create(args).AddDefaults(features);

        public static Bootstrapper CreateDefaultWithout(this BootstrapperFactory factory, string[] args, DefaultFeatures features) =>
            factory.Create(args).AddDefaultsWithout(features);
    }
}
