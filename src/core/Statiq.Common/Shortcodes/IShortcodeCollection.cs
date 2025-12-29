using System;

namespace Statiq.Common
{
    public interface IShortcodeCollection : IReadOnlyShortcodeCollection
    {
        /// <summary>
        /// Adds a shortcode using a factory.
        /// </summary>
        /// <param name="name">The name of the shortcode.</param>
        /// <param name="shortcodeFactory">A factory that returns an <see cref="IShortcode"/>.</param>
        void Add(string name, Func<IShortcode> shortcodeFactory);
    }
}
