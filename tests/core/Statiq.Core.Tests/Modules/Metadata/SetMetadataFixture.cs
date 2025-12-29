using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class SetMetadataFixture : BaseFixture
    {
        public class ExecuteTests : SetMetadataFixture
        {
            [Test]
            public async Task AddsMetadata()
            {
                // Given
                TestDocument input = new TestDocument();
                SetMetadata module = new SetMetadata("Foo", "Bar");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Single()["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task ReplacesMetadata()
            {
                // Given
                TestDocument input = new TestDocument
                {
                    { "Foo", "Baz" }
                };
                SetMetadata module = new SetMetadata("Foo", "Bar");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Single()["Foo"].ShouldBe("Bar");
            }

            [Test]
            public async Task OnlyIfNonExisting()
            {
                // Given
                TestDocument input = new TestDocument
                {
                    { "Foo", "Baz" }
                };
                SetMetadata module = new SetMetadata("Foo", "Bar").OnlyIfNonExisting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Single()["Foo"].ShouldBe("Baz");
            }

            [Test]
            public async Task AddsNull()
            {
                // Given
                TestDocument input = new TestDocument();
                SetMetadata module = new SetMetadata("Foo", (string)null);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Single()["Foo"].ShouldBe((string)null);
            }

            [Test]
            public async Task IgnoresNull()
            {
                // Given
                TestDocument input = new TestDocument();
                SetMetadata module = new SetMetadata("Foo", (string)null).IgnoreNull();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Single().Keys.ShouldNotContain("Foo");
            }
        }
    }
}
