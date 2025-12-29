using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class FilterSourcesFixture : BaseFixture
    {
        public class ExecuteTests : FilterSourcesFixture
        {
            [Test]
            public async Task FiltersSources()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.FileSystem.InputPaths.Add("theme");
                TestDocument a = new TestDocument(new NormalizedPath("/input/Foo/a.txt"));
                TestDocument b = new TestDocument(new NormalizedPath("/theme/Foo/Bar/b.txt"));
                TestDocument c = new TestDocument(new NormalizedPath("/Foo/Bar/c.txt"));
                TestDocument d = new TestDocument(new NormalizedPath("/Baz/d.txt"));
                FilterSources filter = new FilterSources("Foo/**/*.txt");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b, c, d }, context, filter);

                // Then
                results.ShouldBe(new[] { a, b }, true);
            }
        }
    }
}
