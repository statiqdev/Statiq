using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    // Initially based on code from Cake (http://cakebuild.net/)
    internal class LocalFile : IFile
    {
        private const int BufferSize = 8192;

        private readonly LocalFileProvider _fileProvider;
        private readonly System.IO.FileInfo _file;

        public LocalFile(LocalFileProvider fileProvider, in NormalizedPath path)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));

            path.ThrowIfNull(nameof(path));
            path.ThrowIfRelative(nameof(path));

            Path = path;
            _file = new System.IO.FileInfo(Path.FullPath);
        }

        /// <inheritdoc/>
        public NormalizedPath Path { get; }

        /// <inheritdoc/>
        NormalizedPath IFileSystemEntry.Path => Path;

        /// <inheritdoc/>
        public IDirectory Directory => _fileProvider.FileSystem.GetDirectory(Path.Parent);

        /// <inheritdoc/>
        public bool Exists => _file.Exists;

        /// <inheritdoc/>
        public long Length => _file.Length;

        /// <inheritdoc/>
        public string MediaType => Path.MediaType;

        /// <inheritdoc/>
        public DateTime LastWriteTime => _file.LastWriteTime;

        /// <inheritdoc/>
        public DateTime CreationTime => _file.CreationTime;

        /// <inheritdoc/>
        public void Delete() => LocalFileProvider.RetryPolicy.Execute(() =>
        {
            _file.Delete();
            Refresh();
        });

        /// <inheritdoc/>
        public async Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) =>
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.ReadAllTextAsync(_file.FullName, cancellationToken));

        /// <inheritdoc/>
        public async Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            Refresh();
#pragma warning disable VSTHRD103 // GetCacheCode synchronously blocks. Await GetCacheCodeAsync instead.
            _fileProvider.FileSystem.WriteTracker.TrackWrite(Path, GetCacheCode(), true);
#pragma warning restore VSTHRD103
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.WriteAllTextAsync(_file.FullName, contents, cancellationToken));
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default) =>
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.ReadAllBytesAsync(_file.FullName, cancellationToken));

        /// <inheritdoc/>
        public async Task WriteAllBytesAsync(byte[] bytes, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }
            Refresh();
#pragma warning disable VSTHRD103 // GetCacheCode synchronously blocks. Await GetCacheCodeAsync instead.
            _fileProvider.FileSystem.WriteTracker.TrackWrite(Path, GetCacheCode(), true);
#pragma warning restore VSTHRD103
            await LocalFileProvider.AsyncRetryPolicy.ExecuteAsync(() => File.WriteAllBytesAsync(_file.FullName, bytes, cancellationToken));
        }

        /// <inheritdoc/>
        // Assumes most file operations are going to be asynchronous and sequential
        public Stream OpenRead() => LocalFileProvider.RetryPolicy.Execute(() =>
            new FileStream(_file.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan));

        /// <inheritdoc/>
        public Stream OpenWrite(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            // Assumes most file operations are going to be asynchronous and sequential
            return LocalFileProvider.RetryPolicy.Execute(() =>
                new WrittenFileStream(
                    new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan),
                    _fileProvider.FileSystem.WriteTracker,
                    this));
        }

        /// <inheritdoc/>
        public Stream OpenAppend(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            // Assumes most file operations are going to be asynchronous and sequential
            return LocalFileProvider.RetryPolicy.Execute(() =>
                new WrittenFileStream(
                    new FileStream(_file.FullName, FileMode.Append, FileAccess.Write, FileShare.None, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan),
                    _fileProvider.FileSystem.WriteTracker,
                    this));
        }

        /// <inheritdoc/>
        public Stream Open(bool createDirectory = true)
        {
            if (createDirectory)
            {
                CreateDirectory();
            }

            // Assumes most file operations are going to be asynchronous and sequential
            // We don't actually know if this is going to be used for writing, so use the WrittenFileStream just in case
            return LocalFileProvider.RetryPolicy.Execute(() =>
                new WrittenFileStream(
                    new FileStream(_file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, BufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan),
                    _fileProvider.FileSystem.WriteTracker,
                    this));
        }

        /// <inheritdoc/>
        public TextReader OpenText() => LocalFileProvider.RetryPolicy.Execute(() => File.OpenText(_file.FullName));

        private void CreateDirectory() => Directory.Create();

        /// <inheritdoc/>
        public IContentProvider GetContentProvider() => GetContentProvider(MediaType);

        /// <inheritdoc/>
        public IContentProvider GetContentProvider(string mediaType) =>
            _file.Exists ? (IContentProvider)new FileContent(this, mediaType) : new NullContent(mediaType);

        public override string ToString() => Path.ToString();

        /// <inheritdoc/>
        public string ToDisplayString() => Path.ToDisplayString();

        /// <inheritdoc/>
        public Task<int> GetCacheCodeAsync() => Task.FromResult(GetCacheCode());

        /// <inheritdoc/>
        public void Refresh() => _file.Refresh();

        // Make sure to call Refresh() before calling this if the state has changed
        internal int GetCacheCode()
        {
            CacheCode cacheCode = new CacheCode();
            cacheCode.Add(_file.FullName);
            if (_file.Exists)
            {
                cacheCode.Add(_file.Length);
                cacheCode.Add(_file.CreationTime);
                cacheCode.Add(_file.LastWriteTime);
            }
            else
            {
                cacheCode.Add(-1);
            }
            return cacheCode.ToCacheCode();
        }
    }
}