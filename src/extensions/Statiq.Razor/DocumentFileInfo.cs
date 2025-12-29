using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Statiq.Common;

namespace Statiq.Razor
{
    internal class DocumentFileInfo : IFileInfo
    {
        private readonly IFileInfo _info;
        private readonly IDocument _document;

        public DocumentFileInfo(IFileInfo info, IDocument document)
        {
            _info = info;
            _document = document;
        }

        public bool Exists => true;

        public long Length => _document.ContentProvider.GetLength();

        public string PhysicalPath => _info.PhysicalPath;

        public string Name => _info.Name;

        public DateTimeOffset LastModified => DateTimeOffset.Now;

        public bool IsDirectory => false;

        public Stream CreateReadStream() => _document.GetContentStream();
    }
}
