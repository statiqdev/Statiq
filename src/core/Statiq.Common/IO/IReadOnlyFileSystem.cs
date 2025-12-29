using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a file system.
    /// </summary>
    public interface IReadOnlyFileSystem
    {
        // Initially based on code from Cake (http://cakebuild.net/)

        /// <summary>
        /// Gets the file provider.
        /// </summary>
        /// <value>
        /// The file provider.
        /// </value>
        IFileProvider FileProvider { get; }

        /// <summary>
        /// Gets the root path.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        NormalizedPath RootPath { get; }

        /// <summary>
        /// Gets the input paths. These are searched in reverse order for
        /// files and directories. For example, given input paths "A", "B",
        /// and "C" in that order, "C" will be checked for a requested file
        /// or directory first, and then if it doesn't exist in "C", "B"
        /// will be checked, and then "A". If none of the input paths contain
        /// the requested file or directory, the last input path (in this case,
        /// "C") will be used as the location of the requested non-existent file
        /// or directory. If you attempt to create it at this point, it will be
        /// created under path "C".
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        IReadOnlyList<NormalizedPath> InputPaths { get; }

        /// <summary>
        /// Maps input paths to their location in the virtual file system.
        /// The key should be a <see cref="NormalizedPath"/> that exists in
        /// <see cref="InputPaths"/>. The value should be a relative path
        /// where the input path should be mapped within the virtual folder heirarchy.
        /// </summary>
        IReadOnlyDictionary<NormalizedPath, NormalizedPath> InputPathMappings { get; }

        /// <summary>
        /// Gets the excluded paths collection which can be used
        /// to excluded specific paths from the input paths. Any
        /// <see cref="IDirectory"/> or <see cref="IFile"/> within
        /// an excluded path will appear to be non-existing.
        /// </summary>
        /// <value>
        /// The excluded paths.
        /// </value>
        IReadOnlyList<NormalizedPath> ExcludedPaths { get; }

        /// <summary>
        /// Gets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        NormalizedPath OutputPath { get; }

        /// <summary>
        /// Gets the temporary file path.
        /// </summary>
        /// <value>
        /// The temporary file path.
        /// </value>
        NormalizedPath TempPath { get; }

        /// <summary>
        /// Gets the cache file path.
        /// </summary>
        /// <value>
        /// The cache file path.
        /// </value>
        NormalizedPath CachePath { get; }

        /// <summary>
        /// Tracks the state of files being written to and their source content.
        /// This helps determine when a file should be overwritten when using <see cref="CleanMode.Unwritten"/>.
        /// </summary>
        IFileWriteTracker WriteTracker { get; }
    }
}
