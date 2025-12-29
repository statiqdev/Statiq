using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public interface IDocumentFactory
    {
        void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new();

        IDocument CreateDocument(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null);

        TDocument CreateDocument<TDocument>(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider = null)
            where TDocument : FactoryDocument, IDocument, new();
    }
}