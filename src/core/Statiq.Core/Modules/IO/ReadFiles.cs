using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Reads the content of files from the file system into the content of new documents.
    /// </summary>
    /// <remarks>
    /// This module will be executed once and input documents will be ignored if search patterns are specified. Otherwise, if a delegate
    /// is specified, the module will be executed once per input document and the resulting output documents will be
    /// aggregated. In either case, the input documents will not be returned as output of this module.
    /// <see cref="IDocument.Source"/> will be set to the absolute path of the file
    /// (use <see cref="NormalizedPath.GetRelativeInputPath()"/> to get a source path relative to the input folders).
    /// <see cref="IDocument.Destination"/> will be set to the relative path of the file (so that <see cref="WriteFiles"/> will write it
    /// to the same relative path in the output folder).
    /// </remarks>
    /// <category name="Input/Output" />
    public class ReadFiles : ParallelConfigModule<IEnumerable<string>>
    {
        private Func<IFile, Task<bool>> _predicate = null;
        private Func<IFile, string> _mediaType = null;

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths. This allows you to
        /// specify different patterns and/or paths depending on the input.
        /// </summary>
        /// <param name="patterns">
        /// A delegate that returns one or more globbing patterns and/or absolute paths.
        /// If the delegate returns a null or empty collection, all files will be read.
        /// If the delegate returns a single pattern as null or an empty string pattern, no files will be read.
        /// </param>
        public ReadFiles(Config<IEnumerable<string>> patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Reads all files that match the specified globbing pattern and/or absolute path. This allows you to
        /// specify different patterns and/or paths depending on the input.
        /// </summary>
        /// <param name="pattern">
        /// A delegate that returns a globbing patterns and/or absolute paths.
        /// If the delegate returns null or an empty string pattern, no files will be read.
        /// </param>
        public ReadFiles(Config<string> pattern)
            : base(pattern.ThrowIfNull(nameof(pattern)).MakeEnumerable(), false)
        {
        }

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths.
        /// </summary>
        /// <param name="patterns">
        /// The globbing patterns and/or absolute paths to read.
        /// If a null or empty array is specified, all files will be read.
        /// If the array contains a single pattern as null or an empty string pattern, no files will be read.
        /// </param>
        public ReadFiles(params string[] patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Reads all files that match the specified globbing patterns and/or absolute paths.
        /// </summary>
        /// <param name="patterns">
        /// The globbing patterns and/or absolute paths to read.
        /// If a null or empty collection is specified, all files will be read.
        /// If the collection contains a single pattern as null or an empty string pattern, no files will be read.
        /// </param>
        public ReadFiles(IEnumerable<string> patterns)
            : base(Config.FromValue(patterns), false)
        {
        }

        /// <summary>
        /// Specifies a predicate that must be satisfied for the file to be read.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the file should be read.</param>
        /// <returns>The current module instance.</returns>
        public ReadFiles Where(Func<IFile, Task<bool>> predicate)
        {
            Func<IFile, Task<bool>> currentPredicate = _predicate;
            _predicate = currentPredicate is null ? predicate : async x => await currentPredicate(x) && await predicate(x);
            return this;
        }

        /// <summary>
        /// Specifies a function to set the media type for each file.
        /// </summary>
        /// <param name="mediaType">A function that determines the media type for each file.</param>
        /// <returns>The current module instance.</returns>
        public ReadFiles WithMediaType(Func<IFile, string> mediaType)
        {
            _mediaType = mediaType;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IEnumerable<string> value)
        {
            IEnumerable<IFile> files = context.FileSystem.GetInputFiles(value);
            files = await files.ParallelWhereAsync(async file => _predicate is null || await _predicate(file));
            return files.AsParallel()
                .Select(file =>
                {
                    context.LogDebug($"Read file {file.Path.FullPath}");
                    IContentProvider contentProvider = _mediaType is null
                        ? file?.GetContentProvider()
                        : file?.GetContentProvider(_mediaType(file));
                    return context.CloneOrCreateDocument(input, file.Path, file.Path.GetRelativeInputPath(), contentProvider);
                })
                .OrderBy(x => x.Source); // Use a deterministic output ordering
        }
    }
}