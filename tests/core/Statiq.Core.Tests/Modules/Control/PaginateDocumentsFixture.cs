using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class PaginateDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : PaginateDocumentsFixture
        {
            [Test]
            public async Task PaginateSetsDocumentsInMetadata()
            {
                // Given
                List<IList<string>> content = new List<IList<string>>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                PaginateDocuments paginate = new PaginateDocuments(3);
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        List<string> pageContent = await d
                            .GetDocuments(Keys.Children)
                            .ToAsyncEnumerable()
                            .Select(async (IDocument x, CancellationToken _) => await x.GetContentStringAsync())
                            .ToListAsync();
                        content.Add(pageContent);
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, paginate, gatherData);

                // Then
                Assert.That(content.Count, Is.EqualTo(3));
                Assert.That(content[0], Is.EqualTo(new[] { "1", "2", "3" }));
                Assert.That(content[1], Is.EqualTo(new[] { "4", "5", "6" }));
                Assert.That(content[2], Is.EqualTo(new[] { "7", "8" }));
            }

            [Test]
            public async Task SetsIndex()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestDocument e = new TestDocument();
                PaginateDocuments paginate = new PaginateDocuments(2);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b, c, d, e }, paginate);

                // Then
                results.Select(x => x.GetInt(Keys.Index)).ShouldBe(new[] { 1, 2, 3 });
            }

            [Test]
            public async Task SetsTotalPages()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestDocument e = new TestDocument();
                PaginateDocuments paginate = new PaginateDocuments(2);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b, c, d, e }, paginate);

                // Then
                results.Select(x => x.GetInt(Keys.TotalPages)).ShouldBe(new[] { 3, 3, 3 });
            }

            [Test]
            public async Task SetsTotalItems()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestDocument e = new TestDocument();
                PaginateDocuments paginate = new PaginateDocuments(2);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b, c, d, e }, paginate);

                // Then
                results.Select(x => x.GetInt(Keys.TotalItems)).ShouldBe(new[] { 5, 5, 5 });
            }

            [Test]
            public async Task SetsPrevious()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestDocument e = new TestDocument();
                PaginateDocuments paginate = new PaginateDocuments(2);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b, c, d, e }, paginate);
                TestExecutionContext context = new TestExecutionContext(results);  // Reset the context since that's what would happen during execution

                // Then
                results[0].GetDocument(Keys.Previous).ShouldBeNull();
                results[1].GetDocument(Keys.Previous).ShouldBe(results[0]);
                results[2].GetDocument(Keys.Previous).ShouldBe(results[1]);
            }

            [Test]
            public async Task SetsNext()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestDocument e = new TestDocument();
                PaginateDocuments paginate = new PaginateDocuments(2);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b, c, d, e }, paginate);
                TestExecutionContext context = new TestExecutionContext(results);  // Reset the context since that's what would happen during execution

                // Then
                results[0].GetDocument(Keys.Next).ShouldBe(results[1]);
                results[1].GetDocument(Keys.Next).ShouldBe(results[2]);
                results[2].GetDocument(Keys.Next).ShouldBeNull();
            }
        }
    }
}