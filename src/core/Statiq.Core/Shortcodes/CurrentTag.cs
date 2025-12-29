using System.Text;

namespace Statiq.Core
{
    internal class CurrentTag
    {
        public StringBuilder Content { get; } = new StringBuilder();
        public int FirstIndex { get; }

        public CurrentTag(int firstIndex)
        {
            FirstIndex = firstIndex;
        }
    }
}
