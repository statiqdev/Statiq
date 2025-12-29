using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class VirtualInputDirectoryFixture : BaseFixture
    {
        public class ConstructorTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ThrowsForNullFileSystem()
            {
                // Given, When, Then
                Assert.That(() => new VirtualInputDirectory(null, new NormalizedPath("A")), Throws.TypeOf<ArgumentNullException>());
            }

            [Test]
            public void ThrowsForNullDirectoryPath()
            {
                // Given, When, Then
                Assert.That(() => new VirtualInputDirectory(new TestFileSystem(), null), Throws.TypeOf<ArgumentNullException>());
            }

            [Test]
            public void ThrowsForNonRelativePath()
            {
                // Given, When, Then
                Assert.That(() => new VirtualInputDirectory(new TestFileSystem(), new NormalizedPath("/A")), Throws.TypeOf<ArgumentException>());
            }
        }

        public class GetDirectoriesTests : VirtualInputDirectoryFixture
        {
            [TestCase(".", SearchOption.AllDirectories, new[] { "c", "c/1", "d", "a", "a/b" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "c", "d", "a" })]
            public void RootVirtualDirectoryDoesNotIncludeSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                Assert.That(directories.Select(x => x.Path.FullPath), Is.EquivalentTo(expectedPaths));
            }

            [TestCase("a", SearchOption.AllDirectories, new[] { "a/b" })]
            [TestCase("a", SearchOption.TopDirectoryOnly, new[] { "a/b" })]
            public void NonRootVirtualDirectoryIncludesSelf(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                Assert.That(directories.Select(x => x.Path.FullPath), Is.EquivalentTo(expectedPaths));
            }

            [TestCase("", SearchOption.TopDirectoryOnly, new[] { "x", "i" })]
            [TestCase("", SearchOption.AllDirectories, new[] { "x", "x/y", "x/y/z", "i", "i/h" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "x", "i" })]
            [TestCase(".", SearchOption.AllDirectories, new[] { "x", "x/y", "x/y/z", "i", "i/h" })]
            [TestCase("x", SearchOption.TopDirectoryOnly, new[] { "x/y" })]
            [TestCase("x", SearchOption.AllDirectories, new[] { "x/y", "x/y/z" })]
            [TestCase("x/y", SearchOption.TopDirectoryOnly, new[] { "x/y/z" })]
            [TestCase("x/y", SearchOption.AllDirectories, new[] { "x/y/z" })]
            [TestCase("x/y/z", SearchOption.TopDirectoryOnly, new string[] { })]
            [TestCase("x/y/z", SearchOption.AllDirectories, new string[] { })]
            [TestCase("q", SearchOption.TopDirectoryOnly, new string[] { })]
            [TestCase("q", SearchOption.AllDirectories, new string[] { })]
            public void GetsMappedDirectories(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetMappedVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetDirectories(searchOption);

                // Then
                directories.Select(x => x.Path.FullPath).ShouldBe(expectedPaths, true);
            }
        }

        public class GetExistingInputDirectoriesTests : VirtualInputDirectoryFixture
        {
            [TestCase("", new[] { "/root/a", "/root/b", "x", "i" })]
            [TestCase(".", new[] { "/root/a", "/root/b", "x", "i" })]
            [TestCase("x", new[] { "/root/a/x", "/root/b/x", "/root/d/e", "x/y" })] // Includes a virtual x/y directory
            [TestCase("x/y", new[] { "/root/a/x/y", "/root/c", "/root/d/e/y" })]
            [TestCase("x/y/z", new[] { "/root/a/x/y/z", "/root/c/z", "/root/d/e/y/z" })]
            public void GetsMappedDirectories(string virtualPath, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetMappedVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IDirectory> directories = directory.GetExistingInputDirectories();

                // Then
                directories.Select(x => x.Path.FullPath).ShouldBe(expectedPaths, true);
            }
        }

        public class GetFilesTests : VirtualInputDirectoryFixture
        {
            [TestCase(".", SearchOption.AllDirectories, new[] { "/a/b/c/foo.txt", "/a/b/c/baz.txt", "/a/b/c/1/2.txt", "/a/b/d/baz.txt", "/foo/baz.txt", "/foo/c/baz.txt" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new[] { "/foo/baz.txt" })]
            [TestCase("c", SearchOption.AllDirectories, new[] { "/a/b/c/foo.txt", "/a/b/c/baz.txt", "/a/b/c/1/2.txt", "/foo/c/baz.txt" })]
            [TestCase("c", SearchOption.TopDirectoryOnly, new[] { "/a/b/c/foo.txt", "/a/b/c/baz.txt", "/foo/c/baz.txt" })]
            public void GetsFiles(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IFile> files = directory.GetFiles(searchOption);

                // Then
                Assert.That(files.Select(x => x.Path.FullPath), Is.EquivalentTo(expectedPaths));
            }

            [TestCase("", SearchOption.TopDirectoryOnly, new string[] { })]
            [TestCase("", SearchOption.AllDirectories, new[] { "/root/a/x/y/z/foo.txt", "/root/b/x/bar.txt", "/root/c/z/fizz.txt", "/root/d/e/y/z/buzz.txt", "/f/g/h/bazz.txt" })]
            [TestCase(".", SearchOption.TopDirectoryOnly, new string[] { })]
            [TestCase(".", SearchOption.AllDirectories, new[] { "/root/a/x/y/z/foo.txt", "/root/b/x/bar.txt", "/root/c/z/fizz.txt", "/root/d/e/y/z/buzz.txt", "/f/g/h/bazz.txt" })]
            [TestCase("x", SearchOption.TopDirectoryOnly, new[] { "/root/b/x/bar.txt" })]
            [TestCase("x", SearchOption.AllDirectories, new[] { "/root/a/x/y/z/foo.txt", "/root/b/x/bar.txt", "/root/c/z/fizz.txt", "/root/d/e/y/z/buzz.txt" })]
            [TestCase("x/y", SearchOption.TopDirectoryOnly, new string[] { })]
            [TestCase("x/y", SearchOption.AllDirectories, new[] { "/root/a/x/y/z/foo.txt", "/root/c/z/fizz.txt", "/root/d/e/y/z/buzz.txt" })]
            [TestCase("x/y/z", SearchOption.TopDirectoryOnly, new[] { "/root/a/x/y/z/foo.txt", "/root/c/z/fizz.txt", "/root/d/e/y/z/buzz.txt" })]
            [TestCase("x/y/z", SearchOption.AllDirectories, new[] { "/root/a/x/y/z/foo.txt", "/root/c/z/fizz.txt", "/root/d/e/y/z/buzz.txt" })]
            [TestCase("q", SearchOption.TopDirectoryOnly, new string[] { })]
            [TestCase("q", SearchOption.AllDirectories, new string[] { })]
            public void GetsMappedFiles(string virtualPath, SearchOption searchOption, string[] expectedPaths)
            {
                // Given
                VirtualInputDirectory directory = GetMappedVirtualInputDirectory(virtualPath);

                // When
                IEnumerable<IFile> files = directory.GetFiles(searchOption);

                // Then
                files.Select(x => x.Path.FullPath).ShouldBe(expectedPaths, true);
            }
        }

        public class GetFileTests : VirtualInputDirectoryFixture
        {
            [TestCase(".", "c/foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase(".", "baz.txt", "/foo/baz.txt", true)]
            [TestCase("c", "foo.txt", "/a/b/c/foo.txt", true)]
            [TestCase("c", "1/2.txt", "/a/b/c/1/2.txt", true)]
            [TestCase("c", "1/3.txt", "/foo/c/1/3.txt", false)]
            [TestCase("c", "baz.txt", "/foo/c/baz.txt", true)]
            [TestCase("c", "bar.txt", "/foo/c/bar.txt", false)]
            [TestCase("x/y/z", "bar.txt", "/foo/x/y/z/bar.txt", false)]
            public void GetsInputFile(string virtualPath, string filePath, string expectedPath, bool expectedExists)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IFile file = directory.GetFile(filePath);

                // Then
                Assert.That(file.Path.FullPath, Is.EqualTo(expectedPath));
                Assert.That(file.Exists, Is.EqualTo(expectedExists));
            }

            [TestCase("", "x/y/z/foo.txt", "/root/a/x/y/z/foo.txt", true)]
            [TestCase("", "x/y/z/buzz.txt", "/root/d/e/y/z/buzz.txt", true)]
            [TestCase(".", "x/y/z/foo.txt", "/root/a/x/y/z/foo.txt", true)]
            [TestCase(".", "x/y/z/buzz.txt", "/root/d/e/y/z/buzz.txt", true)]
            [TestCase("x", "y/z/foo.txt", "/root/a/x/y/z/foo.txt", true)]
            [TestCase("x", "y/z/buzz.txt", "/root/d/e/y/z/buzz.txt", true)]
            [TestCase("x", "qwerty.txt", "/root/d/e/qwerty.txt", false)]
            [TestCase("x/y", "z/foo.txt", "/root/a/x/y/z/foo.txt", true)]
            [TestCase("x/y", "z/buzz.txt", "/root/d/e/y/z/buzz.txt", true)]
            [TestCase("x/y", "qwerty.txt", "/root/d/e/y/qwerty.txt", false)]
            [TestCase("x/y/z", "foo.txt", "/root/a/x/y/z/foo.txt", true)]
            [TestCase("x/y/z", "buzz.txt", "/root/d/e/y/z/buzz.txt", true)]
            [TestCase("x/y/z", "qwerty.txt", "/root/d/e/y/z/qwerty.txt", false)]
            [TestCase("", "i/h/bazz.txt", "/f/g/h/bazz.txt", true)]
            [TestCase(".", "i/h/bazz.txt", "/f/g/h/bazz.txt", true)]
            [TestCase("i", "h/bazz.txt", "/f/g/h/bazz.txt", true)]
            [TestCase("i/h", "bazz.txt", "/f/g/h/bazz.txt", true)]
            public void GetsMappedInputFile(string virtualPath, string filePath, string expectedPath, bool expectedExists)
            {
                // Given
                VirtualInputDirectory directory = GetMappedVirtualInputDirectory(virtualPath);

                // When
                IFile file = directory.GetFile(filePath);

                // Then
                Assert.That(file.Path.FullPath, Is.EqualTo(expectedPath));
                Assert.That(file.Exists, Is.EqualTo(expectedExists));
            }

            [Test]
            public void GetsInputFileAboveInputDirectory()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("/foo");
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When
                IFile file = directory.GetFile("../c/foo.txt");

                // Then
                Assert.That(file.Path.FullPath, Is.EqualTo("/a/b/c/foo.txt"));
            }

            [Test]
            public void GetsMappedInputFileAboveInputDirectory()
            {
                // Given
                VirtualInputDirectory directory = GetMappedVirtualInputDirectory("x/y/q/w");

                // When
                IFile file = directory.GetFile("../../z/fizz.txt");

                // Then
                Assert.That(file.Path.FullPath, Is.EqualTo("/root/c/z/fizz.txt"));
            }

            [Test]
            public void FileGetsVirtualParent()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory("c");

                // When
                IFile file = directory.GetFile("baz.txt");

                // Then
                IDirectory parent = file.Directory;
                parent
                    .GetFiles(SearchOption.TopDirectoryOnly)
                    .Select(x => x.Path.FullPath)
                    .ShouldBe(new[] { "/a/b/c/foo.txt", "/a/b/c/baz.txt", "/foo/c/baz.txt" }, true);
            }

            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                Should.Throw<ArgumentNullException>(() => directory.GetFile(null));
            }

            [Test]
            public void ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                NormalizedPath filePath = "/a/test.txt";

                // When, Then
                Should.Throw<ArgumentException>(() => directory.GetFile(filePath));
            }
        }

        public class GetDirectoryTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "..", "a")]
            [TestCase("a/b/", "..", "a")]
            [TestCase("a/b/../c", "..", "a")]
            [TestCase(".", "..", "..")]
            [TestCase("a", "..", "")]
            [TestCase("a/b", "c", "a/b/c")]
            public void ShouldReturnDirectory(string virtualPath, string path, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = directory.GetDirectory(path);

                // Then
                Assert.That(result.Path.FullPath, Is.EqualTo(expected));
            }

            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");

                // When, Then
                Should.Throw<ArgumentNullException>(() => directory.GetDirectory(null));
            }

            [Test]
            public void ThrowsForAbsolutePath()
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(".");
                NormalizedPath directoryPath = "/a/b";

                // When, Then
                Should.Throw<ArgumentException>(() => directory.GetDirectory(directoryPath));
            }
        }

        public class GetParentTests : VirtualInputDirectoryFixture
        {
            [TestCase("a/b", "a")]
            [TestCase("a/b/", "a")]
            [TestCase(".", "")]
            [TestCase("a", "")]
            public void ShouldReturnParentDirectory(string virtualPath, string expected)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                IDirectory result = directory.Parent;

                // Then
                Assert.That(result?.Path.FullPath, Is.EqualTo(expected));
            }
        }

        public class ExistsTests : VirtualInputDirectoryFixture
        {
            [TestCase(".")]
            [TestCase("c")]
            [TestCase("c/1")]
            [TestCase("a/b")]
            public void ShouldReturnTrueForExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                bool exists = directory.Exists;

                // Then
                exists.ShouldBeTrue();
            }

            [TestCase("x")]
            [TestCase("bar")]
            [TestCase("baz")]
            [TestCase("a/b/c")]
            [TestCase("q/w/e")]
            public void ShouldReturnFalseForNonExistingPaths(string virtualPath)
            {
                // Given
                VirtualInputDirectory directory = GetVirtualInputDirectory(virtualPath);

                // When
                bool exists = directory.Exists;

                // Then
                exists.ShouldBeFalse();
            }

            [TestCase("a")]
            [TestCase("a/b")]
            public void ShouldReturnFalseForExcludedPath(string excluded)
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.ExcludedPaths.Add(excluded);
                VirtualInputDirectory directory = GetVirtualInputDirectory("a/b", fileSystem);

                // When
                bool exists = directory.Exists;

                // Then
                exists.ShouldBeFalse();
            }
        }

        public class CreateTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ShouldThrow()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                Should.Throw<NotSupportedException>(() => directory.Create());
            }
        }

        public class DeleteTests : VirtualInputDirectoryFixture
        {
            [Test]
            public void ShouldThrow()
            {
                // Given
                TestFileSystem fileSystem = new TestFileSystem();
                VirtualInputDirectory directory = new VirtualInputDirectory(fileSystem, ".");

                // When, Then
                Should.Throw<NotSupportedException>(() => directory.Delete(false));
            }
        }

        private VirtualInputDirectory GetVirtualInputDirectory(string path) =>
            GetVirtualInputDirectory(path, new TestFileSystem(GetFileProvider()));

        private VirtualInputDirectory GetVirtualInputDirectory(string path, TestFileSystem fileSystem)
        {
            fileSystem.RootPath = "/a";
            fileSystem.InputPaths.Add("b");
            fileSystem.InputPaths.Add("/foo");
            return new VirtualInputDirectory(fileSystem, path);
        }

        private IFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider
            {
                { "/a/b/c/foo.txt" },
                { "/a/b/c/baz.txt" },
                { "/a/b/c/1/2.txt" },
                { "/a/b/d/baz.txt" },
                { "/a/x/bar.txt" },
                { "/foo/baz.txt" },
                { "/foo/c/baz.txt" },
                { "/bar/baz.txt" },
            };

            // Directories that don't exist as files
            fileProvider.AddDirectory("/a/y/z");
            fileProvider.AddDirectory("/foo/a/b");

            return fileProvider;
        }

        private VirtualInputDirectory GetMappedVirtualInputDirectory(string path)
        {
            TestFileSystem fileSystem = new TestFileSystem(new TestFileProvider
            {
                { "/root/a/x/y/z/foo.txt" },
                { "/root/b/x/bar.txt" },
                { "/root/c/z/fizz.txt" },
                { "/root/d/e/y/z/buzz.txt" },
                { "/f/g/h/bazz.txt" }
            });
            fileSystem.RootPath = "/root";
            fileSystem.InputPaths.Add("a");
            fileSystem.InputPaths.Add("b");
            fileSystem.InputPaths.Add("c");
            fileSystem.InputPaths.Add("d/e");
            fileSystem.InputPaths.Add("../f/g");
            fileSystem.InputPathMappings.Add("c", "x/y");
            fileSystem.InputPathMappings.Add("d/e", "x");
            fileSystem.InputPathMappings.Add("../f/g", "i");
            return new VirtualInputDirectory(fileSystem, path);
        }
    }
}
