using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Extracts the content of a Sidecar file for each document and sends it to a child module for processing.
    /// </summary>
    /// <remarks>
    /// This module is typically used in conjunction with the Yaml module to enable putting YAML in a Sidecar file.
    /// First, an attempt is made to find the specified sidecar file for each input document. Once found, the
    /// content in this file is passed to the specified child module(s). Any metadata from the child
    /// module output document(s) is added to the input document. Note that if the child module(s) result
    /// in more than one output document, multiple clones of the input document will be made for each one.
    /// The output document content is set to the original input document content.
    /// </remarks>
    /// <category name="Control" />
    public class ProcessSidecarFile : ParentModule
    {
        private readonly Config<NormalizedPath> _sidecarPath;

        /// <summary>
        /// Searches for sidecar files at the same path as the input document <see cref="IDocument.Source"/> with the additional extension .meta.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public ProcessSidecarFile(params IModule[] modules)
            : this(".meta", modules)
        {
        }

        /// <summary>
        /// Searches for sidecar files at the same path as the input document <see cref="IDocument.Source"/> with the specified additional extension.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="extension">The extension to search.</param>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public ProcessSidecarFile(string extension, params IModule[] modules)
            : base(modules)
        {
            if (string.IsNullOrEmpty(extension))
            {
                throw new ArgumentException("Value cannot be null or empty.", nameof(extension));
            }

            _sidecarPath = Config.FromDocument(doc => doc.Source.IsNull ? NormalizedPath.Null : doc.Source.AppendExtension(extension));
        }

        /// <summary>
        /// Uses a delegate to describe where to find the sidecar file for each input document.
        /// If a sidecar file is found, it's content is passed to the specified child modules for processing.
        /// </summary>
        /// <param name="sidecarPath">A delegate that returns a <see cref="NormalizedPath"/> with the desired sidecar path.</param>
        /// <param name="modules">The modules to execute against the sidecar file.</param>
        public ProcessSidecarFile(Config<NormalizedPath> sidecarPath, params IModule[] modules)
            : base(modules)
        {
            _sidecarPath = sidecarPath.ThrowIfNull(nameof(sidecarPath));
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            NormalizedPath sidecarPath = await _sidecarPath.GetValueAsync(input, context);
            if (!sidecarPath.IsNull)
            {
                IFile sidecarFile = context.FileSystem.GetInputFile(sidecarPath);
                if (sidecarFile.Exists)
                {
                    context.LogDebug($"Processing sidecar file {sidecarPath} for {input.Source.ToDisplayString("unknown")}");
                    IContentProvider sidecarContent = sidecarFile.GetContentProvider();
                    foreach (IDocument result in await context.ExecuteModulesAsync(Children, input.Clone(sidecarContent).Yield()))
                    {
                        return input.Clone(result).Yield();
                    }
                }
                else
                {
                    return input.Yield();
                }
            }
            else
            {
                return input.Yield();
            }
            return null;
        }
    }
}