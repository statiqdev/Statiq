using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Version = SemanticVersioning.Version;

namespace Statiq.Core
{
    public static class IEngineExtensions
    {
        public static void LogAndCheckVersion(this IEngine engine, Assembly assembly, string name, string minimumVersionKey)
        {
            if (!(Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) is AssemblyInformationalVersionAttribute versionAttribute))
            {
                throw new Exception($"Could not determine the {name} version from {assembly.FullName}");
            }

            // Get and print the version
            string informationalVersion = versionAttribute.InformationalVersion;
            engine.Logger.LogInformation($"{name} version {informationalVersion}", true);
            Version version = new Version(informationalVersion, true);

            // Get all version ranges
            (string Key, Version Version)[] minimumVersions = engine.Settings.Keys
                .Where(k => k.StartsWith(minimumVersionKey))
                .Select(k => (Key: k, Value: engine.Settings.GetString(k)))
                .Where(x => !x.Value.IsNullOrWhiteSpace())
                .Select(x => (x.Key, new Version(x.Value, true)))
                .ToArray();
            foreach ((string Key, Version Version) minimumVersion in minimumVersions)
            {
                if (version < minimumVersion.Version)
                {
                    throw new Exception($"{name} {informationalVersion} does not meet the minimum version requirement of {minimumVersion.Version} defined in {minimumVersion.Key}");
                }
            }
        }
    }
}
