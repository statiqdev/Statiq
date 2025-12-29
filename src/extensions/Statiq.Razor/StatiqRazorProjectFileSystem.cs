using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// A RazorProjectFileSystem that lets us use the Statiq file provider while
    /// allowing replacement of the stream with document content.
    /// </summary>
    // See https://github.com/aspnet/AspNetCore/blob/dfba024c7888e9357ce40687e1771953074340fd/src/Mvc/Mvc.Razor.RuntimeCompilation/src/FileProviderRazorProjectFileSystem.cs
    internal class StatiqRazorProjectFileSystem : RazorProjectFileSystem
    {
        private const string RazorFileExtension = ".cshtml";

        private readonly Microsoft.Extensions.FileProviders.IFileProvider _fileProvider;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public StatiqRazorProjectFileSystem(Microsoft.Extensions.FileProviders.IFileProvider fileProvider, IWebHostEnvironment hostingEnviroment)
        {
            _fileProvider = fileProvider.ThrowIfNull(nameof(fileProvider));
            _hostingEnvironment = hostingEnviroment.ThrowIfNull(nameof(hostingEnviroment));
        }

        [Obsolete("Use GetItem(string path, string fileKind) instead.")]
        public override RazorProjectItem GetItem(string path)
        {
            return GetItem(path, fileKind: null);
        }

        public override RazorProjectItem GetItem(string path, string fileKind)
        {
            path = NormalizeAndEnsureValidPath(path);
            IFileInfo fileInfo = _fileProvider.GetFileInfo(path);

            return new FileProviderRazorProjectItem(fileInfo, basePath: string.Empty, filePath: path, root: _hostingEnvironment.ContentRootPath, fileKind);
        }

        public RazorProjectItem GetItem(string path, IDocument document)
        {
            FileProviderRazorProjectItem projectItem = (FileProviderRazorProjectItem)GetItem(path, fileKind: null);
            return new FileProviderRazorProjectItem(
                new DocumentFileInfo(projectItem.FileInfo, document),
                projectItem.BasePath,
                projectItem.FilePath,
                _hostingEnvironment.ContentRootPath);
        }

        public override IEnumerable<RazorProjectItem> EnumerateItems(string path)
        {
            path = NormalizeAndEnsureValidPath(path);
            return EnumerateFiles(_fileProvider.GetDirectoryContents(path), path, prefix: string.Empty);
        }

        private IEnumerable<RazorProjectItem> EnumerateFiles(IDirectoryContents directory, string basePath, string prefix)
        {
            if (directory.Exists)
            {
                foreach (IFileInfo fileInfo in directory)
                {
                    if (fileInfo.IsDirectory)
                    {
                        string relativePath = prefix + "/" + fileInfo.Name;
                        IDirectoryContents subDirectory = _fileProvider.GetDirectoryContents(JoinPath(basePath, relativePath));
                        IEnumerable<RazorProjectItem> children = EnumerateFiles(subDirectory, basePath, relativePath);
                        foreach (RazorProjectItem child in children)
                        {
                            yield return child;
                        }
                    }
                    else if (string.Equals(RazorFileExtension, Path.GetExtension(fileInfo.Name), StringComparison.OrdinalIgnoreCase))
                    {
                        string filePath = prefix + "/" + fileInfo.Name;

                        yield return new FileProviderRazorProjectItem(fileInfo, basePath, filePath: filePath, root: _hostingEnvironment.ContentRootPath);
                    }
                }
            }
        }

        private static string JoinPath(string path1, string path2)
        {
            bool hasTrailingSlash = path1.EndsWith("/", StringComparison.Ordinal);
            bool hasLeadingSlash = path2.StartsWith("/", StringComparison.Ordinal);
            if (hasLeadingSlash && hasTrailingSlash)
            {
                return path1 + path2.Substring(1);
            }
            else if (hasLeadingSlash || hasTrailingSlash)
            {
                return path1 + path2;
            }

            return path1 + "/" + path2;
        }
    }
}