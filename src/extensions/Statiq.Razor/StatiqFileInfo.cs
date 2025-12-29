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
    internal class StatiqFileInfo : IFileInfo
    {
        // Pass the path in separately to return the correct input-relative path
        public StatiqFileInfo(IFile file, string path)
        {
            File = file;
            PhysicalPath = path;
        }

        public IFile File { get; }

        public bool Exists => File.Exists;

        public long Length => File.Length;

        public string PhysicalPath { get; }

        public string Name => File.Path.FileName.FullPath;

        public DateTimeOffset LastModified => DateTimeOffset.Now;

        public bool IsDirectory => false;

        public Stream CreateReadStream() => File.OpenRead();
    }
}
