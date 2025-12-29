using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Documents
{
    [TestFixture]
    public class PipelineOutputsFixture : BaseFixture
    {
        private readonly IDocument _a = new Document(new NormalizedPath("b-a.html"));
        private readonly IDocument _b = new Document(new NormalizedPath("b-b.html"));
        private readonly IDocument _c = new Document(new NormalizedPath("a-c.html"));
        private readonly IDocument _d = new Document(new NormalizedPath("a-d.html"));
        private readonly IDocument _e = new Document(new NormalizedPath("a-e.html"));
        private readonly IDocument _f = new Document(new NormalizedPath("b-f.html"));

        public class ByPipelineTests : PipelineOutputsFixture
        {
            [Test]
            public void ReturnsPipelineOutputsInNaturalOrder()
            {
                // Given
                PipelineOutputs outputs = new PipelineOutputs(GetPhaseResults());

                // When
                IReadOnlyDictionary<string, DocumentList<IDocument>> results = outputs.ByPipeline();

                // Then
                results["Pipeline A"].ToArray().ShouldBe(new[] { _e });
                results["Pipeline B"].ToArray().ShouldBe(new[] { _a, _f });
            }
        }

        public class ExceptPipelineTests : PipelineOutputsFixture
        {
            [Test]
            public void ReturnsPipelineOutputsInNaturalOrder()
            {
                // Given
                PipelineOutputs outputs = new PipelineOutputs(GetPhaseResults());

                // When
                DocumentList<IDocument> results = outputs.ExceptPipeline("Pipeline A");

                // Then
                results.ToArray().ShouldBe(new[] { _a, _f });
            }

            [Test]
            public void IgnoresNotFoundPipeline()
            {
                // Given
                PipelineOutputs outputs = new PipelineOutputs(GetPhaseResults());

                // When
                DocumentList<IDocument> results = outputs.ExceptPipeline("Pipeline C");

                // Then
                results.ToArray().ShouldBe(new[] { _e, _a, _f });
            }
        }

        public class FromPipelineTests : PipelineOutputsFixture
        {
            [Test]
            public void ReturnsPipelineOutputsInNaturalOrder()
            {
                // Given
                PipelineOutputs outputs = new PipelineOutputs(GetPhaseResults());

                // When
                DocumentList<IDocument> results = outputs.FromPipeline("Pipeline B");

                // Then
                results.ToArray().ShouldBe(new[] { _a, _f });
            }
        }

        public class IndexerTests : PipelineOutputsFixture
        {
            [Test]
            public void OrderedDescendingByTimestamp()
            {
                // Given
                IPipelineOutputs outputs = new PipelineOutputs(GetPhaseResults());

                // When
                FilteredDocumentList<IDocument> results = outputs["*.html"];

                // Then
                results.ToArray().ShouldBe(new[] { _f, _e, _a });
            }

            [Test]
            public void FiltersByDestination()
            {
                // Given
                IPipelineOutputs outputs = new PipelineOutputs(GetPhaseResults());

                // When
                FilteredDocumentList<IDocument> results = outputs["b-*.html"];

                // Then
                results.ToArray().ShouldBe(new[] { _f, _a });
            }
        }

        public class GetEnumeratorTests : PipelineOutputsFixture
        {
            [Test]
            public void OrderedDescendingByTimestamp()
            {
                // Given
                PipelineOutputs outputs = new PipelineOutputs(GetPhaseResults());

                // When
                IDocument[] enumerated = outputs.ToArray();

                // Then
                enumerated.ShouldBe(new[] { _f, _e, _a });
            }
        }

        private Dictionary<string, PhaseResult[]> GetPhaseResults() => new Dictionary<string, PhaseResult[]>
        {
            {
                "Pipeline A",
                new[]
                {
                    new PhaseResult("Pipeline A", Phase.Input, new[] { _c, _d }.ToImmutableArray(), default, default),
                    new PhaseResult("Pipeline A", Phase.Process, new[] { _e }.ToImmutableArray(), default, default)
                }
            },
            {
                "Pipeline B",
                new[]
                {
                    new PhaseResult("Pipeline A", Phase.Input, new[] { _b }.ToImmutableArray(), default, default),
                    new PhaseResult("Pipeline A", Phase.Process, new[] { _a, _f }.ToImmutableArray(), default, default)
                }
            }
        };
    }
}