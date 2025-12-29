using System;
using System.IO;
using Statiq.Common;

namespace Statiq.Testing
{
    public static class StreamExtensions
    {
        public static string ReadToEnd(this Stream stream)
        {
            stream.ThrowIfNull(nameof(stream));
            stream.Seek(0, SeekOrigin.Begin);
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
