using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Statiq.Common
{
    internal class VirtualInputDirectory : IDirectory
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public VirtualInputDirectory(IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            Path = path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Virtual input paths should always be relative", nameof(path));
            }
        }

        /// <inheritdoc/>
        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        /// <inheritdoc/>
        public IDirectory Parent
        {
            get
            {
                NormalizedPath parentPath = Path.Parent;
                return parentPath.IsNull ? null : new VirtualInputDirectory(_fileSystem, parentPath);
            }
        }

        public DateTime LastWriteTime => throw new NotSupportedException();

        public DateTime CreationTime => throw new NotSupportedException();

        public void Create() => throw new NotSupportedException("Can not create a virtual input directory");

        public void Delete(bool recursive) => throw new NotSupportedException("Can not delete a virtual input directory");

        public void MoveTo(NormalizedPath destinationPath) => throw new NotSupportedException("Can not move a virtual input directory");

        public void MoveTo(IDirectory destinationDirectory) => throw new NotSupportedException("Can not move a virtual input directory");

        /// <inheritdoc/>
        public IEnumerable<IDirectory> GetDirectories(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the relative child directories for each existing input directory (I.e. "select many")
            Dictionary<NormalizedPath, VirtualInputDirectory> directories = new Dictionary<NormalizedPath, VirtualInputDirectory>();
            foreach (IDirectory existing in GetExistingInputDirectories())
            {
                // Add this one if it's virtual (assumption is that real directories will already exist)
                if (existing is VirtualInputDirectory)
                {
                    NormalizedPath relativePath = Path.GetRelativePath(existing.Path);
                    if (!directories.ContainsKey(relativePath))
                    {
                        directories[relativePath] = new VirtualInputDirectory(_fileSystem, Path.Combine(relativePath));
                    }
                }

                // Only descend virtual directories if we're getting all directories
                if (!(existing is VirtualInputDirectory) || searchOption != SearchOption.TopDirectoryOnly)
                {
                    foreach (IDirectory childDirectory in existing.GetDirectories(searchOption))
                    {
                        // Get the relative path starting from the current directory path
                        NormalizedPath relativePath = existing is VirtualInputDirectory
                            ? Path.GetRelativePath(childDirectory.Path)
                            : existing.Path.GetRelativePath(childDirectory.Path);
                        if (!directories.ContainsKey(relativePath))
                        {
                            directories[relativePath] = new VirtualInputDirectory(_fileSystem, Path.Combine(relativePath));
                        }
                    }
                }
            }
            return directories.Values;
        }

        /// <inheritdoc/>
        public IEnumerable<IFile> GetFiles(SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            // Get all the files for each input directory, replacing earlier ones with later ones
            Dictionary<NormalizedPath, VirtualInputFile> files = new Dictionary<NormalizedPath, VirtualInputFile>();
            foreach (IDirectory existing in GetExistingInputDirectories())
            {
                // Only descend virtual directories if we're getting all files
                if (!(existing is VirtualInputDirectory) || searchOption != SearchOption.TopDirectoryOnly)
                {
                    foreach (IFile file in existing.GetFiles(searchOption))
                    {
                        if (!files.ContainsKey(file.Path))
                        {
                            files[file.Path] = new VirtualInputFile(file, this);
                        }
                    }
                }
            }
            return files.Values;
        }

        /// <inheritdoc/>
        public IDirectory GetDirectory(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new VirtualInputDirectory(_fileSystem, Path.Combine(path));
        }

        /// <inheritdoc/>
        public IFile GetFile(NormalizedPath path)
        {
            path.ThrowIfNull(nameof(path));

            if (!path.IsRelative)
            {
                throw new ArgumentException("Path must be relative", nameof(path));
            }

            return new VirtualInputFile(_fileSystem.GetInputFile(Path.Combine(path)), this);
        }

        /// <summary>
        /// Gets a value indicating whether any of the input paths contain this directory.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this directory exists at one of the input paths; otherwise, <c>false</c>.
        /// </returns>
        public bool Exists => GetExistingInputDirectories().Any();

        /// <summary>
        /// Gets this path under each input directory and returns the ones that exist.
        /// Also returns virtual input directories for any of the mapped paths that don't actually exist.
        /// </summary>
        // Internal for testing
        internal IEnumerable<IDirectory> GetExistingInputDirectories() =>
            _fileSystem
                .GetUnmappedInputPaths(Path, out HashSet<NormalizedPath> nonExistingMappedPaths)
                .Select(x => _fileSystem.GetDirectory(x))
                .Where(x => x.Exists)
                .Concat(nonExistingMappedPaths
                    .Where(x => Path.ContainsDescendantOrSelf(x))
                    .Select(x => Path.Combine(Path.GetRelativePath(x).Segments.FirstOrDefault().ToString()))
                    .Distinct()
                    .Select(x => new VirtualInputDirectory(_fileSystem, x)));

        public override string ToString() => Path.ToString();

        /// <inheritdoc/>
        public string ToDisplayString() => Path.ToDisplayString();
    }
}
