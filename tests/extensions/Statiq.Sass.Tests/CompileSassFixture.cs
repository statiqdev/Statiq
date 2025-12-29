using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Sass.Tests
{
    [TestFixture]
    public class CompileSassFixture : BaseFixture
    {
        public class ExecuteTests : CompileSassFixture
        {
            [Test]
            public async Task Convert()
            {
                // Given
                const string input = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;

body {
  font: 100% $font-stack;
  color: $primary-color;
}";

                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    input);

                CompileSass sass = new CompileSass().WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            [Test]
            public async Task EmptyOutputForEmptyContent()
            {
                // Given
                string input = string.Empty;

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    input);

                CompileSass sass = new CompileSass();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBeEmpty();
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            [Test]
            public async Task ConvertingBadSassFails()
            {
                // Given
                const string input = @"
$font-stack:    Helvetica, sans-serif
$primary-color: #333

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    input);

                CompileSass sass = new CompileSass();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(document, context, sass));
            }

            [Test]
            public async Task NestedImports()
            {
                // Given
                const string outerImport = @"
$font-stack:    Helvetica, sans-serif;";
                const string innerImport = @"
@import 'outer-import.scss';
$primary-color: #333;";
                const string input = @"
@import 'libs/_inner-import.scss';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_outer-import.scss", outerImport);
                fileProvider.AddFile("/input/assets/libs/_inner-import.scss", innerImport);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    input);

                CompileSass sass = new CompileSass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            [Test]
            public async Task ImportWithoutExtension()
            {
                // Given
                const string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                const string input = @"
@import 'libs/_test-import';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    input);

                CompileSass sass = new CompileSass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            [Test]
            public async Task ImportWithoutPrefix()
            {
                // Given
                const string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                const string input = @"
@import 'libs/test-import.scss';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    input);

                CompileSass sass = new CompileSass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            [Test]
            public async Task ImportWithoutPrefixOrExtension()
            {
                // Given
                const string import = @"
$font-stack:    Helvetica, sans-serif;
$primary-color: #333;";
                const string input = @"
@import 'libs/test-import';

body {
  font: 100% $font-stack;
  color: $primary-color;
}";
                const string output = "body { font: 100% Helvetica, sans-serif; color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddDirectory("/input/assets/libs");
                fileProvider.AddFile("/input/assets/test.scss", input);
                fileProvider.AddFile("/input/assets/libs/_test-import.scss", import);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    input);

                CompileSass sass = new CompileSass().IncludeSourceComments(false).WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }

            [TestCase("Sass_primary-color")]
            [TestCase("sass_primary-color")]
            [TestCase("sass_primary color")]
            [TestCase("Sass-primary-color")]
            [TestCase("sass-primary-color")]
            [TestCase("sass-primary color")]
            public async Task InjectsMetadataVariables(string metadataName)
            {
                // Given
                const string input = @"
body {
  color: $primary-color;
}";

                const string output = "body { color: #333; }\n";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/assets");
                fileProvider.AddFile("/input/assets/test.scss", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/assets/test.scss"),
                    new NormalizedPath("assets/test.scss"),
                    new MetadataItems
                    {
                        { metadataName, "#333" }
                    },
                    input);

                CompileSass sass = new CompileSass().WithCompactOutputStyle();

                // When
                TestDocument result = await ExecuteAsync(document, context, sass).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
                result.Destination.FullPath.ShouldBe("assets/test.css");
            }
        }
    }
}