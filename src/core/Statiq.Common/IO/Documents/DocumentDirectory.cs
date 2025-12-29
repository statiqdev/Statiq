using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a sequence of documents as a file system (I.e., for use in the globber).
    /// </summary>
    internal class DocumentDirectory : IDirectory
    {
        private readonly DocumentFileProvider _fileProvider;

        internal DocumentDirectory(DocumentFileProvider fileProvider, in NormalizedPath path)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));
            path.ThrowIfNull(nameof(path));
            Path = path;
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Directories
                    .Where(x => !Path.Equals(x) && Path.ContainsChild(x))
                    .Select(x => new DocumentDirectory(_fileProvider, x))
                    .Where(x => x.Exists)
                    .Cast<IDirectory>();
            }

            return _fileProvider.Directories
                .Where(x => !Path.Equals(x) && Path.ContainsDescendant(x))
                .Select(x => new DocumentDirectory(_fileProvider, x))
                .Where(x => x.Exists)
                .Cast<IDirectory>();
        }

        public IDirectory GetDirectory(NormalizedPath directory)
        {
            directory.ThrowIfNull(nameof(directory));

            if (!directory.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(directory));
            }

            return new DocumentDirectory(_fileProvider, Path.Combine(directory));
        }

        public bool Exists => _fileProvider.Directories.Contains(Path);

        public IFile GetFile(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new DocumentFile(_fileProvider, Path.Combine(path));
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Files.Keys
                    .Where(x => Path.ContainsChild(x))
                    .Select(x => new DocumentFile(_fileProvider, x))
                    .Where(x => x.Exists)
                    .Cast<IFile>();
            }

            return _fileProvider.Files.Keys
                .Where(x => Path.ContainsDescendant(x))
                .Select(x => new DocumentFile(_fileProvider, x))
                .Where(x => x.Exists)
                .Cast<IFile>();
        }

        public IDirectory Parent
        {
            get
            {
                NormalizedPath parentPath = Path.Parent;
                return parentPath.IsNull ? null : new DocumentDirectory(_fileProvider, parentPath);
            }
        }

        public DateTime LastWriteTime => throw new NotSupportedException();

        public DateTime CreationTime => throw new NotSupportedException();

        public void Create() => throw new NotSupportedException();

        public void Delete(bool recursive) => throw new NotSupportedException();

        public void MoveTo(NormalizedPath destinationPath) => throw new NotSupportedException();

        public void MoveTo(IDirectory destinationDirectory) => throw new NotSupportedException();

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();
    }
}
