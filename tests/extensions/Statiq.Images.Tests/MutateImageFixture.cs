using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Images.Tests
{
    [TestFixture]
    public class MutateImageFixture : BaseFixture
    {
        public class ExecuteTests : MutateImageFixture
        {
            [Test]
            public async Task OutputsTheSameAsInput()
            {
                // Given
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/a/b/test.png"),
                    fileStream);
                MutateImage module = new MutateImage();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("a/b/test.png");
            }

            [Test]
            public async Task ChangesPathWhenOutputFormatSpecified()
            {
                // Given
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/a/b/test.png"),
                    fileStream);
                MutateImage module = new MutateImage().OutputAsGif();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("a/b/test.gif");
            }

            [Test]
            public async Task ChangesPathWhenBrightnessSpecified()
            {
                // Given
                MemoryStream fileStream = GetTestFileStream("logo-square.png");
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/a/b/test.png"),
                    fileStream);
                MutateImage module = new MutateImage().Brightness(123);

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Destination.FullPath.ShouldBe("a/b/test-b123.png");
            }
        }
    }
}
