using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class CombineDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : CombineDocumentsFixture
        {
            [Test]
            public async Task AppendsContent()
            {
                // Given
                TestDocument a = new TestDocument("a");
                TestDocument b = new TestDocument("b");
                CombineDocuments combine = new CombineDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b }, combine);

                // Then
                Assert.That(
                    await results
                        .ToAsyncEnumerable()
                        .Select(async (IDocument x, CancellationToken _) => await x.GetContentStringAsync())
                        .ToListAsync(),
                    Is.EqualTo(new[] { "ab" }));
            }

            [Test]
            public async Task KeepsSameMediaType()
            {
                // Given
                TestDocument a = new TestDocument("a", "Foo");
                TestDocument b = new TestDocument("b", "Foo");
                CombineDocuments combine = new CombineDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b }, combine);

                // Then
                results.Single().ContentProvider.MediaType.ShouldBe("Foo");
            }

            [Test]
            public async Task DifferentMediaTypes()
            {
                // Given
                TestDocument a = new TestDocument("a", "Foo");
                TestDocument b = new TestDocument("b", "Bar");
                CombineDocuments combine = new CombineDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b }, combine);

                // Then
                results.Single().ContentProvider.MediaType.ShouldBeNull();
            }

            [Test]
            public async Task NullMediaType()
            {
                // Given
                TestDocument a = new TestDocument("a", "Foo");
                TestDocument b = new TestDocument("b");
                CombineDocuments combine = new CombineDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b }, combine);

                // Then
                results.Single().ContentProvider.MediaType.ShouldBeNull();
            }

            [Test]
            public async Task MergesMetadata()
            {
                // Given
                TestDocument a = new TestDocument(new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 2 }
                });
                TestDocument b = new TestDocument(new Dictionary<string, object>
                {
                    { "b", 3 },
                    { "c", 4 }
                });
                CombineDocuments combine = new CombineDocuments();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, combine);

                // Then
                new Dictionary<string, object>
                {
                    { "a", 1 },
                    { "b", 3 },
                    { "c", 4 }
                }.ShouldBeSubsetOf(results[0]);
            }
        }

        private IEnumerable Iterate(IEnumerator iterator)
        {
            while (iterator.MoveNext())
            {
                yield return iterator.Current;
            }
        }
    }
}