using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.YouTube.Tests
{
    [TestFixture]
    public class ReadYouTubeFixture : BaseFixture
    {
        public class ExecuteTests : ReadYouTubeFixture
        {
            [Test]
            public async Task SetsMetadata()
            {
                // Given
                TestDocument document = new TestDocument();
                IModule youtube = new ReadYouTube("abcd")
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, youtube).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }
        }
    }
}