using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A file system that can be configured.
    /// </summary>
    public interface IFileSystem : IReadOnlyFileSystem
    {
        /// <summary>
        /// Gets the file provider.
        /// </summary>
        /// <value>
        /// The file provider.
        /// </value>
        new IFileProvider FileProvider { get; set; }

        /// <summary>
        /// Gets or sets the root path.
        /// </summary>
        /// <value>
        /// The root path.
        /// </value>
        new NormalizedPath RootPath { get; set; }

        /// <summary>
        /// Gets the input paths collection which can be used
        /// to add or remove input paths.
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        new PathCollection InputPaths { get; }

        /// <summary>
        /// Gets the input path mapping dictionary which can be used
        /// to add or remove input path mappings.
        /// </summary>
        new IDictionary<NormalizedPath, NormalizedPath> InputPathMappings { get; }

        /// <summary>
        /// Gets the excluded paths collection which can be used
        /// to excluded specific paths from the input paths. Any
        /// <see cref="IDirectory"/> or <see cref="IFile"/> within
        /// an excluded path will appear to be non-existing.
        /// </summary>
        /// <value>
        /// The input paths.
        /// </value>
        new PathCollection ExcludedPaths { get; }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        /// <value>
        /// The output path.
        /// </value>
        new NormalizedPath OutputPath { get; set; }

        /// <summary>
        /// Gets or sets the temporary file path.
        /// </summary>
        new NormalizedPath TempPath { get; set; }

        /// <summary>
        /// Gets or sets the cache file path.
        /// </summary>
        new NormalizedPath CachePath { get; set; }
    }
}
