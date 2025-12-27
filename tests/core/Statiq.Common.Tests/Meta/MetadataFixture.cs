using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    public class MetadataFixture : BaseFixture
    {
        public class IndexerTests : MetadataFixture
        {
            [Test]
            public void MissingKeyThrowsKeyNotFoundException()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                TestDelegate test = () =>
                {
                    object value = metadata["A"];
                };

                // Then
                Assert.Throws<KeyNotFoundException>(test);
            }

            [Test]
            public void NullKeyThrowsKeyNotFoundException()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                TestDelegate test = () =>
                {
                    object value = metadata[null];
                };

                // Then
                Assert.Throws<ArgumentNullException>(test);
            }

            [Test]
            public void ReturnsCorrectResultWithMetadataValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new SimpleMetadataValue { Value = "a" } } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata["A"];

                // Then
                Assert.That(value, Is.EqualTo("a"));
            }

            [Test]
            public void ReturnsCorrectResultForKeysWithDifferentCase()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new SimpleMetadataValue { Value = "a" } } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata["a"];

                // Then
                Assert.That(value, Is.EqualTo("a"));
            }
        }

        public class ContainsKeyTests : MetadataFixture
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("A");

                // Then
                Assert.That(contains, Is.True);
            }

            [Test]
            public void ReturnsFalseForInvalidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("B");

                // Then
                Assert.That(contains, Is.False);
            }

            [Test]
            public void ReturnsTrueForSameKeysWithDifferentCase()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                bool contains = metadata.ContainsKey("a");

                // Then
                Assert.That(contains, Is.True);
            }
        }

        public class TryGetValueTests : MetadataFixture
        {
            [Test]
            public void ReturnsTrueForValidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("A", out value);

                // Then
                Assert.That(contains, Is.True);
                Assert.That(value, Is.EqualTo("a"));
            }

            [Test]
            public void ReturnsFalseForInvalidValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("B", out value);

                // Then
                Assert.That(contains, Is.False);
                Assert.That(value, Is.Null);
            }

            [Test]
            public void ReturnsCorrectResultWithMetadataValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new SimpleMetadataValue { Value = "a" } } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object value;
                bool contains = metadata.TryGetValue("A", out value);

                // Then
                Assert.That(contains, Is.True);
                Assert.That(value, Is.EqualTo("a"));
            }
        }

        public class CloneTests : MetadataFixture
        {
            [Test]
            public void CanCloneWithNewValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                Metadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", "a") });

                // Then
                Assert.That(metadata["A"], Is.EqualTo("a"));
            }

            [Test]
            public void ContainsPreviousValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.That(clone["A"], Is.EqualTo("a"));
            }

            [Test]
            public void ClonedMetadataDoesNotContainNewValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.That(metadata.ContainsKey("B"), Is.False);
            }

            [Test]
            public void ContainsNewValues()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "B", "b" } });

                // Then
                Assert.That(clone["B"], Is.EqualTo("b"));
            }

            [Test]
            public void ReplacesValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                Metadata clone = new Metadata(metadata, new Dictionary<string, object> { { "A", "b" } });

                // Then
                Assert.That(metadata["A"], Is.EqualTo("a"));
                Assert.That(clone["A"], Is.EqualTo("b"));
            }
        }

        public class GetTests : MetadataFixture
        {
            [Test]
            public void GetWithMetadataValueReturnsCorrectResult()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", "a" } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");

                // Then
                Assert.That(value, Is.EqualTo("a"));
            }

            [Test]
            public void ReturnsCorrectResultWithDerivedMetadataValue()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new DerivedMetadataValue { Key = "X" } }, { "X", "x" }
                };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");

                // Then
                Assert.That(value, Is.EqualTo("x"));
            }

            [Test]
            public void MetadataValueCalledForEachRequest()
            {
                // Given
                SimpleMetadataValue metadataValue = new SimpleMetadataValue { Value = "a" };
                MetadataItems initialMetadata = new MetadataItems { { "A", metadataValue } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                object value = metadata.Get("A");
                value = metadata.Get("A");
                value = metadata.Get("A");

                // Then
                Assert.That(value, Is.EqualTo("a"));
                Assert.That(metadataValue.Calls, Is.EqualTo(3));
            }
        }

        public class GetListTests : MetadataFixture
        {
            [Test]
            public void ReturnsCorrectResultForList()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.GetList<int>("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
            }

            [Test]
            public void ReturnsCorrectResultForConvertedStringList()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<string> { "1", "2", "3" } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.GetList<int>("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
            }

            [Test]
            public void ReturnsCorrectResultForConvertedIntList()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<string> result = metadata.GetList<string>("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(new[] { "1", "2", "3" }));
            }

            [Test]
            public void ReturnsCorrectResultForArray()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new[] { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IReadOnlyList<int> result = metadata.GetList<int>("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(new[] { 1, 2, 3 }));
            }
        }

        public class GetDocumentsTests : MetadataFixture
        {
            [Test]
            public void ReturnsNullWhenKeyNotFound()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IEnumerable<IDocument> result = metadata.GetDocuments("A");

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public void ReturnsListForList()
            {
                // Given
                IDocument a = new TestDocument();
                IDocument b = new TestDocument();
                IDocument c = new TestDocument();
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<IDocument> { a, b, c } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IEnumerable<IDocument> result = metadata.GetDocuments("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(new[] { a, b, c }));
            }

            [Test]
            public void ReturnsEmptyListForListOfInt()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IEnumerable<IDocument> result = metadata.GetDocuments("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ReturnsEmptyListForSingleInt()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", 1 } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                IEnumerable<IDocument> result = metadata.GetDocuments("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        public class GetDocumentListTests : MetadataFixture
        {
            [Test]
            public void ReturnsEmptyListWhenKeyNotFound()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                DocumentList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ReturnsListForList()
            {
                // Given
                IDocument a = new TestDocument();
                IDocument b = new TestDocument();
                IDocument c = new TestDocument();
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<IDocument> { a, b, c } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                DocumentList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(new[] { a, b, c }));
            }

            [Test]
            public void ReturnsEmptyListForListOfInt()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", new List<int> { 1, 2, 3 } } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                DocumentList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }

            [Test]
            public void ReturnsEmptyListForSingleInt()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems { { "A", 1 } };
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                DocumentList<IDocument> result = metadata.GetDocumentList("A");

                // Then
                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.Empty);
            }
        }

        public class GetStringTests : MetadataFixture
        {
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            public void ReturnsCorrectStringForFilePath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata,
                    new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                object result = metadata.GetString("A");

                // Then
                Assert.That(result, Is.InstanceOf<string>());
                Assert.That(result, Is.EqualTo(expected));
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            public void ReturnsCorrectStringForDirectoryPath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata,
                    new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                object result = metadata.GetString("A");

                // Then
                Assert.That(result, Is.InstanceOf<string>());
                Assert.That(result, Is.EqualTo(expected));
            }
        }

        public class GetPathTests : MetadataFixture
        {
            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            public void ReturnsCorrectFilePathForFilePath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata,
                    new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                NormalizedPath result = metadata.GetPath("A");

                // Then
                result.FullPath.ShouldBe(expected);
            }

            [TestCase("/a/b/c.txt", "/a/b/c.txt")]
            [TestCase("a/b/c.txt", "a/b/c.txt")]
            [TestCase(null, null)]
            public void ReturnsCorrectFilePathForString(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", path) });
                NormalizedPath result = metadata.GetPath("A");

                // Then
                if (expected is null)
                {
                    result.IsNull.ShouldBeTrue();
                }
                else
                {
                    result.FullPath.ShouldBe(expected);
                }
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            public void ReturnsCorrectDirectoryPathForDirectoryPath(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata,
                    new[] { new KeyValuePair<string, object>("A", new NormalizedPath(path)) });
                NormalizedPath result = metadata.GetPath("A");

                // Then
                result.FullPath.ShouldBe(expected);
            }

            [TestCase("/a/b/c", "/a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase(null, null)]
            public void ReturnsCorrectDirectoryPathForString(string path, string expected)
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems();
                IMetadata metadata = new Metadata(initialMetadata);

                // When
                metadata = new Metadata(metadata, new[] { new KeyValuePair<string, object>("A", path) });
                NormalizedPath result = metadata.GetPath("A");

                // Then
                if (expected is null)
                {
                    result.IsNull.ShouldBeTrue();
                }
                else
                {
                    result.FullPath.ShouldBe(expected);
                }
            }
        }

        public class EnumeratorTests : MetadataFixture
        {
            [Test]
            public void EnumeratingMetadataValuesReturnsCorrectResults()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "B", new SimpleMetadataValue { Value = "b" } },
                    { "C", new SimpleMetadataValue { Value = "c" } }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                object[] values = metadata.Select(x => x.Value).ToArray();

                // Then
                Assert.That(values, Is.EquivalentTo(new[] { "a", "b", "c" }));
            }
        }

        public class CountTests : MetadataFixture
        {
            [Test]
            public void GetsCorrectCount()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "B", new SimpleMetadataValue { Value = "b" } },
                    { "C", new SimpleMetadataValue { Value = "c" } }
                };
                Metadata metadata = new Metadata(initialMetadata);

                // When
                int count = metadata.Count;

                // Then
                count.ShouldBe(3);
            }

            [Test]
            public void GetsCorrectCountWithPrevious()
            {
                // Given
                MetadataItems initialMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "B", new SimpleMetadataValue { Value = "b" } },
                    { "C", new SimpleMetadataValue { Value = "c" } }
                };
                MetadataItems newMetadata = new MetadataItems
                {
                    { "A", new SimpleMetadataValue { Value = "a" } },
                    { "D", new SimpleMetadataValue { Value = "b" } },
                    { "E", new SimpleMetadataValue { Value = "c" } }
                };
                Metadata previous = new Metadata(initialMetadata);
                Metadata metadata = new Metadata(previous, newMetadata);

                // When
                int count = metadata.Count;

                // Then
                count.ShouldBe(5);
            }
        }

        private class SimpleMetadataValue : IMetadataValue
        {
            public object Value { get; set; }
            public int Calls { get; set; }

            object IMetadataValue.Get(string key, IMetadata metadata)
            {
                Calls++;
                return Value;
            }
        }

        private class DerivedMetadataValue : IMetadataValue
        {
            public string Key { get; set; }

            object IMetadataValue.Get(string key, IMetadata metadata)
            {
                return metadata[Key];
            }
        }
    }
}
