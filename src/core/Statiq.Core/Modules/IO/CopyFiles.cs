using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Copies the content of files from one path on to another path.
    /// </summary>
    /// <remarks>
    /// By default, files are copied from the input folder (or a subfolder) to the same relative
    /// location in the output folder, but this doesn't have to be the case. The output of this module are documents
    /// representing the files copied by the module. Note that the input documents are not output by this module.
    /// </remarks>
    /// <category name="Input/Output" />
    public class CopyFiles : ParallelConfigModule<IEnumerable<string>>
    {
        private Func<IFile, IFile, Task<NormalizedPath>> _destinationPath;
        private Func<IFile, Task<bool>> _predicate = null;

        /// <summary>
        /// Copies all files that match the specified globbing patterns and/or absolute paths. This allows you to specify different
        /// patterns and/or paths depending on the input document.
        /// When this constructor is used, the module is evaluated once for every input document, which may result in copying the same file
        /// more than once (and may also result in IO conflicts since copying is typically done in parallel). It is recommended you only
        /// specify a function-based source path if there will be no overlap between the path returned from each input document.
        /// </summary>
        /// <param name="patterns">A delegate that returns one or more globbing patterns and/or absolute paths.</param>
        public CopyFiles(Config<IEnumerable<string>> patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Copies all files that match the specified globbing patterns and/or absolute paths. When this constructor is used, the module is
        /// evaluated only once against empty input document. This makes it possible to string multiple CopyFiles modules together in one pipeline.
        /// Keep in mind that the result of the whole pipeline in this case will be documents representing the files copied only by the last CopyFiles
        /// module in the pipeline (since the output documents of the previous CopyFiles modules will have been consumed by the last one).
        /// </summary>
        /// <param name="patterns">The globbing patterns and/or absolute paths to read.</param>
        public CopyFiles(params string[] patterns)
            : base((Config<IEnumerable<string>>)patterns.ThrowIfNull(nameof(patterns)), false)
        {
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be copied.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be copied.</param>
        /// <returns>The current module instance.</returns>
        public CopyFiles Where(Func<IFile, Task<bool>> predicate)
        {
            Func<IFile, Task<bool>> currentPredicate = _predicate;
            _predicate = currentPredicate is null ? predicate : async x => await currentPredicate(x) && await predicate(x);
            return this;
        }

        /// <summary>
        /// Specifies an alternate destination path for each file (by default files are copied to their
        /// same relative path in the output directory). The output of the function should be the full
        /// file path (including file name) of the destination file. If the delegate returns
        /// <c>null</c> for a particular file, that file will not be copied.
        /// </summary>
        /// <param name="destinationPath">A delegate that specifies an alternate destination.
        /// The parameter contains the source <see cref="IFile"/>.</param>
        /// <returns>The current module instance.</returns>
        public CopyFiles To(Func<IFile, Task<NormalizedPath>> destinationPath)
        {
            destinationPath.ThrowIfNull(nameof(destinationPath));
            _destinationPath = async (source, _) => await destinationPath(source);
            return this;
        }

        /// <summary>
        /// Specifies an alternate destination path for each file (by default files are copied to their
        /// same relative path in the output directory). The output of the function should be the full
        /// file path (including file name) of the destination file. If the delegate returns
        /// <c>null</c> for a particular file, that file will not be copied. This overload allows you to
        /// view the <see cref="IFile"/> where the module would normally have copied the file to and then
        /// manipulate it (or not) as appropriate.
        /// </summary>
        /// <param name="destinationPath">A delegate that specifies an alternate destination.
        /// The first parameter contains the source <see cref="IFile"/> and the second contains
        /// an <see cref="IFile"/> representing the calculated destination.</param>
        /// <returns>The current module instance.</returns>
        public CopyFiles To(Func<IFile, IFile, Task<NormalizedPath>> destinationPath)
        {
            _destinationPath = destinationPath.ThrowIfNull(nameof(destinationPath));
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IEnumerable<string> value)
        {
            if (value is object)
            {
                // Use a semaphore to limit the write operations so we don't try to do a bunch of writes at once
                SemaphoreSlim semaphore = new SemaphoreSlim(20, 20);

                // Create copy tasks
                IEnumerable<IFile> inputFiles = context.FileSystem.GetInputFiles(value);
                List<Task<IDocument>> copyTasks = new List<Task<IDocument>>();
                foreach (IFile inputFile in inputFiles)
                {
                    if (_predicate is null || await _predicate(inputFile))
                    {
                        copyTasks.Add(CopyFileAsync(input, inputFile, context, semaphore));
                    }
                }

                // Run all the tasks
                IDocument[] outputs = await Task.WhenAll(copyTasks);
                return outputs.Where(x => x is object);
            }
            return null;
        }

        private async Task<IDocument> CopyFileAsync(IDocument input, IFile file, IExecutionContext context, SemaphoreSlim semaphore)
        {
            await semaphore.WaitAsync();
            try
            {
                // Get the default destination file
                NormalizedPath relativePath = file.Path.GetRelativeInputPath();
                IFile destinationFile = context.FileSystem.GetOutputFile(relativePath);

                // Calculate an alternate destination if needed
                if (_destinationPath is object)
                {
                    destinationFile = context.FileSystem.GetOutputFile(await _destinationPath(file, destinationFile));
                }

                // Did we copy this file last time and has no one messed with it? (I.e. check the destination file)
                // Then check if the content we're about to write is the same as last time (I.e. check the source file)
                int destinationFileHash = await destinationFile.GetCacheCodeAsync();
                int sourceFileHash = await file.GetCacheCodeAsync(); // Get a source file hash that will represent the "content" (since we don't want to read the entire file to get a hash)
                if (context.FileSystem.WriteTracker.TryGetPreviousWrite(destinationFile.Path, out int previousWriteHash)
                    && previousWriteHash == destinationFileHash
                    && context.FileSystem.WriteTracker.TryGetPreviousContent(destinationFile.Path, out int previousContentHash)
                    && previousContentHash == sourceFileHash)
                {
                    // We wrote this file last time, it still exists, and it hasn't changed
                    // Make sure to add the appropriate entries so it looks like we wrote it this time
                    context.LogDebug($"Skipped copying file {destinationFile.Path.FullPath} from {relativePath} because it already exists and the content is the same");
                    context.FileSystem.WriteTracker.TrackWrite(destinationFile.Path, previousWriteHash, false);
                    context.FileSystem.WriteTracker.TrackContent(destinationFile.Path, previousContentHash);
                }
                else
                {
                    // Copy to the destination
                    await file.CopyToAsync(destinationFile, cancellationToken: context.CancellationToken);
                    context.FileSystem.WriteTracker.TrackContent(destinationFile.Path, sourceFileHash);
                    context.LogDebug("Copied file {0} to {1}", file.Path.FullPath, destinationFile.Path.FullPath);

                    // Don't need to set write hash since it'll be tracked when the file write actually occurs
                    // unless the length is 0 in which case no write will have been recorded so we should manually add it
                    destinationFile.Refresh();
                    if (destinationFile.Length == 0)
                    {
                        destinationFileHash = await destinationFile.GetCacheCodeAsync(); // Get a new output file hash that includes the newly written file metadata since we just touched it
                        context.FileSystem.WriteTracker.TrackWrite(destinationFile.Path, destinationFileHash, true);
                    }
                }

                // Return the document
                return context.CloneOrCreateDocument(input, file.Path, relativePath, file?.GetContentProvider());
            }
            catch (Exception ex)
            {
                context.LogError($"Error while copying file {file.Path.FullPath}: {ex.Message}");
                throw;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}