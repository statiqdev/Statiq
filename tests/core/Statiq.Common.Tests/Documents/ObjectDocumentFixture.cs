using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class ObjectDocumentFixture : BaseFixture
    {
        public class ConstructorTests : ObjectDocumentFixture
        {
            [Test]
            public void IdIsNotTheSameForDifferentDocuments()
            {
                // Given, When
                CustomObject obj = new CustomObject();
                IDocument a = new ObjectDocument<CustomObject>(obj);
                IDocument b = new ObjectDocument<CustomObject>(obj);

                // Then
                Assert.That(a.Id, Is.Not.EqualTo(b.Id));
            }
        }

        public class CloneTests : ObjectDocumentFixture
        {
            [Test]
            public void IdIsTheSameAfterClone()
            {
                // Given
                CustomObject obj = new CustomObject();
                IDocument document = new ObjectDocument<CustomObject>(obj);

                // When
                IDocument cloned = document.Clone(null);

                // Then
                Assert.That(cloned.Id, Is.EqualTo(document.Id));
            }

            [Test]
            public void DocumentTypeTheSameAfterClone()
            {
                // Given
                CustomObject obj = new CustomObject();
                IDocument document = new ObjectDocument<CustomObject>(obj);

                // When
                IDocument cloned = document.Clone(null);

                // Then
                Assert.That(cloned, Is.TypeOf<ObjectDocument<CustomObject>>());
            }

            [Test]
            public void MembersAreCloned()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                IDocument document = new ObjectDocument<CustomObject>(obj);

                // When
                IDocument cloned = document.Clone(null);

                // Then
                Assert.That(((ObjectDocument<CustomObject>)cloned).Object.Foo, Is.EqualTo("abc"));
            }
        }

        public class MetadataTests : ObjectDocumentFixture
        {
            [Test]
            public void DoesNotIncludeDocumentProperties()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                MetadataItems settings = new MetadataItems
                {
                    { "A", "a" }
                };

                // When
                IDocument document = new ObjectDocument<CustomObject>(obj, new Metadata(settings));

                // Then
                Assert.That(document.Keys, Is.EquivalentTo(new[] { "Foo", "A" }));
            }

            [Test]
            public void MetadataOverwritesSettings()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                MetadataItems settings = new MetadataItems
                {
                    { "A", "a" }
                };
                IDocument document = new ObjectDocument<CustomObject>(obj, new Metadata(settings));
                IDocument cloned = document.Clone(new MetadataItems { { "A", "b" } });

                // When
                string initialValue = document.GetString("A");
                string clonedValue = cloned.GetString("A");

                // Then
                Assert.That(initialValue, Is.EqualTo("a"));
                Assert.That(clonedValue, Is.EqualTo("b"));
            }

            [Test]
            public void GetsPropertyMetadata()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                IDocument document = new ObjectDocument<CustomObject>(obj);
                IDocument cloned = document.Clone(new MetadataItems { { "Foo", "xyz" } });

                // When
                string initialValue = document.GetString("Foo");
                string clonedValue = cloned.GetString("Foo");

                // Then
                Assert.That(initialValue, Is.EqualTo("abc"));
                Assert.That(clonedValue, Is.EqualTo("xyz"));
            }
        }

        public class CountTests : ObjectDocumentFixture
        {
            [Test]
            public void GetsCorrectCount()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", "a" },
                    { "B", "b" },
                    { "C", "c" }
                };
                IDocument document = new ObjectDocument<CustomObject>(obj, initialMetadata);

                // When
                int count = document.Count;

                // Then
                Assert.That(count, Is.EqualTo(4));
            }

            [Test]
            public void GetsCorrectCountWithProperty()
            {
                // Given
                CustomObject obj = new CustomObject
                {
                    Foo = "abc"
                };
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", "a" },
                    { "B", "b" },
                    { "C", "c" },
                    { "Foo", "foo" }
                };
                IDocument document = new ObjectDocument<CustomObject>(obj, initialMetadata);

                // When
                int count = document.Count;

                // Then
                Assert.That(count, Is.EqualTo(4));
            }
        }

        private class CustomObject
        {
            public string Foo { get; set; }
        }
    }
}