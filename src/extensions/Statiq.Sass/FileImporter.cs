using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Sass
{
    internal class FileImporter
    {
        // Maps each parent path to the containing path for use in nested imports
        // since the parent path may be relative in those cases
        private readonly ConcurrentDictionary<NormalizedPath, NormalizedPath> _parentAbsolutePaths
            = new ConcurrentDictionary<NormalizedPath, NormalizedPath>();

        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly Func<string, string> _importPathFunc;

        public FileImporter(IReadOnlyFileSystem fileSystem, Func<string, string> importPathFunc)
        {
            _fileSystem = fileSystem;
            _importPathFunc = importPathFunc;
        }

        // This is a TryImportDelegate which unfortunately isn't async
        public bool TryImport(ref string requestedFile, string parentPath, out string scss, out string map)
        {
#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks. Use await or JoinableTaskFactory.Run instead.
            scss = TryImportAsync(requestedFile, parentPath).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
            map = null;
            return scss is object;
        }

        public async Task<string> TryImportAsync(string requestedFile, string parentPath)
        {
            // Modify the requested file if we have an import path function
            string modifiedParentPath = null;
            if (_importPathFunc is object)
            {
                requestedFile = _importPathFunc(requestedFile);
                modifiedParentPath = _importPathFunc(parentPath);
            }
            if (string.IsNullOrWhiteSpace(requestedFile))
            {
                return null;
            }

            // Get the input relative path to the parent file
            // Make sure to try checking for a previously processed parent post-modification
            NormalizedPath parentFilePath = new NormalizedPath(parentPath);
            NormalizedPath requestedFilePath = new NormalizedPath(requestedFile);
            if (parentFilePath.IsRelative
                && !_parentAbsolutePaths.TryGetValue(parentFilePath, out parentFilePath)
                && (modifiedParentPath is null || !_parentAbsolutePaths.TryGetValue(modifiedParentPath, out parentFilePath)))
            {
                // Relative parent path and no available absolute path, try with the relative path
                parentFilePath = new NormalizedPath(parentPath);
            }

            // Try to get the relative path to the parent file from inside the input virtual file system
            // But if the parent file isn't under an input path, just use it directly
            NormalizedPath containingInputPath = _fileSystem.GetContainingInputPath(parentFilePath);
            NormalizedPath parentRelativePath = containingInputPath.IsNull
                ? parentFilePath
                : containingInputPath.GetRelativePath(parentFilePath);

            // Find the requested file by first combining with the parent
            NormalizedPath filePath = parentRelativePath.ChangeFileName(requestedFilePath);
            string scss = await GetFileVariationsAsync(filePath, requestedFilePath);
            if (scss is object)
            {
                return scss;
            }

            // That didn't work so try it again as a relative path from the input folder
            scss = await GetFileVariationsAsync(requestedFilePath, requestedFilePath);
            if (!requestedFilePath.IsAbsolute && scss is object)
            {
                return scss;
            }

            return null;
        }

        private async Task<string> GetFileVariationsAsync(NormalizedPath filePath, NormalizedPath requestedFilePath)
        {
            // ...as specified
            string scss = await GetFileAsync(filePath, requestedFilePath);
            if (scss is object)
            {
                return scss;
            }

            // ...with extension (if not already)
            if (!filePath.HasExtension || filePath.Extension != ".scss")
            {
                NormalizedPath extensionPath = filePath.AppendExtension(".scss");
                scss = await GetFileAsync(extensionPath, requestedFilePath);
                if (scss is object)
                {
                    return scss;
                }

                // ...and with underscore prefix (if not already)
                if (!extensionPath.FileName.FullPath.StartsWith("_"))
                {
                    extensionPath = extensionPath.ChangeFileName("_" + extensionPath.FileName.FullPath);
                    scss = await GetFileAsync(extensionPath, requestedFilePath);
                    if (scss is object)
                    {
                        return scss;
                    }
                }
            }

            // ...with underscore prefix (if not already)
            if (!filePath.FileName.FullPath.StartsWith("_"))
            {
                filePath = filePath.ChangeFileName("_" + filePath.FileName.FullPath);
                scss = await GetFileAsync(filePath, requestedFilePath);
                if (scss is object)
                {
                    return scss;
                }
            }

            return null;
        }

        private async Task<string> GetFileAsync(NormalizedPath filePath, NormalizedPath requestedFilePath)
        {
            string scss = null;
            IFile file = _fileSystem.GetInputFile(filePath);
            if (file.Exists)
            {
                if (requestedFilePath.IsRelative)
                {
                    _parentAbsolutePaths.AddOrUpdate(requestedFilePath, file.Path, (x, y) => file.Path);
                }
                scss = await file.ReadAllTextAsync();
            }
            return scss;
        }
    }
}