using System.Collections.Generic;

namespace Statiq.Core
{
    internal class ShortcodeLocation
    {
        public ShortcodeLocation(int firstIndex, string name, KeyValuePair<string, string>[] arguments)
        {
            FirstIndex = firstIndex;
            Name = name;
            Arguments = arguments;
        }

        public void Finish(int lastIndex)
        {
            LastIndex = lastIndex;
        }

        public int FirstIndex { get; }
        public string Name { get; }
        public KeyValuePair<string, string>[] Arguments { get; }

        public string Content { get; set; } = string.Empty;

        public int LastIndex { get; private set; }
    }
}
