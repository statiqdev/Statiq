using Microsoft.Extensions.Configuration;
using Statiq.Common;

namespace Statiq.App
{
    public static class IConfigurationBuilderExtensions
    {
        /// <summary>
        /// Adds a settings file with support for JSON, YAML, and XML variants.
        /// </summary>
        /// <param name="builder">The configuration builder.</param>
        /// <param name="path">The path or filename of the settings file to add.</param>
        /// <returns>The configuration builder with the new settings file.</returns>
        public static IConfigurationBuilder AddSettingsFile(this IConfigurationBuilder builder, in NormalizedPath path) =>
            builder
                .AddJsonFile(path.ChangeExtension(".json").FullPath, true)
                .AddYamlFile(path.ChangeExtension(".yml").FullPath, true)
                .AddYamlFile(path.ChangeExtension(".yaml").FullPath, true)
                .AddXmlFile(path.ChangeExtension(".xml").FullPath, true);
    }
}
