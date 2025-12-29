using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Statiq.Common;

namespace Statiq.Common
{
    public class DocumentFactory : IDocumentFactory
    {
        private readonly IExecutionState _executionState;
        private readonly IReadOnlySettings _settings;

        private IFactory _defaultFactory = Factory<Document>.Instance;

        public DocumentFactory(IExecutionState executionState, IReadOnlySettings settings)
        {
            _executionState = executionState.ThrowIfNull(nameof(executionState));
            _settings = settings.ThrowIfNull(nameof(settings));
        }

        private interface IFactory
        {
            IDocument CreateDocument(
                IReadOnlySettings settings,
                NormalizedPath source,
                NormalizedPath destination,
                IMetadata metadata,
                IContentProvider contentProvider);
        }

        private class Factory<TDocument> : IFactory
             where TDocument : FactoryDocument, IDocument, new()
        {
            // Use a singleton pattern to avoid repeated allocations of factories for the same document type
            public static readonly Factory<TDocument> Instance = new Factory<TDocument>();

            private Factory()
            {
            }

            public IDocument CreateDocument(
                IReadOnlySettings settings,
                NormalizedPath source,
                NormalizedPath destination,
                IMetadata metadata,
                IContentProvider contentProvider) =>
                new TDocument().Initialize(settings, source, destination, metadata, contentProvider);
        }

        public virtual void SetDefaultDocumentType<TDocument>()
            where TDocument : FactoryDocument, IDocument, new() =>
            _defaultFactory = Factory<TDocument>.Instance;

        public virtual IDocument CreateDocument(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider) =>
            _defaultFactory.CreateDocument(
                _settings,
                source,
                destination,
                new Metadata(_executionState, items),
                contentProvider);

        public virtual TDocument CreateDocument<TDocument>(
            NormalizedPath source,
            NormalizedPath destination,
            IEnumerable<KeyValuePair<string, object>> items,
            IContentProvider contentProvider)
            where TDocument : FactoryDocument, IDocument, new() =>
            (TDocument)Factory<TDocument>.Instance.CreateDocument(
                _settings,
                source,
                destination,
                new Metadata(_executionState, items),
                contentProvider);
    }
}