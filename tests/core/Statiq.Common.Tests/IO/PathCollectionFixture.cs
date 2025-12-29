using System;
using NUnit.Framework;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class PathCollectionFixture : BaseFixture
    {
        private readonly NormalizedPath _upperCaseA;
        private readonly NormalizedPath _lowerCaseA;
        private readonly NormalizedPath _upperCaseB;
        private readonly NormalizedPath _lowerCaseB;
        private readonly NormalizedPath _upperCaseC;
        private readonly NormalizedPath _lowerCaseC;

        public PathCollectionFixture()
        {
            if (typeof(NormalizedPath) == typeof(NormalizedPath))
            {
                _upperCaseA = new NormalizedPath("A");
                _lowerCaseA = new NormalizedPath("a");
                _upperCaseB = new NormalizedPath("B");
                _lowerCaseB = new NormalizedPath("b");
                _upperCaseC = new NormalizedPath("C");
                _lowerCaseC = new NormalizedPath("c");
            }
            else if (typeof(NormalizedPath) == typeof(NormalizedPath))
            {
                _upperCaseA = new NormalizedPath("A.txt");
                _lowerCaseA = new NormalizedPath("a.txt");
                _upperCaseB = new NormalizedPath("B.txt");
                _lowerCaseB = new NormalizedPath("b.txt");
                _upperCaseC = new NormalizedPath("C.txt");
                _lowerCaseC = new NormalizedPath("c.txt");
            }
            else
            {
                throw new InvalidOperationException("Need to specify test paths for generic type");
            }
        }

        public class CountTests : PathCollectionFixture
        {
            [Test]
            public void ShouldReturnTheNumberOfPathsInTheCollection()
            {
                // Given
                PathCollection collection = new PathCollection(new[] { _upperCaseA, _upperCaseB });

                // When, Then
                Assert.That(collection.Count, Is.EqualTo(2));
            }
        }

        public class AddTests : PathCollectionFixture
        {
            [Test]
            public void ShouldAddPathIfNotAlreadyPresent()
            {
                // Given
                PathCollection collection = new PathCollection();
                collection.Add(_upperCaseB);

                // When
                collection.Add(_upperCaseA);

                // Then
                Assert.That(collection.Count, Is.EqualTo(2));
            }
        }

        public class AddRangeTests : PathCollectionFixture
        {
            [Test]
            public void ShouldAddPathsThatAreNotPresent()
            {
                // Given
                PathCollection collection = new PathCollection(
                    new[] { _upperCaseA, _upperCaseB });

                // When
                collection.AddRange(new[] { _upperCaseA, _upperCaseB, _upperCaseC });

                // Then
                Assert.That(collection.Count, Is.EqualTo(3));
            }
        }
    }
}
