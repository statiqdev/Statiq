using System;

namespace Statiq.Common
{
    public class ShortcodeParserException : Exception
    {
        public ShortcodeParserException(string message)
            : base(message)
        {
        }
    }
}
