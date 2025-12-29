using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Execution
{
    [TestFixture]
    public class NamespaceCollectionFixture : BaseFixture
    {
        public class AddTests : NamespaceCollectionFixture
        {
            [TestCase((string)null)]
            [TestCase("")]
            [TestCase(" ")]
            public void ThrowsForNullOrWhiteSpaceNamespace(string ns)
            {
                // Given
                NamespaceCollection collection = new NamespaceCollection();

                // When, Then
                Should.Throw<Exception>(() => collection.Add(ns));
            }
        }

        public class AddRangeTests : NamespaceCollectionFixture
        {
            [TestCase((string)null)]
            [TestCase("")]
            [TestCase(" ")]
            public void ThrowsForNullOrWhiteSpaceNamespace(string ns)
            {
                // Given
                NamespaceCollection collection = new NamespaceCollection();

                // When, Then
                Should.Throw<Exception>(() => collection.AddRange(new[] { ns, "Foo" }));
            }
        }
    }
}
