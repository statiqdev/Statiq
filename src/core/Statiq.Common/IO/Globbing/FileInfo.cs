using System;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace Statiq.Common
{
    internal class FileInfo : FileInfoBase
    {
        private readonly IFile _file;

        public FileInfo(IFile file)
        {
            _file = file.ThrowIfNull(nameof(file));
        }

        public override string Name => _file.Path.FileName.FullPath;

        public override string FullName => _file.Path.FullPath;

        public override DirectoryInfoBase ParentDirectory => new DirectoryInfo(_file.Directory);
    }
}
