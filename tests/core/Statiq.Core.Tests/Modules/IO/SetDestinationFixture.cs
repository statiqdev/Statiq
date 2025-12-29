using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class SetDestinationFixture : BaseFixture
    {
        public class ExecuteTests : SetDestinationFixture
        {
            [TestCase(Keys.DestinationPath, "OtherFolder/foo.bar", "OtherFolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "foo.bar", "Subfolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "fizz/foo.bar", "Subfolder/fizz/foo.bar")]
            [TestCase(Keys.DestinationFileName, "../fizz/foo.bar", "fizz/foo.bar")]
            [TestCase(Keys.DestinationExtension, "foo", "Subfolder/write-test.foo")]
            [TestCase(Keys.DestinationExtension, ".foo", "Subfolder/write-test.foo")]
            public async Task SetsDestinationFromMetadata(string key, string value, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"))
                {
                    { key, value }
                };
                SetDestination setDestination = new SetDestination();

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            [TestCase(Keys.DestinationPath, "OtherFolder/foo.bar", "OtherFolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "foo.bar", "Subfolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "fizz/foo.bar", "Subfolder/fizz/foo.bar")]
            [TestCase(Keys.DestinationFileName, "../fizz/foo.bar", "fizz/foo.bar")]
            [TestCase(Keys.DestinationExtension, "foo", "Subfolder/write-test.foo")]
            [TestCase(Keys.DestinationExtension, ".foo", "Subfolder/write-test.foo")]
            public async Task SetsDestinationFromMetadataForFalsePageExtension(string key, string value, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"))
                {
                    { key, value }
                };
                SetDestination setDestination = new SetDestination(false);

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            [TestCase(Keys.DestinationPath, "OtherFolder/foo.bar", "OtherFolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "foo.bar", "foo.bar")]
            [TestCase(Keys.DestinationFileName, "fizz/foo.bar", "fizz/foo.bar")]
            [TestCase(Keys.DestinationFileName, "../fizz/foo.bar", "../fizz/foo.bar")]
            [TestCase(Keys.DestinationExtension, "foo", null)]
            [TestCase(Keys.DestinationExtension, ".foo", null)]
            public async Task SetsDestinationFromMetadataWhenNullDestination(string key, string value, string expected)
            {
                // Given
                TestDocument input = new TestDocument()
                {
                    { key, value }
                };
                SetDestination setDestination = new SetDestination();

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            [TestCase(".foo", "Subfolder/write-test.foo")]
            [TestCase(".foo.bar", "Subfolder/write-test.foo.bar")]
            public async Task SetsDestinationUsingExtension(string extension, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination(extension);

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            public async Task SetsDestinationUsingPageExtension()
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination(true);

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("Subfolder/write-test.html");
            }

            [TestCase("foo", "foo")]
            [TestCase("./abc/foo", "abc/foo")]
            public async Task SetsDestinationUsingPath(string path, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination(path);

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            [TestCase("foo", "foo")]
            [TestCase("abc/foo", "abc/foo")]
            [TestCase("./abc/foo", "abc/foo")]
            public async Task SetsDestinationUsingNormalizedPath(string path, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination(new NormalizedPath(path));

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            [TestCase(Keys.DestinationPath, "OtherFolder/foo.bar", "OtherFolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "foo.bar", "Subfolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "fizz/foo.bar", "Subfolder/fizz/foo.bar")]
            [TestCase(Keys.DestinationFileName, "../fizz/foo.bar", "fizz/foo.bar")]
            [TestCase(Keys.DestinationExtension, "foo", "Subfolder/write-test.foo")]
            [TestCase(Keys.DestinationExtension, ".foo", "Subfolder/write-test.foo")]
            public async Task SetsDestinationFromMetadataOverridesExtension(string key, string value, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"))
                {
                    { key, value }
                };
                SetDestination setDestination = new SetDestination(".txt");

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            [TestCase(Keys.DestinationPath, "OtherFolder/foo.bar", "OtherFolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "foo.bar", "Subfolder/foo.bar")]
            [TestCase(Keys.DestinationFileName, "fizz/foo.bar", "Subfolder/fizz/foo.bar")]
            [TestCase(Keys.DestinationFileName, "../fizz/foo.bar", "fizz/foo.bar")]
            [TestCase(Keys.DestinationExtension, "foo", "Subfolder/write-test.foo")]
            [TestCase(Keys.DestinationExtension, ".foo", "Subfolder/write-test.foo")]
            public async Task SetsDestinationFromMetadataOverridesPageExtension(string key, string value, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"))
                {
                    { key, value }
                };
                SetDestination setDestination = new SetDestination(true);

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            [TestCase(Keys.DestinationPath)]
            [TestCase(Keys.DestinationFileName)]
            [TestCase(Keys.DestinationExtension)]
            public async Task SetsDestinationFromDelegateOverridesMetadata(string key)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"))
                {
                    { key, "foo" }
                };
                SetDestination setDestination = new SetDestination(
                    Config.FromDocument(doc => doc.Destination.ChangeExtension(".bar")),
                    true);

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("Subfolder/write-test.bar");
            }

            [TestCase(Keys.DestinationPath, "Subfolder/bar.baz", "Subfolder/bar.baz")]
            [TestCase(Keys.DestinationFileName, "fizz.buzz", "Subfolder/fizz.buzz")]
            [TestCase(Keys.DestinationExtension, "baz", "Subfolder/write-test.baz")]
            public async Task SetsDestinationFromDelegateDoesNotOverrideMetadata(string key, string value, string expected)
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"))
                {
                    { key, value }
                };
                SetDestination setDestination = new SetDestination(
                    Config.FromDocument(doc => doc.Destination.ChangeExtension(".bar")));

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(expected);
            }

            public async Task SetsDestinationFromDocumentDelegate()
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination(
                    Config.FromDocument(doc => doc.Destination.ChangeExtension(".bar")));

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("Subfolder/write-test.bar");
            }

            public async Task SetsDestinationFromStringDelegate()
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination("foo/bar.txt");

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("foo/bar.txt");
            }

            [Test]
            public async Task ExtensionWithDot()
            {
                // Given
                TestDocument input = new TestDocument(new NormalizedPath("Subfolder/write-test.abc"));
                SetDestination setDestination = new SetDestination(".txt");

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("Subfolder/write-test.txt");
            }

            [Test]
            public async Task ShouldWriteDotFile()
            {
                // Given
                TestDocument input = new TestDocument();
                SetDestination setDestination = new SetDestination((NormalizedPath)".dotfile");

                // When
                TestDocument result = await ExecuteAsync(input, setDestination).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe(".dotfile");
            }
        }
    }
}
