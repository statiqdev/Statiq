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
    public class AddDocumentsToMetadataFixture : BaseFixture
    {
        public class ExecuteTests : AddDocumentsToMetadataFixture
        {
            [Test]
            public async Task AddsDocument()
            {
                // Given
                TestDocument input = new TestDocument();
                TestDocument document = new TestDocument("Fuzz");
                AddDocumentsToMetadata addDocuments = new AddDocumentsToMetadata("Foo")
                {
                    new ExecuteConfig(document)
                };

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, addDocuments);

                // Then
                results.Single()["Foo"].ShouldBe(document);
            }

            [Test]
            public async Task AddsDocuments()
            {
                // Given
                TestDocument input = new TestDocument();
                TestDocument a = new TestDocument("A");
                TestDocument b = new TestDocument("B");
                AddDocumentsToMetadata addDocuments = new AddDocumentsToMetadata("Foo")
                {
                    new ExecuteConfig(new[] { a, b })
                };

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, addDocuments);

                // Then
                results.Single()["Foo"].ShouldBe(new[] { a, b });
            }
        }
    }
}
