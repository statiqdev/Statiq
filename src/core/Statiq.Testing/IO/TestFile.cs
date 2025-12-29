using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestFile : IFile
    {
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly TestFileProvider _fileProvider;

        public TestFile(IReadOnlyFileSystem fileSystem, TestFileProvider fileProvider, in NormalizedPath path)
        {
            _fileSystem = fileSystem;
            _fileProvider = fileProvider;
            Path = path;
        }

        /// <inheritdoc/>
        public NormalizedPath Path { get; }

        /// <inheritdoc/>
        NormalizedPath IFileSystemEntry.Path => Path;

        /// <inheritdoc/>
        public bool Exists => _fileProvider.Files.ContainsKey(Path);

        /// <inheritdoc/>
        public IDirectory Directory => _fileSystem.GetDirectory(Path.Parent);

        /// <inheritdoc/>
        public long Length => _fileProvider.Files[Path].Length;

        /// <inheritdoc/>
        public string MediaType => Path.MediaType;

        /// <inheritdoc/>
        public DateTime LastWriteTime { get; set; }

        /// <inheritdoc/>
        public DateTime CreationTime { get; set; }

        /// <inheritdoc/>
        private void CreateDirectory(bool createDirectory, IFile file)
        {
            if (!createDirectory && !_fileProvider.Directories.Contains(file.Path.Parent))
            {
                throw new IOException($"Directory {file.Path.Parent} does not exist");
            }
            if (createDirectory)
            {
                NormalizedPath parent = file.Path.Parent;
                while (!parent.IsNullOrEmpty)
                {
                    _fileProvider.Directories.Add(parent);
                    parent = parent.Parent;
                }
            }
        }

        /// <inheritdoc/>
        public void Delete() => _fileProvider.Files.TryRemove(Path, out StringBuilder _);

        /// <inheritdoc/>
        public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) => Task.FromResult(_fileProvider.Files[Path].ToString());

        /// <inheritdoc/>
        public Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default) => Task.FromResult(Encoding.UTF8.GetBytes(_fileProvider.Files[Path].ToString()));

        /// <inheritdoc/>
        public Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            CreateDirectory(createDirectory, this);
            _fileProvider.Files[Path] = new StringBuilder(contents);
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task WriteAllBytesAsync(byte[] bytes, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            CreateDirectory(createDirectory, this);
            _fileProvider.Files[Path] = new StringBuilder(Encoding.UTF8.GetString(bytes));
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Stream OpenRead()
        {
            if (!_fileProvider.Files.TryGetValue(Path, out StringBuilder builder))
            {
                throw new FileNotFoundException();
            }
            byte[] bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return new MemoryStream(bytes);
        }

        /// <inheritdoc/>
        public TextReader OpenText()
        {
            if (!_fileProvider.Files.TryGetValue(Path, out StringBuilder builder))
            {
                throw new FileNotFoundException();
            }
            return new StringReader(builder.ToString());
        }

        /// <inheritdoc/>
        public Stream OpenWrite(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path, new StringBuilder(), (x, y) => y));
        }

        /// <inheritdoc/>
        public Stream OpenAppend(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            StringBuilderStream stream = new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path, new StringBuilder(), (x, y) => y));

            // Start appending at the end of the stream.
            stream.Position = stream.Length;
            return stream;
        }

        /// <inheritdoc/>
        public Stream Open(bool createDirectory = true)
        {
            CreateDirectory(createDirectory, this);
            return new StringBuilderStream(_fileProvider.Files.AddOrUpdate(Path, new StringBuilder(), (x, y) => y));
        }

        /// <inheritdoc/>
        public IContentProvider GetContentProvider() => GetContentProvider(MediaType);

        /// <inheritdoc/>
        public IContentProvider GetContentProvider(string mediaType) =>
            _fileProvider.Files.ContainsKey(Path) ? (IContentProvider)new FileContent(this, mediaType) : new NullContent(mediaType);

        public override string ToString() => Path.ToString();

        /// <inheritdoc/>
        public string ToDisplayString() => Path.ToSafeDisplayString();

        /// <inheritdoc/>
        public Task<int> GetCacheCodeAsync()
        {
            CacheCode cacheCode = new CacheCode();
            cacheCode.Add(Path.FullPath);
            if (Exists)
            {
                cacheCode.Add(Length);
                cacheCode.Add(CreationTime);
                cacheCode.Add(LastWriteTime);
            }
            else
            {
                cacheCode.Add(-1);
            }
            return Task.FromResult(cacheCode.ToCacheCode());
        }

        /// <inheritdoc/>
        public void Refresh()
        {
        }
    }
}