using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A file that is excluded from the file system due to
    /// <see cref="IFileSystem.ExcludedPaths"/> (I.e. the file
    /// is non-existing).
    /// </summary>
    internal class ExcludedFile : IFile
    {
        private readonly IFileProvider _fileProvider;

        public ExcludedFile(IFileProvider fileProvider, in NormalizedPath path)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));
            Path = path;
        }

        public NormalizedPath Path { get; }

        NormalizedPath IFileSystemEntry.Path => Path;

        public IDirectory Directory => _fileProvider.GetDirectory(Path.Parent);

        public bool Exists => false;

        public string MediaType => Path.MediaType;

        public IContentProvider GetContentProvider() => GetContentProvider(MediaType);

        public IContentProvider GetContentProvider(string mediaType) => new NullContent(mediaType);

        public override string ToString() => Path.ToString();

        public string ToDisplayString() => Path.ToDisplayString();

        public long Length =>
            throw new NotSupportedException("Not supported for an excluded path");

        public DateTime LastWriteTime =>
            throw new NotSupportedException("Not supported for an excluded path");

        public DateTime CreationTime =>
            throw new NotSupportedException("Not supported for an excluded path");

        public void Delete() =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Task<string> ReadAllTextAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Task WriteAllTextAsync(string contents, bool createDirectory = true, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Task<byte[]> ReadAllBytesAsync(CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Task WriteAllBytesAsync(byte[] bytes, bool createDirectory = true, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Stream OpenRead() =>
            throw new NotSupportedException("Not supported for an excluded path");

        public TextReader OpenText() =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Stream OpenWrite(bool createDirectory = true) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Stream OpenAppend(bool createDirectory = true) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Stream Open(bool createDirectory = true) =>
            throw new NotSupportedException("Not supported for an excluded path");

        public Task<int> GetCacheCodeAsync() =>
            throw new NotSupportedException("Not supported for an excluded path");

        public void Refresh()
        {
        }
    }
}
