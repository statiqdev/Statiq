using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.App
{
    public static class BoostrapperShortcodeExtensions
    {
        public static Bootstrapper AddShortcode<TShortcode>(this Bootstrapper bootstrapper)
            where TShortcode : IShortcode =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add<TShortcode>());

        public static Bootstrapper AddShortcode<TShortcode>(this Bootstrapper bootstrapper, string name)
            where TShortcode : IShortcode =>
            bootstrapper.ConfigureEngine(x => x.Shortcodes.Add<TShortcode>(name));
    }
}
