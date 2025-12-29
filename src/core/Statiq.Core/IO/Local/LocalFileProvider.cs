using System;
using Polly;
using Statiq.Common;

namespace Statiq.Core
{
    public class LocalFileProvider : IFileProvider
    {
        public LocalFileProvider(IReadOnlyFileSystem fileSystem)
        {
            FileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
        }

        public IReadOnlyFileSystem FileSystem { get; }

        internal static Policy RetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetry(5, retry => retry * TimeSpan.FromMilliseconds(100));

        internal static AsyncPolicy AsyncRetryPolicy { get; } =
            Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(5, retry => retry * TimeSpan.FromMilliseconds(100));

        /// <inheritdoc/>
        public IFile GetFile(NormalizedPath path) => new LocalFile(this, path);

        /// <inheritdoc/>
        public IDirectory GetDirectory(NormalizedPath path) => new LocalDirectory(this, path);
    }
}