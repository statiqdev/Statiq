using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Statiq.Common
{
    public static class BootstrapperSettingsExtensions
    {
        public static TBootstrapper ConfigureSettings<TBootstrapper>(this TBootstrapper bootstrapper, Action<ISettings> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableSettings>(x => action(x.Settings));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureSettings<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Action<ISettings, IServiceCollection> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableSettings>(x => action(x.Settings, x.ServiceCollection));
            return bootstrapper;
        }

        public static TBootstrapper ConfigureSettings<TBootstrapper>(
            this TBootstrapper bootstrapper,
            Action<ISettings, IServiceCollection, IReadOnlyFileSystem> action)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            action.ThrowIfNull(nameof(action));
            bootstrapper.Configurators.Add<ConfigurableSettings>(x => action(x.Settings, x.ServiceCollection, x.FileSystem));
            return bootstrapper;
        }

        public static TBootstrapper AddSettings<TBootstrapper>(this TBootstrapper bootstrapper, IEnumerable<KeyValuePair<string, object>> settings)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.AddOrReplaceRange(settings));

        public static TBootstrapper AddSetting<TBootstrapper>(this TBootstrapper bootstrapper, KeyValuePair<string, object> setting)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x[setting.Key] = setting.Value);

        public static TBootstrapper AddSetting<TBootstrapper>(this TBootstrapper bootstrapper, string key, object value)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x[key] = value);

        public static TBootstrapper AddSettingsIfNonExisting<TBootstrapper>(this TBootstrapper bootstrapper, IEnumerable<KeyValuePair<string, object>> settings)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.TryAddRange(settings));

        public static TBootstrapper AddSettingIfNonExisting<TBootstrapper>(this TBootstrapper bootstrapper, KeyValuePair<string, object> setting)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.TryAdd(setting.Key, setting.Value));

        public static TBootstrapper AddSettingIfNonExisting<TBootstrapper>(this TBootstrapper bootstrapper, string key, object value)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureSettings(x => x.TryAdd(key, value));
    }
}