using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Testing;
using Shouldly;
using Statiq.Common;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class MergeDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : MergeDocumentsFixture
        {
            [Test]
            public async Task ReplacesContent()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new MergeDocuments(b),
                    new SetMetadata("Content", Config.FromDocument(async doc => await doc.GetContentStringAsync())));

                // Then
                Assert.That(results.Select(x => x["Content"]), Is.EqualTo(new[] { "1121" }));
            }

            [Test]
            public async Task ReverseReplacesContent()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(
                    a,
                    new MergeDocuments(b).Reverse(),
                    new SetMetadata("Content", Config.FromDocument(async doc => await doc.GetContentStringAsync())));

                // Then
                Assert.That(results.Select(x => x["Content"]), Is.EqualTo(new[] { "11" }));
            }

            [Test]
            public async Task CombinesMetadata()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                Assert.That(results.Select(x => x["A"]), Is.EqualTo(new[] { 11 }));
                Assert.That(results.Select(x => x["B"]), Is.EqualTo(new[] { 21 }));
            }

            [Test]
            public async Task CombinesAndOverwritesMetadata()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("A")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                Assert.That(results.Select(x => x["A"]), Is.EqualTo(new[] { 21 }));
            }

            [Test]
            public async Task SingleInputSingleResult()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                Assert.That(a.OutputCount, Is.EqualTo(1));
                Assert.That(b.OutputCount, Is.EqualTo(1));
                Assert.That(results.Select(x => x["A"]), Is.EqualTo(new[] { 11 }));
                Assert.That(results.Select(x => x["B"]), Is.EqualTo(new[] { 21 }));
            }

            [Test]
            public async Task SingleInputMultipleResults()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                Assert.That(a.OutputCount, Is.EqualTo(1));
                Assert.That(b.OutputCount, Is.EqualTo(2));
                Assert.That(results.Select(x => x["A"]), Is.EqualTo(new[] { 11, 11 }));
                Assert.That(results.Select(x => x["B"]), Is.EqualTo(new[] { 21, 22 }));
            }

            [Test]
            public async Task MultipleInputsSingleResult()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                a.OutputCount.ShouldBe(2);
                b.OutputCount.ShouldBe(2);
                results.Select(x => x["A"]).ShouldBe(new object[] { 11, 12, 11, 12 });
                results.Select(x => x["B"]).ShouldBe(new object[] { 21, 22, 21, 22 });
            }

            [Test]
            public async Task MultipleInputsMultipleResults()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    Value = 10,
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    Value = 20,
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b));

                // Then
                a.OutputCount.ShouldBe(2);
                b.OutputCount.ShouldBe(4);
                results.Select(x => x["A"]).ShouldBe(new object[] { 11, 11, 12, 12, 11, 11, 12, 12 });
                results.Select(x => x["B"]).ShouldBe(new object[] { 21, 22, 23, 24, 21, 22, 23, 24 });
            }

            [Test]
            public async Task ResultsInCorrectCountsWithInputDocuments()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 1,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule c = new CountModule("C")
                {
                    AdditionalOutputs = 3,
                    EnsureInputDocument = true
                };

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new MergeDocuments(b), c);

                // Then
                Assert.That(a.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.ExecuteCount, Is.EqualTo(1));
                Assert.That(c.ExecuteCount, Is.EqualTo(1));
                Assert.That(a.InputCount, Is.EqualTo(1));
                Assert.That(b.InputCount, Is.EqualTo(2));
                Assert.That(c.InputCount, Is.EqualTo(12));
                Assert.That(a.OutputCount, Is.EqualTo(2));
                Assert.That(b.OutputCount, Is.EqualTo(6));
                Assert.That(c.OutputCount, Is.EqualTo(48));
                results.Count.ShouldBe(48);
            }
        }
    }
}
