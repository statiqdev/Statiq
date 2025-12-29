using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Statiq.Common;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Statiq.Razor
{
    internal class StatiqDirectoryInfo : IFileInfo
    {
        // Pass the path in separately to return the correct input-relative path
        public StatiqDirectoryInfo(IDirectory directory, string path)
        {
            Directory = directory;
            PhysicalPath = path;
        }

        public IDirectory Directory { get; }

        public bool Exists => Directory.Exists;

        public long Length => -1L;

        public string PhysicalPath { get; }

        public string Name => Directory.Path.Name;

        public DateTimeOffset LastModified => DateTimeOffset.Now;

        public bool IsDirectory => true;

        public Stream CreateReadStream()
        {
            throw new InvalidOperationException("Cannot create a stream for a directory.");
        }
    }
}
