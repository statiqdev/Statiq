using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestPipelineOutputs : IPipelineOutputs
    {
        public TestPipelineOutputs(IDictionary<string, ImmutableArray<IDocument>> outputs = null)
        {
            Dictionary = outputs ?? new Dictionary<string, ImmutableArray<IDocument>>();
        }

        public IDictionary<string, ImmutableArray<IDocument>> Dictionary { get; }

        public IReadOnlyDictionary<string, DocumentList<IDocument>> ByPipeline() =>
            Dictionary.ToDictionary(x => x.Key, x => x.Value.ToDocumentList());

        public DocumentList<IDocument> ExceptPipeline(string pipelineName)
        {
            pipelineName.ThrowIfNull(nameof(pipelineName));
            return Dictionary
                .Where(x => !x.Key.Equals(pipelineName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value)
                .ToDocumentList();
        }

        public DocumentList<IDocument> FromPipeline(string pipelineName)
        {
            pipelineName.ThrowIfNull(nameof(pipelineName));
            return Dictionary.TryGetValue(pipelineName, out ImmutableArray<IDocument> results)
                ? results.ToDocumentList()
                : DocumentList<IDocument>.Empty;
        }

        public IEnumerator<IDocument> GetEnumerator() =>
            Dictionary.SelectMany(x => x.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
