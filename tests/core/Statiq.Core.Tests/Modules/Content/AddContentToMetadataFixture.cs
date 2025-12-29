using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class AddContentToMetadataFixture : BaseFixture
    {
        public class ExecuteTests : ReplaceInContentFixture
        {
            [Test]
            public async Task AddsContent()
            {
                // Given
                TestDocument input = new TestDocument();
                AddContentToMetadata addContent = new AddContentToMetadata("Foo")
                {
                    new ExecuteConfig("Bar")
                };

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, addContent);

                // Then
                results.Single()["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task AddsContentArray()
            {
                // Given
                TestDocument input = new TestDocument();
                AddContentToMetadata addContent = new AddContentToMetadata("Foo")
                {
                    new ExecuteConfig(new[] { "Bar", "Baz" })
                };

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, addContent);

                // Then
                results.Single()["Foo"].ShouldBe(new[] { "Bar", "Baz" });
            }

            [Test]
            public async Task AddsContentFromDocument()
            {
                // Given
                TestDocument input = new TestDocument();
                TestDocument content = new TestDocument("Fuzz");
                AddContentToMetadata addContent = new AddContentToMetadata("Foo")
                {
                    new ExecuteConfig(content)
                };

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, addContent);

                // Then
                results.Single()["Foo"].ShouldBe("Fuzz");
            }
        }
    }
}
