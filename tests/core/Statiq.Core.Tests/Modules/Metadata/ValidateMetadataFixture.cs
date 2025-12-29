using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class ValidateMetadataFixture : BaseFixture
    {
        public class ExecuteTests : ValidateMetadataFixture
        {
            [Test]
            public async Task ExistenceOfKeyDoesNotThrow()
            {
                // Given
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                ValidateMetadata<string> validateMeta = new ValidateMetadata<string>("Title");

                // When, Then
                await Should.NotThrowAsync(async () => await ExecuteAsync(document, validateMeta));
            }

            [Test]
            public async Task AbsenceOfKeyThrows()
            {
                // Given
                TestDocument document = new TestDocument();
                ValidateMetadata<string> validateMeta = new ValidateMetadata<string>("Title");

                // When, Then
                await Should.ThrowAsync<AggregateException>(async () => await ExecuteAsync(document, validateMeta));
            }

            [Test]
            public async Task FailedAssertionThrows()
            {
                // Given
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                ValidateMetadata<string> validateMeta = new ValidateMetadata<string>("Title").WithAssertion(x => x == "Baz");

                // When, Then
                await Should.ThrowAsync<AggregateException>(async () => await ExecuteAsync(document, validateMeta));
            }

            [Test]
            public async Task PassedAssertionDoesNotThrow()
            {
                // Given
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "Title", "Foo" }
                    });
                ValidateMetadata<string> validateMeta = new ValidateMetadata<string>("Title").WithAssertion(x => x == "Foo");

                // When, Then
                await Should.NotThrowAsync(async () => await ExecuteAsync(document, validateMeta));
            }
        }
    }
}