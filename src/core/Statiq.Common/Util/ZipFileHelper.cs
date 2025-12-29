using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class ZipFileHelper
    {
        public static IFile CreateZipFile(IExecutionContext context, in NormalizedPath directory)
        {
            directory.ThrowIfNull(nameof(directory));

            IDirectory sourceDirectory = context.FileSystem.GetRootDirectory(directory);
            if (!sourceDirectory.Exists)
            {
                throw new DirectoryNotFoundException("Source zip directory does not exist");
            }
            IFile zipFile = context.FileSystem.GetTempFile();
            context.LogDebug($"Creating zip file from {sourceDirectory.Path.FullPath} at {zipFile.Path.FullPath}");
            ZipFile.CreateFromDirectory(sourceDirectory.Path.FullPath, zipFile.Path.FullPath);
            return zipFile;
        }
    }
}