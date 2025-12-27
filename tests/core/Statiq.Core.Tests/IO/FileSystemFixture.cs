using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.IO
{
    [TestFixture]
    public class FileSystemFixture : BaseFixture
    {
        public class ConstructorTests : FileSystemFixture
        {
            [Test]
            public void AddsDefaultInputPath()
            {
                // Given, When
                FileSystem fileSystem = new FileSystem();

                // Then
                Assert.That(fileSystem.InputPaths.Select(x => x.FullPath), Is.EquivalentTo(new[] { "input" }));
            }
        }

        public class RootPathTests : FileSystemFixture
        {
            [Test]
            public void SetThrowsForNullValue()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.RootPath = null);
            }

            [Test]
            public void SetThrowsForRelativePath()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentException>(() => fileSystem.RootPath = "foo");
            }

            [Test]
            public void CanSet()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When
                fileSystem.RootPath = "/foo/bar";

                // Then
                Assert.That(fileSystem.RootPath.FullPath, Is.EqualTo("/foo/bar"));
            }
        }

        public class OutputPathTests : FileSystemFixture
        {
            [Test]
            public void SetThrowsForNullValue()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Assert.Throws<ArgumentNullException>(() => fileSystem.OutputPath = null);
            }

            [Test]
            public void CanSet()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When
                fileSystem.OutputPath = "/foo/bar";

                // Then
                Assert.That(fileSystem.OutputPath.FullPath, Is.EqualTo("/foo/bar"));
            }
        }

        public class InputPathMappingTests : FileSystemFixture
        {
            [Test]
            public void CanAddInputPathMapping()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When
                fileSystem.InputPathMappings.Add("foo", "bar");

                // Then
                fileSystem.InputPathMappings.ShouldContainKeyAndValue("foo", "bar");
            }

            [Test]
            public void AddingAbsoluteMappingThrows()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Should.Throw<ArgumentException>(() => fileSystem.InputPathMappings.Add("foo", "/bar"));
            }

            [Test]
            public void AddingAbsoluteMappingViaIndexerThrows()
            {
                // Given
                FileSystem fileSystem = new FileSystem();

                // When, Then
                Should.Throw<ArgumentException>(() => fileSystem.InputPathMappings["foo"] = "/bar");
            }
        }
    }
}
