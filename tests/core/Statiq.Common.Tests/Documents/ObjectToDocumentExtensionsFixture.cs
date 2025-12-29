using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class ObjectToDocumentExtensionsFixture : BaseFixture
    {
        public class ToDocumentTests : ObjectToDocumentExtensionsFixture
        {
            [Test]
            public void GetsProperty()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                IDocument document = obj.ToDocument();

                // When
                object result = document["Foo"];

                // Then
                result.ShouldBe("abc");
            }

            [Test]
            public void GetsPropertyFromObjectType()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                IDocument document = ((object)obj).ToDocument();

                // When
                object result = document["Foo"];

                // Then
                result.ShouldBe("abc");
            }

            [Test]
            public void ReturnsSameDocument()
            {
                // Given
                TestDocument document = new TestDocument
                {
                    { "Foo", "abc" }
                };

                // When
                IDocument result = document.ToDocument();

                // Then
                result["Foo"].ShouldBe("abc");
                result.ShouldBe(document);
            }

            [Test]
            public void ClonesDocument()
            {
                // Given
                TestDocument document = new TestDocument()
                {
                    { "Foo", "abc" }
                };

                // When
                IDocument result = document.ToDocument(new MetadataItems
                {
                    { "Bar", "123" }
                });

                // Then
                result["Foo"].ShouldBe("abc");
                result["Bar"].ShouldBe("123");
                result.ShouldBeOfType<TestDocument>();
            }
        }

        private class CustomObject
        {
            public string Foo { get; set; }
        }
    }
}
