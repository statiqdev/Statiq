using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IFileExtensions
    {
        /// <summary>
        /// Copies from a source stream to a file, truncating any remaining data in the destination file.
        /// </summary>
        public static async Task WriteFromAsync(
            this IFile destinationFile,
            Stream sourceStream,
            bool overwrite = true,
            bool createDirectory = true,
            CancellationToken cancellationToken = default)
        {
            destinationFile.ThrowIfNull(nameof(destinationFile));
            sourceStream.ThrowIfNull(nameof(sourceStream));

            if (!destinationFile.Exists || overwrite)
            {
                using (Stream fileStream = destinationFile.OpenWrite(createDirectory))
                {
                    long initialPosition = fileStream.Position;
                    await sourceStream.CopyToAsync(fileStream, cancellationToken);
                    long length = fileStream.Position - initialPosition;
                    fileStream.SetLength(length);
                }
            }
        }

        /// <summary>
        /// Copies from a source stream to a file, appending to the end of the file.
        /// </summary>
        public static async Task AppendFromAsync(
            this IFile destinationFile,
            Stream sourceStream,
            bool createDirectory = true,
            CancellationToken cancellationToken = default)
        {
            destinationFile.ThrowIfNull(nameof(destinationFile));
            sourceStream.ThrowIfNull(nameof(sourceStream));

            using (Stream fileStream = destinationFile.OpenAppend(createDirectory))
            {
                await sourceStream.CopyToAsync(fileStream, cancellationToken);
            }
        }

        /// <summary>
        /// Copies from a file to a destination stream.
        /// </summary>
        public static async Task CopyToAsync(
            this IFile sourceFile,
            Stream destinationStream,
            CancellationToken cancellationToken = default)
        {
            sourceFile.ThrowIfNull(nameof(sourceFile));
            destinationStream.ThrowIfNull(nameof(destinationStream));

            using (Stream sourceStream = sourceFile.OpenRead())
            {
                await sourceStream.CopyToAsync(destinationStream, cancellationToken);
            }
        }

        /// <summary>
        /// Copies the file to the specified destination file, truncating any remaining data in the destination file.
        /// </summary>
        /// <param name="source">The source file.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="overwrite">Will overwrite existing destination file if set to <c>true</c>.</param>
        /// <param name="createDirectory">Will create any needed directories that don't already exist if set to <c>true</c>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public static async Task CopyToAsync(this IFile source, IFile destination, bool overwrite = true, bool createDirectory = true, CancellationToken cancellationToken = default)
        {
            source.ThrowIfNull(nameof(source));
            destination.ThrowIfNull(nameof(destination));

            using (Stream sourceStream = source.OpenRead())
            {
                await destination.WriteFromAsync(sourceStream, overwrite, createDirectory, cancellationToken);
            }
        }

        /// <summary>
        /// Moves the file to the specified destination file, truncating any remaining data in the destination file.
        /// </summary>
        /// <param name="source">The source file.</param>
        /// <param name="destination">The destination file.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        public static async Task MoveToAsync(this IFile source, IFile destination, CancellationToken cancellationToken = default)
        {
            source.ThrowIfNull(nameof(source));
            destination.ThrowIfNull(nameof(destination));

            using (Stream sourceStream = source.OpenRead())
            {
                await destination.WriteFromAsync(sourceStream, true, true, cancellationToken);
            }
            source.Delete();
        }

        public static async ValueTask<TValue> DeserializeJsonAsync<TValue>(
            this IFile file,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenRead())
            {
                return await JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken);
            }
        }

        public static async ValueTask<object> DeserializeJsonAsync(
            this IFile file,
            Type returnType,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenRead())
            {
                return await JsonSerializer.DeserializeAsync(stream, returnType, options, cancellationToken);
            }
        }

        public static async Task SerializeJsonAsync<TValue>(
            this IFile file,
            TValue value,
            bool createDirectory,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenWrite(createDirectory))
            {
                long initialPosition = stream.Position;
                await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);
                long length = stream.Position - initialPosition;
                stream.SetLength(length);
            }
        }

        public static async Task SerializeJsonAsync<TValue>(
            this IFile file,
            TValue value,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default) =>
            await file.SerializeJsonAsync(value, true, options, cancellationToken);

        public static async Task SerializeJsonAsync(
            this IFile file,
            object value,
            Type inputType,
            bool createDirectory,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default)
        {
            file.ThrowIfNull(nameof(file));
            using (Stream stream = file.OpenWrite(createDirectory))
            {
                long initialPosition = stream.Position;
                await JsonSerializer.SerializeAsync(stream, value, inputType, options, cancellationToken);
                long length = stream.Position - initialPosition;
                stream.SetLength(length);
            }
        }

        public static async Task SerializeJsonAsync(
            this IFile file,
            object value,
            Type inputType,
            JsonSerializerOptions options = null,
            CancellationToken cancellationToken = default) =>
            await file.SerializeJsonAsync(value, inputType, true, options, cancellationToken);
    }
}
