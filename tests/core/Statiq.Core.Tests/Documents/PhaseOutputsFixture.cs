using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Documents
{
    [TestFixture]
    public class PhaseOutputsFixture : BaseFixture
    {
        public class EnumeratorTests : PhaseOutputsFixture
        {
            [Test]
            public void ProcessPhaseGetsDocumentsFromTransientDependencies()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2 }, true);
            }

            [Test]
            public void TransformPhaseGetsAllDocuments()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.PostProcess, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.PostProcess, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.PostProcess, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2, c1, d1, d2 }, true);
            }

            [Test]
            public void ThrowsIfCurrentPipelineIsOsolated()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }

            [Test]
            public void ThrowsIfInputPhase()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> documents =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Input, pipelines, documents, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                PhaseOutputs documentCollection = new PhaseOutputs(documents, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }

            [Test]
            public void ThrowsIfOutputPhase()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>());
                phase.Pipeline.Isolated = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.ToArray());
            }

            [Test]
            public void OutputPhaseFromDeploymentGetsAllNonDeploymentDocuments()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, new[] { a1 }, Phase.Output);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Output, pipelines, phaseResults, new[] { b1, b2 }, Phase.Output, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Output, pipelines, phaseResults, new[] { c1 }, Phase.Output, phaseB);
                phaseC.Pipeline.Deployment = true;
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Output, pipelines, phaseResults, new[] { d1, d2 }, Phase.Output);
                phaseD.Pipeline.Deployment = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2 }, true);
            }

            [Test]
            public void OutputPhaseFromDeploymentIgnoresNonExecutingNonDeploymentPipelines()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, new[] { a1 }, Phase.Output);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Output, pipelines, phaseResults, new[] { b1, b2 }, Phase.Output, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Output, pipelines, phaseResults, new[] { c1 }, Phase.Output, phaseB);
                phaseC.Pipeline.Deployment = true;
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Output, pipelines, phaseResults, new[] { d1, d2 }, Phase.Output);
                phaseD.Pipeline.Deployment = true;
                phaseResults.Remove("A", out _);
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.ShouldBe(new[] { b1, b2 }, true);
            }

            [Test]
            public void PostProcessPhaseDoesNotGetDocumentsFromTransientDependencies()
            {
                // Given
                TestDocument a1 = new TestDocument((NormalizedPath)"a1", "a1");
                TestDocument b1 = new TestDocument((NormalizedPath)"b1", "b1");
                TestDocument b2 = new TestDocument((NormalizedPath)"b2", "b2");
                TestDocument c1 = new TestDocument((NormalizedPath)"c1", "c1");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, new[] { a1 }, Phase.PostProcess);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.PostProcess, pipelines, phaseResults, new[] { b1, b2 }, Phase.PostProcess, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.PostProcess, pipelines, phaseResults, new[] { c1 }, Phase.PostProcess, phaseB);
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.Select(x => x.Source).ShouldBeEmpty();
            }

            [Test]
            public void PostProcessHasDependenciesPhaseGetsDocumentsFromTransientDependencies()
            {
                // Given
                TestDocument a1 = new TestDocument((NormalizedPath)"a1", "a1");
                TestDocument b1 = new TestDocument((NormalizedPath)"b1", "b1");
                TestDocument b2 = new TestDocument((NormalizedPath)"b2", "b2");
                TestDocument c1 = new TestDocument((NormalizedPath)"c1", "c1");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, new[] { a1 }, Phase.PostProcess);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.PostProcess, pipelines, phaseResults, new[] { b1, b2 }, Phase.PostProcess, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.PostProcess, pipelines, phaseResults, new[] { c1 }, Phase.PostProcess, phaseB);
                phaseC.Pipeline.PostProcessHasDependencies = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ToArray();

                // Then
                result.Select(x => x.Source).ShouldBe(new NormalizedPath[] { "/input/a1", "/input/b1", "/input/b2" }, true);
            }
        }

        public class ExceptPipelineTests : PhaseOutputsFixture
        {
            [Test]
            public void ExcludeCurrentPipelineDuringProcess()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ExceptPipeline("C").ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2 }, true);
            }

            [Test]
            public void ExcludeCurrentPipelineDuringTransform()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.PostProcess, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.PostProcess, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.PostProcess, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ExceptPipeline("C").ToArray();

                // Then
                result.ShouldBe(new[] { a1, b1, b2, d1, d2 }, true);
            }

            [Test]
            public void ExcludeFromDeploymentDuringOutput()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, new[] { a1 }, Phase.Output);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Output, pipelines, phaseResults, new[] { b1, b2 }, Phase.Output, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Output, pipelines, phaseResults, new[] { c1 }, Phase.Output, phaseB);
                phaseC.Pipeline.Deployment = true;
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Output, pipelines, phaseResults, new[] { d1, d2 }, Phase.Output);
                phaseD.Pipeline.Deployment = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.ExceptPipeline("A").ToArray();

                // Then
                result.ShouldBe(new[] { b1, b2 }, true);
            }
        }

        public class FromPipelineTests : PhaseOutputsFixture
        {
            [Test]
            public void ThrowsForInvalidPipeline()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, Array.Empty<IDocument>());
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<KeyNotFoundException>(() => documentCollection.FromPipeline("B"));
            }

            [Test]
            public void ThrowsForNullPipeline()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, Array.Empty<IDocument>());
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<ArgumentException>(() => documentCollection.FromPipeline(null));
            }

            [Test]
            public void ThrowsForEmptyPipeline()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, Array.Empty<IDocument>());
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<ArgumentException>(() => documentCollection.FromPipeline(string.Empty));
            }

            [Test]
            public void ThrowsForCurrentPipelineDuringProcess()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, Array.Empty<IDocument>());
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("A"));
            }

            [Test]
            public void ThrowsDuringInput()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Input, pipelines, phaseResults, Array.Empty<IDocument>(), Phase.Input);
                GetPipelineAndPhase("B", Phase.Input, pipelines, phaseResults, Array.Empty<IDocument>(), Phase.Input);
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("B"));
            }

            [Test]
            public void ThrowsDuringOutput()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>(), Phase.Output);
                GetPipelineAndPhase("B", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>(), Phase.Output);
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("B"));
            }

            [Test]
            public void DoesNotThrowForDeploymentDuringOutput()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>(), Phase.Output);
                phase.Pipeline.Deployment = true;
                GetPipelineAndPhase("B", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>(), Phase.Output);
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.NotThrow(() => documentCollection.FromPipeline("B"));
            }

            [Test]
            public void ThrowsForCurrentDeploymentPipelineDuringOutput()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>(), Phase.Output);
                phase.Pipeline.Deployment = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("A"));
            }

            [Test]
            public void ThrowsForOtherDeploymentDuringOutput()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>());
                phase.Pipeline.Deployment = true;
                PipelinePhase phaseB = GetPipelineAndPhase("B", Phase.Output, pipelines, phaseResults, Array.Empty<IDocument>());
                phaseB.Pipeline.Deployment = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("B"));
            }

            [Test]
            public void DoesNotThrowForCurrentPipelineDuringTransform()
            {
                // Given
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phase = GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, Array.Empty<IDocument>());
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phase, pipelines);

                // When, Then
                Should.NotThrow(() => documentCollection.FromPipeline("A"));
            }

            [Test]
            public void ThrowsForNonExistingPipeline()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When, Then
                Should.Throw<KeyNotFoundException>(() => documentCollection.FromPipeline("E"));
            }

            [Test]
            public void GetsDocumentsDuringTransformPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.PostProcess, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.PostProcess, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.PostProcess, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("D").ToArray();

                // Then
                result.ShouldBe(new[] { d1, d2 });
            }

            [Test]
            public void IsCaseInsensitive()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.PostProcess, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.PostProcess, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.PostProcess, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("d").ToArray();

                // Then
                result.ShouldBe(new[] { d1, d2 });
            }

            [Test]
            public void ThrowsForNonDependentPipelineDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When, Then
                Should.Throw<InvalidOperationException>(() => documentCollection.FromPipeline("D"));
            }

            [Test]
            public void GetsDocumentsForDependentPipelineDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("B").ToArray();

                // Then
                result.ShouldBe(new[] { b1, b2 });
            }

            [Test]
            public void GetsDocumentsForTransientDependentPipelineDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                TestDocument c1 = new TestDocument("c1");
                TestDocument d1 = new TestDocument("d1");
                TestDocument d2 = new TestDocument("d2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1 });
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PipelinePhase phaseC =
                    GetPipelineAndPhase("C", Phase.Process, pipelines, phaseResults, new[] { c1 }, phaseB);
                PipelinePhase phaseD =
                    GetPipelineAndPhase("D", Phase.Process, pipelines, phaseResults, new[] { d1, d2 });
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseC, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("A").ToArray();

                // Then
                result.ShouldBe(new[] { a1 });
            }

            [Test]
            public void GetsDocumentsForInputPhaseDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument a2 = new TestDocument("a2");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, new[] { a1, a2 }, Phase.Input);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseB, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("A").ToArray();

                // Then
                result.ShouldBe(new[] { a1, a2 });
            }

            [Test]
            public void GetsEmptyDocumentsForNoPhasesDuringProcessPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument a2 = new TestDocument("a2");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Process, pipelines, phaseResults, null, Phase.Input);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Process, pipelines, phaseResults, new[] { b1, b2 }, phaseA);

                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseB, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("A").ToArray();

                // Then
                result.ShouldBeEmpty();
            }

            [Test]
            public void GetsDocumentsForInputPhaseDuringTransformPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument a2 = new TestDocument("a2");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.PostProcess, pipelines, phaseResults, new[] { a1, a2 }, Phase.Input);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.PostProcess, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseB, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("A").ToArray();

                // Then
                result.ShouldBe(new[] { a1, a2 });
            }

            [Test]
            public void GetsDocumentsForInputPhaseDuringDeploymentOutputPhase()
            {
                // Given
                TestDocument a1 = new TestDocument("a1");
                TestDocument a2 = new TestDocument("a2");
                TestDocument b1 = new TestDocument("b1");
                TestDocument b2 = new TestDocument("b2");
                ConcurrentDictionary<string, PhaseResult[]> phaseResults =
                    new ConcurrentDictionary<string, PhaseResult[]>(StringComparer.OrdinalIgnoreCase);
                IPipelineCollection pipelines = new TestPipelineCollection();
                PipelinePhase phaseA =
                    GetPipelineAndPhase("A", Phase.Output, pipelines, phaseResults, new[] { a1, a2 }, Phase.Input);
                PipelinePhase phaseB =
                    GetPipelineAndPhase("B", Phase.Output, pipelines, phaseResults, new[] { b1, b2 }, phaseA);
                phaseB.Pipeline.Deployment = true;
                PhaseOutputs documentCollection = new PhaseOutputs(phaseResults, phaseB, pipelines);

                // When
                IDocument[] result = documentCollection.FromPipeline("A").ToArray();

                // Then
                result.ShouldBe(new[] { a1, a2 });
            }
        }

        private PipelinePhase GetPipelineAndPhase(
            string pipelineName,
            Phase currentPhase,
            IPipelineCollection pipelines,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            IDocument[] outputs,
            Phase phaseForOutputs,
            params PipelinePhase[] dependencies)
        {
            TestPipeline pipeline = new TestPipeline();
            PipelinePhase pipelinePhase = new PipelinePhase(pipeline, pipelineName, currentPhase, Array.Empty<IModule>(), NullLogger.Instance, dependencies);
            pipelines.Add(pipelineName, pipeline);
            PhaseResult[] results = new PhaseResult[4];
            if (outputs is object)
            {
                results[(int)phaseForOutputs] = new PhaseResult(pipelineName, phaseForOutputs, outputs.ToImmutableArray(), default, 0);
            }
            phaseResults.TryAdd(pipelineName, results);
            return pipelinePhase;
        }

        private PipelinePhase GetPipelineAndPhase(
            string pipelineName,
            Phase phase,
            IPipelineCollection pipelines,
            ConcurrentDictionary<string, PhaseResult[]> phaseResults,
            IDocument[] outputs,
            params PipelinePhase[] dependencies) =>
            GetPipelineAndPhase(pipelineName, phase, pipelines, phaseResults, outputs, Phase.Process, dependencies);
    }
}