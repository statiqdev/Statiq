using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Testing;
using Shouldly;
using Statiq.Common;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ConcatDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : ConcatDocumentsFixture
        {
            [Test]
            public async Task ResultsInCorrectCountsWithInputDocumentsOnConcat()
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
                IReadOnlyList<IDocument> results = await ExecuteAsync(a, new ConcatDocuments(b), c);

                // Then
                Assert.That(a.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.ExecuteCount, Is.EqualTo(1));
                Assert.That(c.ExecuteCount, Is.EqualTo(1));
                Assert.That(a.InputCount, Is.EqualTo(1));
                Assert.That(b.InputCount, Is.EqualTo(2));
                Assert.That(c.InputCount, Is.EqualTo(8));
                Assert.That(a.OutputCount, Is.EqualTo(2));
                Assert.That(b.OutputCount, Is.EqualTo(6));
                Assert.That(c.OutputCount, Is.EqualTo(32));
                results.Count.ShouldBe(32);
            }
        }
    }
}
