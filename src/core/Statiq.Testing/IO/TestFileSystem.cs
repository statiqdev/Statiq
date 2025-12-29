using System.Collections.Concurrent;
using System.Collections.Generic;
using Statiq.Common;

namespace Statiq.Testing
{
    /// <summary>
    /// A file system for testing that uses a single file provider.
    /// </summary>
    public class TestFileSystem : IFileSystem
    {
        private IFileProvider _fileProvider;

        public TestFileSystem()
            : this(null)
        {
        }

        public TestFileSystem(IFileProvider fileProvider)
        {
            FileProvider = fileProvider ?? new TestFileProvider(this, "input");
        }

        public IFileProvider FileProvider
        {
            get => _fileProvider;
            set
            {
                if (value is TestFileProvider testFileProvider)
                {
                    // Attach the file provider to this file system
                    testFileProvider.FileSystem = this;
                }
                _fileProvider = new ExcludedFileProvider(this, value);
            }
        }

        /// <inheritdoc />
        public NormalizedPath RootPath { get; set; } = NormalizedPath.AbsoluteRoot;

        /// <inheritdoc />
        public PathCollection InputPaths { get; set; } = new PathCollection(new NormalizedPath("input"));

        IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        public IDictionary<NormalizedPath, NormalizedPath> InputPathMappings { get; set; } = new ConcurrentDictionary<NormalizedPath, NormalizedPath>();

        IReadOnlyDictionary<NormalizedPath, NormalizedPath> IReadOnlyFileSystem.InputPathMappings => (IReadOnlyDictionary<NormalizedPath, NormalizedPath>)InputPathMappings;

        /// <inheritdoc />
        public PathCollection ExcludedPaths { get; set; } = new PathCollection();

        IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.ExcludedPaths => ExcludedPaths;

        /// <inheritdoc />
        public NormalizedPath OutputPath { get; set; } = "output";

        /// <inheritdoc />
        public NormalizedPath TempPath { get; set; } = "temp";

        /// <inheritdoc />
        public NormalizedPath CachePath { get; set; } = "cache";

        /// <inheritdoc />
        public IFileWriteTracker WriteTracker { get; } = new TestFileWriteTracker();
    }
}
