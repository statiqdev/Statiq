using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class ToLookupExtensionsFixture : BaseFixture
    {
        public class ToLookupManyTests : ToLookupExtensionsFixture
        {
            [Test]
            public void ReturnsCorrectLookupOfInt()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { 1, 2, 3 } }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { 2, 3, 4 } }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { 3 } }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { 4 } }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, IDocument> lookup = documents.ToLookupMany<int>("Numbers");

                // Then
                Assert.That(lookup.Count, Is.EqualTo(4));
                Assert.That(lookup[1], Is.EquivalentTo(new[] { a }));
                Assert.That(lookup[2], Is.EquivalentTo(new[] { a, b }));
                Assert.That(lookup[3], Is.EquivalentTo(new[] { a, b, c }));
                Assert.That(lookup[4], Is.EquivalentTo(new[] { b, d }));
            }

            [Test]
            public void ReturnsCorrectLookupOfString()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { "1", "2", "3" } }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { "2", "3", "4" } }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { "3" } }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { "4" } }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<string, IDocument> lookup = documents.ToLookupMany<string>("Numbers");

                // Then
                Assert.That(lookup.Count, Is.EqualTo(4));
                Assert.That(lookup["1"], Is.EquivalentTo(new[] { a }));
                Assert.That(lookup["2"], Is.EquivalentTo(new[] { a, b }));
                Assert.That(lookup["3"], Is.EquivalentTo(new[] { a, b, c }));
                Assert.That(lookup["4"], Is.EquivalentTo(new[] { b, d }));
            }

            [Test]
            public void ReturnsCorrectLookupWithValues()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { 1, 2, 3 } },
                    { "Colors", "Red" }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { 2, 3, 4 } },
                    { "Colors", "Red" }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { 3 } },
                    { "Colors", "Green" }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { 4 } },
                    { "Colors", "Green" }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, string> lookup = documents.ToLookupMany<int, string>("Numbers", "Colors");

                // Then
                Assert.That(lookup.Count, Is.EqualTo(4));
                Assert.That(lookup[1], Is.EquivalentTo(new[] { "Red" }));
                Assert.That(lookup[2], Is.EquivalentTo(new[] { "Red", "Red" }));
                Assert.That(lookup[3], Is.EquivalentTo(new[] { "Red", "Red", "Green" }));
                Assert.That(lookup[4], Is.EquivalentTo(new[] { "Red", "Green" }));
            }
        }

        public class ToLookupManyToManyTests : ToLookupExtensionsFixture
        {
            [Test]
            public void ReturnsCorrectLookupWithValues()
            {
                // Given
                IDocument a = new TestDocument("a")
                {
                    { "Numbers", new[] { 1, 2, 3 } },
                    { "Colors", new[] { "Red" } }
                };
                IDocument b = new TestDocument("b")
                {
                    { "Numbers", new[] { 2, 3, 4 } },
                    { "Colors", new[] { "Red", "Blue" } }
                };
                IDocument c = new TestDocument("c")
                {
                    { "Numbers", new[] { 3 } },
                    { "Colors", new[] { "Green" } }
                };
                IDocument d = new TestDocument("d")
                {
                    { "Numbers", new[] { 4 } },
                    { "Colors", new[] { "Green", "Blue" } }
                };
                List<IDocument> documents = new List<IDocument>() { a, b, c, d };

                // When
                ILookup<int, string> lookup = documents.ToLookupManyToMany<int, string>("Numbers", "Colors");

                // Then
                Assert.That(lookup.Count, Is.EqualTo(4));
                Assert.That(lookup[1], Is.EquivalentTo(new[] { "Red" }));
                Assert.That(lookup[2], Is.EquivalentTo(new[] { "Red", "Red", "Blue" }));
                Assert.That(lookup[3], Is.EquivalentTo(new[] { "Red", "Red", "Blue", "Green" }));
                Assert.That(lookup[4], Is.EquivalentTo(new[] { "Red", "Blue", "Green", "Blue" }));
            }
        }
    }
}
