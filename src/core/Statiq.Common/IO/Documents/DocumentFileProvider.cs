using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Represents a sequence of documents as a file system (I.e., for use in the globber).
    /// </summary>
    public class DocumentFileProvider : IFileProvider
    {
        /// <summary>
        /// Creates a file provider for a sequence of documents.
        /// </summary>
        /// <remarks>
        /// This constructor does not flatten the documents. Only the top-level sequence
        /// (usually the parent-most documents) will be part of the file provider.
        /// </remarks>
        /// <param name="documents">The documents to provide virtual directories and files for.</param>
        /// <param name="source">
        /// <c>true</c> to use <see cref="IDocument.Source"/> as the basis for paths,
        /// <c>false</c> to use <see cref="IDocument.Destination"/>.
        /// </param>
        /// <param name="flatten">
        /// <c>true</c> to flatten the documents, <c>false</c> otherwise.
        /// If <c>false</c> only the top-level sequence (usually the parent-most documents) will be part of the file provider.
        /// </param>
        /// <param name="childrenKey">
        /// The metadata key that contains the children or <c>null</c> to flatten documents in all metadata keys.
        /// This parameter has no effect if <paramref name="flatten"/> is <c>false</c>.
        /// </param>
        public DocumentFileProvider(IEnumerable<IDocument> documents, bool source, bool flatten = true, string childrenKey = Keys.Children)
        {
            if (documents is object)
            {
                foreach (IDocument document in flatten ? documents.Flatten(childrenKey) : documents)
                {
                    // Ignore documents without a destination if using destination as the basis
                    if (!source && document.Destination.IsNull)
                    {
                        continue;
                    }

                    NormalizedPath path = source
                        ? document.Source
                        : NormalizedPath.AbsoluteRoot.Combine(document.Destination);
                    if (!path.IsNull)
                    {
                        Files[path] = document;
                        NormalizedPath directory = path.Parent;
                        while (!directory.IsNullOrEmpty)
                        {
                            Directories.Add(directory);
                            directory = directory.Parent;
                        }
                    }
                }
            }
        }

        internal Dictionary<NormalizedPath, IDocument> Files { get; } = new Dictionary<NormalizedPath, IDocument>();

        internal HashSet<NormalizedPath> Directories { get; } = new HashSet<NormalizedPath>();

        public IDirectory GetDirectory(NormalizedPath path) => new DocumentDirectory(this, path);

        public IFile GetFile(NormalizedPath path) => new DocumentFile(this, path);

        public IDocument GetDocument(in NormalizedPath path) =>
            Files.TryGetValue(path, out IDocument document) ? document : throw new KeyNotFoundException();
    }
}