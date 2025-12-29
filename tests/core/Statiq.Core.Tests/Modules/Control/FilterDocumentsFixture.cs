using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class FilterDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : FilterDocumentsFixture
        {
            [Test]
            public async Task FiltersAllDocumentsWithoutPredicate()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(0);
            }

            [Test]
            public async Task FiltersWithSingePredicate()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(Config.FromDocument(x => x.GetString("Foo") == "b"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task FiltersWithMultiplePredicates()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(
                    Config.FromDocument(x => x.GetString("Foo") == "b"))
                    .Or(Config.FromDocument(x => x.GetString("Foo") == "a"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(2);
            }

            [Test]
            public async Task FiltersWithFalsePredicate()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(
                    Config.FromDocument(x => x.GetString("Foo") == "c"))
                    .Or(Config.FromDocument(x => x.GetString("Foo") == "a"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task FiltersWithSingePredicateNoMatches()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments(Config.FromDocument(x => x.GetString("Foo") == "c"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.ShouldBeEmpty();
            }

            [Test]
            public async Task FiltersWithMetadataKey()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Foo", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments("Foo");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(2);
            }

            [Test]
            public async Task FiltersWithMultipleMetadataKeys()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Bar", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments("Foo")
                    .Or("Bar");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(2);
            }

            [Test]
            public async Task FiltersWithMetadataKeyAndPredicate()
            {
                // Given
                TestDocument a = new TestDocument()
                {
                    { "Foo", "a" }
                };
                TestDocument b = new TestDocument()
                {
                    { "Bar", "b" }
                };
                TestDocument c = new TestDocument();
                FilterDocuments filter = new FilterDocuments("Foo")
                    .Or(Config.FromDocument(doc => doc.GetString("Bar") == "b"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c }, filter);

                // Then
                results.Count.ShouldBe(2);
            }
        }
    }
}
