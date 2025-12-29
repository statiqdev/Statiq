using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestDirectory : IDirectory
    {
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly TestFileProvider _fileProvider;

        public TestDirectory(IReadOnlyFileSystem fileSystem, TestFileProvider fileProvider, in NormalizedPath path)
        {
            _fileSystem = fileSystem;
            _fileProvider = fileProvider;

            // Use a slash for an empty path (can't compare to NormalizePath.Empty since this might be absolute
            // in the case of a ".." from the globber, and that's always relative, so compare against FullPath)
            Path = path.FullPath == string.Empty ? NormalizedPath.Slash : path;
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public bool Exists => Path == NormalizedPath.Slash || _fileProvider.Directories.Contains(Path);

        public DateTime LastWriteTime { get; set; }

        public DateTime CreationTime { get; set; }

        public IDirectory Parent
        {
            get
            {
                NormalizedPath parentPath = Path.Parent;
                return parentPath.IsNull ? null : _fileSystem.GetDirectory(parentPath);
            }
        }

        public void Create() => _fileProvider.Directories.Add(Path);

        public void Delete(bool recursive) => _fileProvider.Directories.Remove(Path);

        public void MoveTo(NormalizedPath destinationPath) => throw new NotImplementedException();

        public void MoveTo(IDirectory destinationDirectory) => throw new NotImplementedException();

        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Directories
                    .Where(x => Path.ContainsChild(x))
                    .Select(x => _fileSystem.GetDirectory(x))
                    .Where(x => x.Exists);
            }
            return _fileProvider.Directories
                .Where(x => Path.ContainsDescendant(x))
                .Select(x => _fileSystem.GetDirectory(x))
                .Where(x => x.Exists);
        }

        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            if (searchOption == SearchOption.TopDirectoryOnly)
            {
                return _fileProvider.Files.Keys
                    .Where(x => Path.ContainsChild(x))
                    .Select(x => _fileSystem.GetFile(x))
                    .Where(x => x.Exists);
            }
            return _fileProvider.Files.Keys
                .Where(x => Path.ContainsDescendant(x))
                .Select(x => _fileSystem.GetFile(x))
                .Where(x => x.Exists);
        }

        public IDirectory GetDirectory(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return _fileSystem.GetDirectory(Path.Combine(path));
        }

        public IFile GetFile(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return _fileSystem.GetFile(Path.Combine(path));
        }

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToSafeDisplayString();
    }
}