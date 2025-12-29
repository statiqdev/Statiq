using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class SetContentFixture : BaseFixture
    {
        public class ExecuteTests : SetContentFixture
        {
            [Test]
            public async Task ReplacesContent()
            {
                // Given
                TestDocument document = new TestDocument("Original");
                SetContent module = new SetContent("Replaced");

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe("Replaced");
            }

            [Test]
            public async Task KeepsOriginalMediaType()
            {
                // Given
                TestDocument document = new TestDocument("Original", "Foo");
                SetContent module = new SetContent("Replaced");

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.ContentProvider.MediaType.ShouldBe("Foo");
            }

            [Test]
            public async Task ChangesMediaType()
            {
                // Given
                TestDocument document = new TestDocument("Original", "Foo");
                SetContent module = new SetContent("Replaced", "Bar");

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.ContentProvider.MediaType.ShouldBe("Bar");
            }

            [Test]
            public async Task ReplacesContentWithEmptyString()
            {
                // Given
                TestDocument document = new TestDocument("Original");
                SetContent module = new SetContent(string.Empty);

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBeEmpty();
            }

            [Test]
            public async Task OutputsOriginalDocumentForNullContent()
            {
                // Given
                TestDocument document = new TestDocument("Original");
                SetContent module = new SetContent((string)null);

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.ShouldBe(document);
            }
        }
    }
}
