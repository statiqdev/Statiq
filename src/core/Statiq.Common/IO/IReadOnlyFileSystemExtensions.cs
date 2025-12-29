using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Common
{
    public static class IReadOnlyFileSystemExtensions
    {
        /// <summary>
        /// Given a relative input path this "unmaps" it for each input path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to "unmap".</param>
        /// <param name="nonExistingMappedPaths">
        /// Some input paths won't map directly to a path in the file system (I.e. if the mapped
        /// path is deeper than the real one). This contains all the mapped input paths that couldn't be unmapped.
        /// </param>
        /// <returns>The "unmapped" input paths.</returns>
        public static IEnumerable<NormalizedPath> GetUnmappedInputPaths(
            this IReadOnlyFileSystem fileSystem,
            in NormalizedPath path,
            out HashSet<NormalizedPath> nonExistingMappedPaths)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            path.ThrowIfNull(nameof(path));
            if (!path.IsRelative)
            {
                throw new ArgumentException("The input path to unmap must be relative", nameof(path));
            }

            List<NormalizedPath> unmappedInputPaths = new List<NormalizedPath>();
            nonExistingMappedPaths = new HashSet<NormalizedPath>();
            foreach (NormalizedPath inputPath in fileSystem.InputPaths)
            {
                // Is this input path mapped?
                if (fileSystem.InputPathMappings.TryGetValue(inputPath, out NormalizedPath mappedInputPath))
                {
                    // Does this path exist under the mapped path (I.e. it's a real path)
                    if (mappedInputPath.ContainsDescendantOrSelf(path))
                    {
                        unmappedInputPaths.Add(fileSystem.RootPath.Combine(inputPath).Combine(mappedInputPath.GetRelativePath(path)));
                    }
                    else
                    {
                        // Otherwise it's a virtual path that doesn't actually exist
                        nonExistingMappedPaths.Add(mappedInputPath);
                    }
                }
                else
                {
                    // Just a normal input path
                    unmappedInputPaths.Add(fileSystem.RootPath.Combine(inputPath).Combine(path));
                }
            }
            return unmappedInputPaths;
        }

        /// <summary>
        /// Gets a file representing an input.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the input file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then operations will search all
        /// current input paths.
        /// </param>
        /// <returns>An input file.</returns>
        public static IFile GetInputFile(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            path.ThrowIfNull(nameof(path));

            if (path.IsRelative)
            {
                IFile notFound = null;
                foreach (NormalizedPath inputPath in fileSystem.GetUnmappedInputPaths(path, out _).Reverse())
                {
                    IFile file = fileSystem.GetFile(inputPath);
                    if (notFound is null)
                    {
                        notFound = file;
                    }
                    if (file.Exists)
                    {
                        return file;
                    }
                }
                if (notFound is null)
                {
                    throw new InvalidOperationException("The input paths collection must have at least one path");
                }
                return notFound;
            }
            return fileSystem.GetFile(path);
        }

        /// <summary>
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        public static IEnumerable<IFile> GetInputFiles(this IReadOnlyFileSystem fileSystem, params string[] patterns)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetInputFiles((IEnumerable<string>)patterns);
        }

        /// <summary>
        /// Gets matching input files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>All input files that match the globbing patterns and/or absolute paths.</returns>
        public static IEnumerable<IFile> GetInputFiles(this IReadOnlyFileSystem fileSystem, IEnumerable<string> patterns)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFiles(fileSystem.GetInputDirectory(), patterns);
        }

        /// <summary>
        /// Gets all absolute input directories.
        /// </summary>
        /// <returns>The absolute input directories.</returns>
        public static IEnumerable<IDirectory> GetInputDirectories(this IReadOnlyFileSystem fileSystem)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.InputPaths.Select(x => fileSystem.GetRootDirectory(x)).ToImmutableArray();
        }

        /// <summary>
        /// Gets the absolute input path that contains the specified file or directory. If the provided
        /// file or directory path is absolute, this returns the input path that contains the specified
        /// path (note that the specified file or directory does not need to exist and this just returns
        /// the input path that would contain the file or directory based only on path information). If
        /// the provided path is relative, this checks all input paths for the existence of the file
        /// or directory and returns the first one where it exists.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The file path.</param>
        /// <returns>The input path that contains the specified file,
        /// or <c>null</c> if no input path does.</returns>
        public static NormalizedPath GetContainingInputPath(this IReadOnlyFileSystem fileSystem, NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            path.ThrowIfNull(nameof(path));

            if (path.IsAbsolute)
            {
                return fileSystem.GetContainingInputPathForAbsolutePath(path).AbsoluteInputPath;
            }

            // Try to find a file first
            IFile file = fileSystem.GetInputFile(path);
            if (file.Exists)
            {
                return fileSystem.GetContainingInputPath(file.Path);
            }

            // Then try to find a directory
            return fileSystem.GetUnmappedInputPaths(path, out _)
                .Reverse()
                .Select(x => (x, fileSystem.GetDirectory(x)))
                .Where(x => x.Item2.Exists)
                .Select(x => x.Item1.Parent)
                .FirstOrDefault();
        }

        internal static (NormalizedPath InputPath, NormalizedPath AbsoluteInputPath) GetContainingInputPathForAbsolutePath(this IReadOnlyFileSystem fileSystem, NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            path.ThrowIfNull(nameof(path));

            if (!path.IsAbsolute)
            {
                throw new ArgumentException("Path must be absolute");
            }

            return fileSystem.InputPaths
                .Reverse()
                .Select(x => (x, fileSystem.RootPath.Combine(x)))
                .FirstOrDefault(x => x.Item2.ContainsDescendantOrSelf(path));
        }

        /// <summary>
        /// Gets a path to the specified file relative to it's containing input directory.
        /// If no input directories contain this file, then a null path is returned.
        /// </summary>
        /// <returns>A path to this file relative to it's containing input directory.</returns>
        public static NormalizedPath GetRelativeInputPath(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            path.ThrowIfNull(nameof(path));

            if (path.IsRelative)
            {
                return path;
            }

            (NormalizedPath inputPath, NormalizedPath absoluteInputPath) = fileSystem.GetContainingInputPathForAbsolutePath(path);
            NormalizedPath relativeInputPath = absoluteInputPath.IsNull ? NormalizedPath.Null : absoluteInputPath.GetRelativePath(path);
            return fileSystem.InputPathMappings.TryGetValue(inputPath, out NormalizedPath mappedInputPath)
                ? mappedInputPath.Combine(relativeInputPath)
                : relativeInputPath;
        }

        /// <summary>
        /// Gets a path to the specified file relative to the output directory.
        /// If this path is not relative to the output directory, then a null path is returned.
        /// </summary>
        /// <returns>
        /// A path to this file relative to the output directory,
        /// the original path if the path is relative,
        /// or a null path if the path is not relative to the output directory.
        /// </returns>
        public static NormalizedPath GetRelativeOutputPath(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            path.ThrowIfNull(nameof(path));

            if (path.IsRelative)
            {
                return path;
            }

            NormalizedPath outputPath = fileSystem.GetOutputPath();
            return outputPath.ContainsDescendantOrSelf(path)
                ? outputPath.GetRelativePath(path)
                : NormalizedPath.Null;
        }

        /// <summary>
        /// Gets a path to the specified file relative to the output directory.
        /// If this path is not relative to the output directory, then the original path is returned.
        /// </summary>
        /// <returns>
        /// A path to this file relative to the output directory,
        /// the original path if the path is relative,
        /// or the original path if the path is not relative to the output directory.</returns>
        public static NormalizedPath GetRelativeOutputPathOrSelf(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            NormalizedPath relativeOutputPath = fileSystem.GetRelativeOutputPath(path);
            return relativeOutputPath.IsNull ? path : relativeOutputPath;
        }

        /// <summary>
        /// Gets the output path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>The output path.</returns>
        public static NormalizedPath GetOutputPath(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetOutputPath(NormalizedPath.Null);

        /// <summary>
        /// Gets an output path by combining it with the root path and output path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path and output path.
        /// If this is <see cref="NormalizedPath.Null"/>, returns the root path combined with the output path.</param>
        /// <returns>The output path.</returns>
        public static NormalizedPath GetOutputPath(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return path.IsNull
                ? fileSystem.RootPath.Combine(fileSystem.OutputPath)
                : fileSystem.RootPath.Combine(fileSystem.OutputPath).Combine(path);
        }

        /// <summary>
        /// Gets a file representing an output.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the output file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path.
        /// </param>
        /// <returns>An output file.</returns>
        public static IFile GetOutputFile(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFile(fileSystem.GetOutputPath(path));
        }

        /// <summary>
        /// Gets a directory representing an output.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>An output directory.</returns>
        public static IDirectory GetOutputDirectory(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetOutputDirectory(NormalizedPath.Null);

        /// <summary>
        /// Gets a directory representing an output.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the output directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current output path. If this is <c>null</c> then the base
        /// output directory is returned.
        /// </param>
        /// <returns>An output directory.</returns>
        public static IDirectory GetOutputDirectory(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetDirectory(fileSystem.GetOutputPath(path));
        }

        /// <summary>
        /// Gets the temp path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>The temp path.</returns>
        public static NormalizedPath GetTempPath(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetTempPath(NormalizedPath.Null);

        /// <summary>
        /// Gets a temp path by combining it with the root path and temp path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path and temp path.
        /// If this is <see cref="NormalizedPath.Null"/>, returns the root path combined with the temp path.</param>
        /// <returns>The temp path.</returns>
        public static NormalizedPath GetTempPath(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return path.IsNull
                ? fileSystem.RootPath.Combine(fileSystem.TempPath)
                : fileSystem.RootPath.Combine(fileSystem.TempPath).Combine(path);
        }

        /// <summary>
        /// Gets a file representing a temp file.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path.
        /// </param>
        /// <returns>A temp file.</returns>
        public static IFile GetTempFile(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFile(fileSystem.GetTempPath(path));
        }

        /// <summary>
        /// Gets a file representing a temp file with a random file name.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>A temp file.</returns>
        public static IFile GetTempFile(this IReadOnlyFileSystem fileSystem)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetTempFile(Path.ChangeExtension(Path.GetRandomFileName(), "tmp"));
        }

        /// <summary>
        /// Gets a directory representing temp files.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>A temp directory.</returns>
        public static IDirectory GetTempDirectory(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetTempDirectory(NormalizedPath.Null);

        /// <summary>
        /// Gets a directory representing temp files.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the temp directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current temp path. If this is <c>null</c> then the base
        /// temp directory is returned.
        /// </param>
        /// <returns>A temp directory.</returns>
        public static IDirectory GetTempDirectory(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetDirectory(fileSystem.GetTempPath(path));
        }

        /// <summary>
        /// Gets the cache path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>The cache path.</returns>
        public static NormalizedPath GetCachePath(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetCachePath(NormalizedPath.Null);

        /// <summary>
        /// Gets a cache path by combining it with the root path and cache path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path and cache path.
        /// If this is <see cref="NormalizedPath.Null"/>, returns the root path combined with the cache path.</param>
        /// <returns>The cache path.</returns>
        public static NormalizedPath GetCachePath(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return path.IsNull
                ? fileSystem.RootPath.Combine(fileSystem.CachePath)
                : fileSystem.RootPath.Combine(fileSystem.CachePath).Combine(path);
        }

        /// <summary>
        /// Gets a file representing a cache file.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current cache path.
        /// </param>
        /// <returns>A cache file.</returns>
        public static IFile GetCacheFile(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFile(fileSystem.GetCachePath(path));
        }

        /// <summary>
        /// Gets a file representing a cache file with a random file name.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>A cache file.</returns>
        public static IFile GetCacheFile(this IReadOnlyFileSystem fileSystem)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetCacheFile(Path.ChangeExtension(Path.GetRandomFileName(), "tmp"));
        }

        /// <summary>
        /// Gets a directory representing cache files.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>A cache directory.</returns>
        public static IDirectory GetCacheDirectory(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetCacheDirectory(NormalizedPath.Null);

        /// <summary>
        /// Gets a directory representing cache files.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the cache directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current cache path. If this is <c>null</c> then the base
        /// cache directory is returned.
        /// </param>
        /// <returns>A cache directory.</returns>
        public static IDirectory GetCacheDirectory(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetDirectory(fileSystem.GetCachePath(path));
        }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>The root path.</returns>
        public static NormalizedPath GetRootPath(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetRootPath(NormalizedPath.Null);

        /// <summary>
        /// Gets a path by combining it with the root path.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">The path to combine with the root path.
        /// If this is <see cref="NormalizedPath.Null"/>, returns the root path.</param>
        /// <returns>The path.</returns>
        public static NormalizedPath GetRootPath(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return path.IsNull
                ? fileSystem.RootPath
                : fileSystem.RootPath.Combine(path);
        }

        /// <summary>
        /// Gets a file representing a root file.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the root file. If this is an absolute path,
        /// then a file representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path.
        /// </param>
        /// <returns>A root file.</returns>
        public static IFile GetRootFile(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFile(fileSystem.RootPath.Combine(path));
        }

        /// <summary>
        /// Gets a directory representing a root directory.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>A root directory.</returns>
        public static IDirectory GetRootDirectory(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetRootDirectory(NormalizedPath.Null);

        /// <summary>
        /// Gets a directory representing a root directory.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The path of the root directory. If this is an absolute path,
        /// then a directory representing the specified path is returned.
        /// If it's a relative path, then it will be combined with the
        /// current root path. If this is <c>null</c> then the base
        /// root directory is returned.
        /// </param>
        /// <returns>A root directory.</returns>
        public static IDirectory GetRootDirectory(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return path.IsNull
                ? fileSystem.GetDirectory(fileSystem.RootPath)
                : fileSystem.GetDirectory(fileSystem.RootPath.Combine(path));
        }

        /// <summary>
        /// Gets an absolute file.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The absolute path of the file.
        /// </param>
        /// <returns>A file.</returns>
        public static IFile GetFile(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.FileProvider.GetFile(path);
        }

        /// <summary>
        /// Gets an absolute directory.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The absolute path of the directory.
        /// </param>
        /// <returns>A directory.</returns>
        public static IDirectory GetDirectory(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.FileProvider.GetDirectory(path);
        }

        /// <summary>
        /// Gets the root virtual input directory.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <returns>The root virtual input.</returns>
        public static IDirectory GetInputDirectory(this IReadOnlyFileSystem fileSystem) =>
            fileSystem.GetInputDirectory(NormalizedPath.Null);

        /// <summary>
        /// Gets a directory representing an input.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="path">
        /// The relative path of the virtual input directory or a null path
        /// for the root virtual input directory. This method will throw
        /// for an absolute path.
        /// </param>
        /// <returns>A virtual input directory.</returns>
        public static IDirectory GetInputDirectory(this IReadOnlyFileSystem fileSystem, in NormalizedPath path)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return path.IsNull
                ? new VirtualInputDirectory(fileSystem, NormalizedPath.Empty)
                : (path.IsRelative ? new VirtualInputDirectory(fileSystem, path) : fileSystem.GetDirectory(path));
        }

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static IEnumerable<IFile> GetFiles(this IReadOnlyFileSystem fileSystem, params string[] patterns)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFiles(fileSystem.GetRootDirectory(), patterns);
        }

        /// <summary>
        /// Gets matching files based on globbing patterns from the root path or absolute paths.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static IEnumerable<IFile> GetFiles(this IReadOnlyFileSystem fileSystem, IEnumerable<string> patterns)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFiles(fileSystem.GetRootDirectory(), patterns);
        }

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static IEnumerable<IFile> GetFiles(this IReadOnlyFileSystem fileSystem, IDirectory directory, params string[] patterns)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            return fileSystem.GetFiles(directory, (IEnumerable<string>)patterns);
        }

        /// <summary>
        /// Gets matching files based on globbing patterns and/or absolute paths. If any absolute paths
        /// are provided, only those that actually exist are returned.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="directory">The directory to search.</param>
        /// <param name="patterns">The globbing patterns and/or absolute paths.</param>
        /// <returns>
        /// All files in the specified directory that match the globbing patterns and/or absolute paths.
        /// </returns>
        public static IEnumerable<IFile> GetFiles(this IReadOnlyFileSystem fileSystem, IDirectory directory, IEnumerable<string> patterns)
        {
            fileSystem.ThrowIfNull(nameof(fileSystem));
            directory.ThrowIfNull(nameof(directory));

            // Return all files if no patterns
            if (patterns?.Any() != true)
            {
                return directory.GetFiles(SearchOption.AllDirectories);
            }

            IEnumerable<Tuple<IDirectory, string>> directoryPatterns = patterns
                .Where(x => x is object && x.Length > 0)
                .Select(x =>
                {
                    bool negated = x[0] == '!';
                    NormalizedPath path = negated ? new NormalizedPath(x.Substring(1)) : new NormalizedPath(x);
                    if (path.IsAbsolute)
                    {
                        // The globber doesn't support absolute paths, so get the root directory of this path
                        IDirectory rootDirectory = fileSystem.GetDirectory(path.Root);
                        NormalizedPath relativePath = path.RootRelative;
                        return Tuple.Create(
                            rootDirectory,
                            negated ? ('!' + relativePath.FullPath) : relativePath.FullPath);
                    }
                    return Tuple.Create(directory, x);
                });
            IEnumerable<IGrouping<IDirectory, string>> patternGroups = directoryPatterns
                .GroupBy(x => x.Item1, x => x.Item2, DirectoryEqualityComparer.Default);

            // We need to run each pattern in isolation so that exclusions aren't global to all patterns
            // This also means we might get duplicate results, so filter those out by path
            return patternGroups.SelectMany(x => x.SelectMany(y => Globber.GetFiles(x.Key, y))).Distinct(new FilePathEqualityComparer());
        }

        private class FilePathEqualityComparer : IEqualityComparer<IFile>
        {
            public bool Equals([AllowNull] IFile x, [AllowNull] IFile y) => x?.Path.Equals(y?.Path) == true;
            public int GetHashCode([DisallowNull] IFile obj) => obj.Path.GetHashCode();
        }
    }
}
