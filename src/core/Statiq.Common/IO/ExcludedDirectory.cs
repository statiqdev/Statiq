using System;
using System.Collections.Generic;
using System.IO;

namespace Statiq.Common
{
    /// <summary>
    /// A directory that is excluded from the file system due to
    /// <see cref="IFileSystem.ExcludedPaths"/> (I.e. the directory
    /// is non-existing).
    /// </summary>
    internal class ExcludedDirectory : IDirectory
    {
        private readonly IFileProvider _fileProvider;

        public ExcludedDirectory(IFileProvider fileProvider, in NormalizedPath path)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));
            Path = path;
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => false;

        public IDirectory Parent => _fileProvider.GetDirectory(Path.Parent);

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();

        public DateTime LastWriteTime =>
            throw new NotSupportedException("Not supported for an excluded path");

        public DateTime CreationTime =>
            throw new NotSupportedException("Not supported for an excluded path");

        public void Create() =>
            throw new NotSupportedException("Not supported for an excluded path");

        public void Delete(bool recursive) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public void MoveTo(NormalizedPath destinationPath) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public void MoveTo(IDirectory destinationDirectory) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public IDirectory GetDirectory(NormalizedPath directory) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public IFile GetFile(NormalizedPath path) =>
            throw new NotSupportedException("Not supported for an excluded path");
    }
}
