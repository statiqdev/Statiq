using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Writes the content of each input document to the file system.
    /// </summary>
    /// <remarks>
    /// Writes files to the location specified by <see cref="IDocument.Destination"/>.
    /// If the destination path is relative, the document will be written to the output
    /// folder at the relative location. If the destination path is absolute, the document
    /// will be written to the absolute location. Use the <see cref="SetDestination"/> module
    /// to set the document destination prior to using this module.
    /// </remarks>
    /// <category name="Input/Output" />
    public class WriteFiles : Module
    {
        private bool _ignoreEmptyContent = true;
        private bool _append;
        private Config<bool> _predicate = true;

        /// <summary>
        /// Ignores documents with empty content, which is the default behavior.
        /// </summary>
        /// <param name="ignoreEmptyContent">If set to <c>true</c>, documents with empty content will be ignored.</param>
        /// <returns>The current module instance.</returns>
        public WriteFiles IgnoreEmptyContent(bool ignoreEmptyContent = true)
        {
            _ignoreEmptyContent = ignoreEmptyContent;
            return this;
        }

        /// <summary>
        /// Appends content to each file instead of overwriting them.
        /// </summary>
        /// <param name="append">Appends to existing files if set to <c>true</c>.</param>
        /// <returns>The current module instance.</returns>
        public WriteFiles Append(bool append = true)
        {
            _append = append;
            return this;
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be written.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be written.</param>
        /// <returns>The current module instance.</returns>
        public WriteFiles Where(Config<bool> predicate)
        {
            _predicate = _predicate.CombineWith(predicate);
            return this;
        }

        /// <summary>
        /// Checks whether the input document should be processed.
        /// </summary>
        /// <param name="input">The input document to check.</param>
        /// <param name="context">The execution context.</param>
        /// <returns><c>true</c> if the input document should be processed, <c>false</c> otherwise.</returns>
        protected Task<bool> ShouldProcessAsync(IDocument input, IExecutionContext context) => _predicate.GetValueAsync(input, context);

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Use a semaphore to limit the write operations so we don't try to do a bunch of writes at once
            SemaphoreSlim semaphore = new SemaphoreSlim(20, 20);

            // Get the output file path for each file in sequence and set up action chains
            // Value = input source string(s) (for reporting a warning if not appending), write action
            Dictionary<NormalizedPath, Tuple<List<string>, Func<Task>>> writesBySource = new Dictionary<NormalizedPath, Tuple<List<string>, Func<Task>>>();
            foreach (IDocument input in context.Inputs)
            {
                await AddWriteFileTaskAsync(input, context, writesBySource, semaphore);
            }

            // Display a warning for any duplicated outputs if not appending
            if (!_append)
            {
                foreach (KeyValuePair<NormalizedPath, Tuple<List<string>, Func<Task>>> kvp in writesBySource.Where(x => x.Value.Item1.Count > 1))
                {
                    string inputSources = Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", kvp.Value.Item1);
                    context.LogWarning($"Multiple documents output to {kvp.Key} (this probably wasn't intended):{inputSources}");
                }
            }

            // Run the write actions
            await Task.WhenAll(writesBySource.Values.Select(x => x.Item2()));

            // Return the input documents
            return context.Inputs;
        }

        private async Task AddWriteFileTaskAsync(
            IDocument input,
            IExecutionContext context,
            Dictionary<NormalizedPath, Tuple<List<string>, Func<Task>>> writesBySource,
            SemaphoreSlim semaphore)
        {
            if (await ShouldProcessAsync(input, context) && !input.Destination.IsNull)
            {
                if (writesBySource.TryGetValue(input.Destination, out Tuple<List<string>, Func<Task>> value))
                {
                    // This output source was already seen so nest the previous write action in a new one
                    value.Item1.Add(input.Source.ToSafeDisplayString());
                    Func<Task> previousWrite = value.Item2;
                    value = new Tuple<List<string>, Func<Task>>(
                        value.Item1,
                        async () =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                // Complete the previous write, then do the next one
                                await previousWrite();
                                await WriteAsync(input, context, input.Destination);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });
                }
                else
                {
                    value = new Tuple<List<string>, Func<Task>>(
                        new List<string> { input.Source.ToSafeDisplayString() },
                        async () =>
                        {
                            await semaphore.WaitAsync();
                            try
                            {
                                await WriteAsync(input, context, input.Destination);
                            }
                            finally
                            {
                                semaphore.Release();
                            }
                        });
                }
                writesBySource[input.Destination] = value;
            }
        }

        private async Task WriteAsync(IDocument input, IExecutionContext context, NormalizedPath outputPath)
        {
            IFile outputFile = context.FileSystem.GetOutputFile(outputPath);
            if (outputFile is object)
            {
                // Did we write this file last time and has no one messed with it?
                int contentHash = await input.ContentProvider.GetCacheCodeAsync();
                if (context.FileSystem.WriteTracker.TryGetPreviousWrite(outputFile.Path, out int previousWriteHash)
                    && previousWriteHash == await outputFile.GetCacheCodeAsync())
                {
                    // We wrote this file last time, it still exists, and it hasn't changed
                    // Now check if the content we're about to write is the same as last time
                    if (context.FileSystem.WriteTracker.TryGetPreviousContent(outputFile.Path, out int previousContentHash)
                        && previousContentHash == contentHash)
                    {
                        // We used the same content last time, so we can skip writing this file
                        // Make sure to add the appropriate entries so it looks like we wrote it this time
                        context.LogDebug($"Skipped writing file {outputFile.Path.FullPath} from {input.Source.ToSafeDisplayString()} because it already exists and the content is the same");
                        context.FileSystem.WriteTracker.TrackWrite(outputFile.Path, previousWriteHash, false);
                        context.FileSystem.WriteTracker.TrackContent(outputFile.Path, previousContentHash);
                        return;
                    }
                }

                // Optimization if the input document is straight from a file
                if (input.ContentProvider is FileContent fileContent)
                {
                    await fileContent.File.CopyToAsync(outputFile, !_append, cancellationToken: context.CancellationToken);
                }
                else
                {
                    // Otherwise open an output stream for the output file and copy the input document stream to it
                    using (Stream inputStream = input.GetContentStream())
                    {
                        // Peek the stream to see if it contains any content
                        byte[] firstByte = new byte[1];
                        if (_ignoreEmptyContent && await inputStream.ReadAsync(firstByte, context.CancellationToken) == 0)
                        {
                            return;
                        }

                        using (Stream outputStream = _append ? outputFile.OpenAppend() : outputFile.OpenWrite())
                        {
                            long initialPosition = outputStream.Position;
                            await outputStream.WriteAsync(firstByte, context.CancellationToken);
                            await inputStream.CopyToAsync(outputStream, context.CancellationToken);
                            long length = outputStream.Position - initialPosition;
                            if (!_append)
                            {
                                outputStream.SetLength(length);
                            }
                        }
                    }
                }
                context.FileSystem.WriteTracker.TrackContent(outputFile.Path, contentHash); // The file write hash will be tracked when the file write actually occurs
                context.LogDebug($"Wrote file {outputFile.Path.FullPath} from {input.Source.ToSafeDisplayString()}");
            }
        }
    }
}