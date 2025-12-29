using System;
using System.Collections.Generic;
using System.IO;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    public class FileSystem : IFileSystem
    {
        private NormalizedPath _rootPath = Directory.GetCurrentDirectory();
        private NormalizedPath _outputPath = "output";
        private NormalizedPath _tempPath = "temp";
        private NormalizedPath _cachePath = "cache";
        private IFileProvider _fileProvider;

        public FileSystem()
        {
            FileProvider = new LocalFileProvider(this);
            WriteTracker = new FileWriteTracker(this);
        }

        public IFileProvider FileProvider
        {
            get => _fileProvider;
            set => _fileProvider = new ExcludedFileProvider(this, value);
        }

        public NormalizedPath RootPath
        {
            get => _rootPath;

            set
            {
                value.ThrowIfNull(nameof(value));

                if (value.IsRelative)
                {
                    throw new ArgumentException("The root path must not be relative");
                }

                _rootPath = value;
            }
        }

        public PathCollection InputPaths { get; } = new PathCollection("input");

        IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.InputPaths => InputPaths;

        public IDictionary<NormalizedPath, NormalizedPath> InputPathMappings { get; set; } = new InputPathMappingDictionary();

        IReadOnlyDictionary<NormalizedPath, NormalizedPath> IReadOnlyFileSystem.InputPathMappings => (IReadOnlyDictionary<NormalizedPath, NormalizedPath>)InputPathMappings;

        public PathCollection ExcludedPaths { get; } = new PathCollection();

        IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.ExcludedPaths => ExcludedPaths;

        public NormalizedPath OutputPath
        {
            get => _outputPath;
            set
            {
                value.ThrowIfNull(nameof(value));
                _outputPath = value;
            }
        }

        public NormalizedPath TempPath
        {
            get => _tempPath;
            set
            {
                value.ThrowIfNull(nameof(value));
                _tempPath = value;
            }
        }

        public NormalizedPath CachePath
        {
            get => _cachePath;
            set
            {
                value.ThrowIfNull(nameof(value));
                _cachePath = value;
            }
        }

        public IFileWriteTracker WriteTracker { get; }
    }
}