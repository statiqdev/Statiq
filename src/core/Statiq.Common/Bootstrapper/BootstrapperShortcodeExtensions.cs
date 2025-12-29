using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class BootstrapperShortcodeExtensions
    {
        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, Type type)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(type));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Type type)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, type));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Config<ShortcodeResult> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, ShortcodeResult> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, IEnumerable<ShortcodeResult>> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<ShortcodeResult>> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));

        public static TBootstrapper AddShortcode<TBootstrapper>(this TBootstrapper bootstrapper, string name, Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<ShortcodeResult>>> shortcode)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add(name, shortcode));
    }
}
