using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// Wraps an <see cref="IFileProvider"/> and returns excluded (non-existing) files and directories
    /// if the path is excluded.
    /// </summary>
    internal class ExcludedFileProvider : IFileProvider
    {
        private readonly IFileSystem _fileSystem;
        private readonly IFileProvider _fileProvider;

        public ExcludedFileProvider(IFileSystem fileSystem, IFileProvider fileProvider)
        {
            _fileSystem = fileSystem;
            _fileProvider = fileProvider;
        }

        public IDirectory GetDirectory(NormalizedPath path)
        {
            NormalizedPath relativeInputPath = _fileSystem.GetRelativeInputPath(path);
            return !path.IsNull
                && !relativeInputPath.IsNull
                && _fileSystem
                    .ExcludedPaths
                    .Any(x => x.ContainsDescendantOrSelf(relativeInputPath))
                ? new ExcludedDirectory(this, path)
                : _fileProvider.GetDirectory(path);
        }

        public IFile GetFile(NormalizedPath path)
        {
            NormalizedPath relativeInputPath = _fileSystem.GetRelativeInputPath(path);
            return !path.IsNull
                && !relativeInputPath.IsNull
                && _fileSystem
                    .ExcludedPaths
                    .Any(x => x.ContainsDescendantOrSelf(relativeInputPath))
                ? new ExcludedFile(this, path)
                : _fileProvider.GetFile(path);
        }
    }
}
