using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Statiq.Common;

namespace Statiq.Core
{
    internal class ShortcodeCollection : IShortcodeCollection
    {
        private readonly Dictionary<string, Func<IShortcode>> _shortcodes =
            new Dictionary<string, Func<IShortcode>>(StringComparer.OrdinalIgnoreCase);

        public IShortcode CreateInstance(string name) => _shortcodes[name]();

        public void Add(string name, Func<IShortcode> shortcodeFactory)
        {
            name.ThrowIfNullOrWhiteSpace(nameof(name));
            if (name.Any(c => char.IsWhiteSpace(c)))
            {
                throw new ArgumentException("Shortcode names must not contain whitespace", nameof(name));
            }
            _shortcodes[name] = shortcodeFactory.ThrowIfNull(name);
        }

        public int Count => _shortcodes.Count;

        public bool Contains(string name) => _shortcodes.ContainsKey(name);

        public IEnumerator<string> GetEnumerator() => _shortcodes.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
