using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Less;
using Statiq.Testing;

namespace Statiq.Sass.Tests
{
    [TestFixture]
    public class CompileLessFixture : BaseFixture
    {
        public class ExecuteTests : CompileLessFixture
        {
            [Test]
            public async Task Convert()
            {
                // Given
                const string input = @"
@foo: black;

div {
 color: @foo;
}";

                const string output = @"div {
  color: black;
}
";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.less", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.less"),
                    new NormalizedPath("assets/test.less"),
                    input);
                CompileLess less = new CompileLess();

                // When
                TestDocument result = await ExecuteAsync(document, context, less).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            [Test]
            public async Task ThrowsForBadLess()
            {
                // Given
                const string input = @"
@foo: black

div {
 color: @foo;
}";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.less", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.less"),
                    new NormalizedPath("assets/test.less"),
                    input);
                CompileLess less = new CompileLess();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(document, context, less));
            }
        }
    }
}
