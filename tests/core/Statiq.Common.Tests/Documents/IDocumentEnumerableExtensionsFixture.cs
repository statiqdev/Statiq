using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class IDocumentEnumerableExtensionsFixture : BaseFixture
    {
        public class FilterSourcesTests : IDocumentEnumerableExtensionsFixture
        {
            [Test]
            public void FiltersSources()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.FileSystem.InputPaths.Add("theme");
                TestDocument a = new TestDocument(new NormalizedPath("/input/Foo/a.txt"));
                TestDocument b = new TestDocument(new NormalizedPath("/theme/Foo/Bar/b.txt"));
                TestDocument c = new TestDocument(new NormalizedPath("/Foo/Bar/c.txt"));
                TestDocument d = new TestDocument(new NormalizedPath("/Baz/d.txt"));
                TestDocument[] documents = new[] { a, b, c, d };

                // When
                TestDocument[] results = documents.FilterSources("Foo/**/*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { a, b }, true);
            }

            [Test]
            public void OrderedDescendingByTimestamp()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.FileSystem.InputPaths.Add("theme");
                TestDocument a = new TestDocument(new NormalizedPath("/input/Foo/a.txt"));
                TestDocument b = new TestDocument(new NormalizedPath("/theme/Foo/Bar/b.txt"));
                TestDocument c = new TestDocument(new NormalizedPath("/Foo/Bar/c.txt"));
                TestDocument d = new TestDocument(new NormalizedPath("/Baz/d.txt"));
                TestDocument[] documents = new[] { d, c, b, a };

                // When
                TestDocument[] results = documents.FilterSources("Foo/**/*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { a, b }, true);
            }
        }

        public class FilterDestinationsTests : IDocumentEnumerableExtensionsFixture
        {
            [Test]
            public void FiltersDestinations()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.FileSystem.InputPaths.Add("theme");
                TestDocument a = new TestDocument(new NormalizedPath("/input/Foo/a.txt"), new NormalizedPath("a/b/c.txt"));
                TestDocument b = new TestDocument(new NormalizedPath("/theme/Foo/Bar/b.txt"), new NormalizedPath("a/x/y.txt"));
                TestDocument c = new TestDocument(new NormalizedPath("/Foo/Bar/c.txt"), new NormalizedPath("l/m.txt"));
                TestDocument d = new TestDocument(new NormalizedPath("/Baz/d.txt"), new NormalizedPath("l/n.md"));
                TestDocument[] documents = new[] { a, b, c, d };

                // When
                TestDocument[] results = documents.FilterDestinations("a/**/*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { a, b }, true);
            }

            [Test]
            public void FiltersDestinationsAtRoot()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.FileSystem.InputPaths.Add("theme");
                TestDocument a = new TestDocument(new NormalizedPath("/input/Foo/a.txt"), new NormalizedPath("c.txt"));
                TestDocument b = new TestDocument(new NormalizedPath("/theme/Foo/Bar/b.txt"), new NormalizedPath("a/x/y.txt"));
                TestDocument c = new TestDocument(new NormalizedPath("/Foo/Bar/c.txt"), new NormalizedPath("m.txt"));
                TestDocument d = new TestDocument(new NormalizedPath("/Baz/d.txt"), new NormalizedPath("l/n.md"));
                TestDocument[] documents = new[] { a, b, c, d };

                // When
                TestDocument[] results = documents.FilterDestinations("*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { a, c }, true);
            }

            [Test]
            public void OrderedDescendingByTimestamp()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.FileSystem.InputPaths.Add("theme");
                TestDocument a = new TestDocument(new NormalizedPath("/input/Foo/a.txt"), new NormalizedPath("a/b/c.txt"));
                TestDocument b = new TestDocument(new NormalizedPath("/theme/Foo/Bar/b.txt"), new NormalizedPath("a/x/y.txt"));
                TestDocument c = new TestDocument(new NormalizedPath("/Foo/Bar/c.txt"), new NormalizedPath("l/m.txt"));
                TestDocument d = new TestDocument(new NormalizedPath("/Baz/d.txt"), new NormalizedPath("l/n.md"));
                TestDocument[] documents = new[] { d, c, b, a };

                // When
                TestDocument[] results = documents.FilterDestinations("a/**/*.txt").ToArray();

                // Then
                results.ShouldBe(new[] { b, a }, true);
            }
        }
    }
}