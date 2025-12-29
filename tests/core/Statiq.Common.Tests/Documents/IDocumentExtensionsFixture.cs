using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class IDocumentExtensionsFixture : BaseFixture
    {
        public class AsDynamicTests : IDocumentExtensionsFixture
        {
            [Test]
            public void GetsDynamicObject()
            {
                // Given
                IDocument document = new TestDocument
                {
                    { "A", "a" },
                    { "B", 2 }
                };

                // When
                dynamic dynamicDocument = document.AsDynamic();

                // Then
                ((object)dynamicDocument.A).ShouldBe("a");
                ((object)dynamicDocument.B).ShouldBe(2);
            }

            [Test]
            public void DynamicObjectConvertsBackToDocument()
            {
                // Given
                IDocument document = new TestDocument
                {
                    { "A", "a" },
                    { "B", 2 }
                };

                // When
                IDocument dynamicDocument = (IDocument)document.AsDynamic();

                // Then
                dynamicDocument.GetString("A").ShouldBe("a");
                dynamicDocument["B"].ShouldBe(2);
            }

            [Test]
            public void GetsSource()
            {
                // Given
                IDocument document = new TestDocument(new NormalizedPath("/a/b/c.txt"));

                // When
                dynamic dynamicDocument = document.AsDynamic();

                // Then
                ((object)dynamicDocument.Source).ShouldBe(new NormalizedPath("/a/b/c.txt"));
            }

            [Test]
            public void ThrowsForNullDocument()
            {
                // Given, When, Then
                Should.Throw<ArgumentNullException>(() => ((IDocument)null).AsDynamic());
            }
        }
    }
}
