using System;

namespace Statiq.App
{
    internal struct ConsoleContent
    {
        public ConsoleContent(ConsoleColor foreground, in ReadOnlyMemory<char> message)
        {
            Foreground = foreground;
            Message = message;
        }

        public ConsoleContent(in ReadOnlyMemory<char> message)
        {
            Foreground = ConsoleColor.Gray;
            Message = message;
        }

        public ConsoleColor Foreground { get; }

        public ReadOnlyMemory<char> Message { get; }
    }
}