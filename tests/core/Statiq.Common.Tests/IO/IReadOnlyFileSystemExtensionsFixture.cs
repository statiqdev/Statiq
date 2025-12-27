using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class IReadOnlyFileSystemExtensionsFixture : BaseFixture
    {
        public class GetInputFileTests : IReadOnlyFileSystemExtensionsFixture
        {
            [TestCase("foo.txt", "/a/b/c/foo.txt")]
            [TestCase("bar.txt", "/a/x/bar.txt")]
            [TestCase("baz.txt", "/a/y/baz.txt")]
            [TestCase("/a/b/c/foo.txt", "/a/b/c/foo.txt")]
            [TestCase("/z/baz.txt", "/z/baz.txt")]
            public void ReturnsInputFile(string input, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");

                // When
                IFile result = fileSystem.GetInputFile(input);

                // Then
                Assert.That(result.Path.FullPath, Is.EqualTo(expected));
            }

            [Test]
            public void ReturnsInputFileAboveInputDirectory()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("x/t");

                // When
                IFile result = fileSystem.GetInputFile("../bar.txt");

                // Then
                Assert.That(result.Path.FullPath, Is.EqualTo("/a/x/bar.txt"));
            }

            [Test]
            public void ReturnsInputFileWhenInputDirectoryAboveRoot()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x");

                // When
                IFile result = fileSystem.GetInputFile("bar.txt");

                // Then
                Assert.That(result.Path.FullPath, Is.EqualTo("/a/x/bar.txt"));
            }

            [Test]
            public void ReturnsInputFileWhenInputDirectoryAndFileAscend()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/b";
                fileSystem.InputPaths.Add("../x/y");

                // When
                IFile result = fileSystem.GetInputFile("../bar.txt");

                // Then
                Assert.That(result.Path.FullPath, Is.EqualTo("/a/x/bar.txt"));
            }
        }

        public class GetInputDirectoryTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public void ReturnsVirtualInputDirectoryForRelativePath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("A/B/C");

                // Then
                Assert.That(result, Is.InstanceOf<VirtualInputDirectory>());
                Assert.That(result.Path.FullPath, Is.EqualTo("A/B/C"));
            }

            [Test]
            public void ReturnsVirtualInputDirectoryForAscendingPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("../A/B/C");

                // Then
                Assert.That(result, Is.InstanceOf<VirtualInputDirectory>());
                Assert.That(result.Path.FullPath, Is.EqualTo("../A/B/C"));
            }

            [Test]
            public void ReturnsVirtualInputDirectoryForNullPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory();

                // Then
                Assert.That(result, Is.InstanceOf<VirtualInputDirectory>());
                Assert.That(result.Path.FullPath, Is.EqualTo(string.Empty));
            }

            [Test]
            public void ReturnsDirectoryForAbsolutePath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When
                IDirectory result = fileSystem.GetInputDirectory("/A/B/C");

                // Then
                Assert.That(result.Path.FullPath, Is.EqualTo("/A/B/C"));
            }
        }

        public class GetInputDirectoriesTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public void ReturnsCombinedInputDirectories()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("theme");
                fileSystem.InputPaths.Add("b/c");
                fileSystem.InputPaths.Add("b/d");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPaths.Add("y");
                fileSystem.InputPaths.Add("../z");

                // When
                IEnumerable<IDirectory> result = fileSystem.GetInputDirectories();

                // Then
                Assert.That(result.Select(x => x.Path.FullPath), Is.EquivalentTo(
                    new[]
                {
                    "/a/theme",
                    "/a/input",
                    "/a/b/c",
                    "/a/b/d",
                    "/a/x",
                    "/a/y",
                    "/z"
                }));
            }
        }

        public class GetContainingInputPathForAbsolutePathTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When, Then
                Should.Throw<ArgumentNullException>(() => fileSystem.GetContainingInputPathForAbsolutePath(null));
            }

            [TestCase("/a/b/c/foo.txt", "b", "/a/b")]
            [TestCase("/a/x/bar.txt", "x", "/a/x")]
            [TestCase("/a/x/baz.txt", "x", "/a/x")]
            [TestCase("/z/baz.txt", null, null)]
            [TestCase("/a/b/c/../e/foo.txt", "b", "/a/b")]
            [TestCase("/a/b/c", "b", "/a/b")]
            [TestCase("/a/x", "x", "/a/x")]
            public void ShouldReturnContainingPathForAbsolutePath(string path, string expectedInputPath, string expectedAbsolutePath)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                (NormalizedPath inputPath, NormalizedPath absoluteInputPath) = fileSystem.GetContainingInputPathForAbsolutePath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expectedInputPath);
                absoluteInputPath.FullPath.ShouldBe(expectedAbsolutePath);
            }

            [TestCase("/a/b/c/foo.txt", "../b", "/a/b")]
            [TestCase("/a/x/bar.txt", "../x", "/a/x")]
            [TestCase("/a/x/baz.txt", "../x", "/a/x")]
            [TestCase("/z/baz.txt", null, null)]
            [TestCase("/a/b/c/../e/foo.txt", "../b", "/a/b")]
            public void ShouldReturnContainingPathForInputPathAboveRootPath(string path, string expectedInputPath, string expectedAbsolutePath)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/y";
                fileSystem.InputPaths.Add("../b");
                fileSystem.InputPaths.Add("../x");

                // When
                (NormalizedPath inputPath, NormalizedPath absoluteInputPath) = fileSystem.GetContainingInputPathForAbsolutePath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expectedInputPath);
                absoluteInputPath.FullPath.ShouldBe(expectedAbsolutePath);
            }
        }

        public class GetContainingInputPathTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public void ThrowsForNullPath()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem();

                // When, Then
                Should.Throw<ArgumentNullException>(() => fileSystem.GetContainingInputPath(null));
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            [TestCase("/a/b/c", "/a/b")]
            [TestCase("/a/x", "/a/x")]
            public void ShouldReturnContainingPathForAbsolutePath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("/a/b/c/foo.txt", "/a/b")]
            [TestCase("/a/x/bar.txt", "/a/x")]
            [TestCase("/a/x/baz.txt", "/a/x")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "/a/b")]
            public void ShouldReturnContainingPathForInputPathAboveRootPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a/y";
                fileSystem.InputPaths.Add("../b");
                fileSystem.InputPaths.Add("../x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("c/foo.txt", "/a/b")]
            [TestCase("bar.txt", "/a/x")]
            [TestCase("baz.txt", null)]
            [TestCase("z/baz.txt", null)]
            [TestCase("c/../e/foo.txt", null)]
            [TestCase("c/e/../foo.txt", "/a/b")]
            public void ShouldReturnContainingPathForRelativeFilePath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("c", "/a/b")]
            [TestCase("z", "/a/y")]
            [TestCase("r", null)]
            [TestCase("c/e", null)]
            public void ShouldReturnContainingPathForRelativeDirectoryPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("y");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnContainingPathWhenOtherInputPathStartsTheSame()
            {
                // Given
                TestFileProvider fileProvider = GetFileProvider();
                fileProvider.AddDirectory("yz");
                fileProvider.AddDirectory("y");
                fileProvider.AddFile("/a/yz/baz.txt");
                IFileSystem fileSystem = new TestFileSystem(fileProvider);
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("yz");
                fileSystem.InputPaths.Add("y");

                // When
                NormalizedPath inputPath = fileSystem.GetContainingInputPath(new NormalizedPath("baz.txt"));

                // Then
                inputPath.FullPath.ShouldBe("/a/yz");
            }
        }

        public class GetRelativeInputPathTests : IReadOnlyFileSystemExtensionsFixture
        {
            [TestCase("/a/b/c/foo.txt", "c/foo.txt")]
            [TestCase("/a/x/bar.txt", "bar.txt")]
            [TestCase("/a/x/baz.txt", "baz.txt")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "e/foo.txt")]
            [TestCase("/a/b/c", "c")]
            [TestCase("/a/x", "")]
            public void ShouldReturnRelativeInputPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");

                // When
                NormalizedPath inputPath = fileSystem.GetRelativeInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }

            [TestCase("/a/b/c/foo.txt", "bb/bbb/c/foo.txt")]
            [TestCase("/a/x/bar.txt", "xx/bar.txt")]
            [TestCase("/a/x/baz.txt", "xx/baz.txt")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "bb/bbb/e/foo.txt")]
            [TestCase("/a/b/c", "bb/bbb/c")]
            [TestCase("/a/x", "xx")]
            public void ShouldReturnRelativeMappedInputPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.InputPaths.Add("b");
                fileSystem.InputPaths.Add("x");
                fileSystem.InputPathMappings.Add("b", "bb/bbb");
                fileSystem.InputPathMappings.Add("x", "xx");

                // When
                NormalizedPath inputPath = fileSystem.GetRelativeInputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }
        }

        public class GetRelativeOutputPathTests : IReadOnlyFileSystemExtensionsFixture
        {
            [TestCase("/a/b/c/foo.txt", "c/foo.txt")]
            [TestCase("/z/baz.txt", null)]
            [TestCase("/a/b/c/../e/foo.txt", "e/foo.txt")]
            [TestCase("/a/b/c", "c")]
            [TestCase("/a/b", "")]
            public void ShouldReturnRelativeOutputPath(string path, string expected)
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                fileSystem.RootPath = "/a";
                fileSystem.OutputPath = "b";

                // When
                NormalizedPath inputPath = fileSystem.GetRelativeOutputPath(new NormalizedPath(path));

                // Then
                inputPath.FullPath.ShouldBe(expected);
            }
        }

        public class GetFilesTests : IReadOnlyFileSystemExtensionsFixture
        {
            [Test]
            public void ShouldThrowForNullDirectory()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());

                // When, Then
                Should.Throw<ArgumentNullException>(() => fileSystem.GetFiles((IDirectory)null, "/"));
            }

            [Test]
            public void ShouldNotThrowForNullPattern()
            {
                // Given
                IFileSystem fileSystem = new TestFileSystem(GetFileProvider());
                IDirectory dir = fileSystem.GetDirectory("/");

                // When
                IEnumerable<IFile> results = fileSystem.GetFiles(dir, null, "**/foo.txt");

                // Then
                Assert.That(results.Select(x => x.Path.FullPath), Is.EquivalentTo(new[] { "/a/b/c/foo.txt" }));
            }

            [TestCase("/", new[] { "/a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "a/b/c/foo.txt", "a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/a", new[] { "/a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "a/b/c/foo.txt", "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "!/a/b/c/foo.txt", "!/a/b/c/foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "**/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "/a/x/bar.txt" }, new[] { "/a/b/c/foo.txt", "/a/x/bar.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "/a/x/baz.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!/a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!/a/x/baz.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!**/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "**/foo.txt", "!**/bar.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "/**/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/", new[] { "/a/b/c/d/../foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)]
            [TestCase("/a", new[] { "a/b/c/foo.txt", "!/a/b/c/d/../foo.txt" }, new string[] { }, true)]
            [TestCase("/", new[] { "/**/*.txt" }, new[] { "/a/x/bar.txt", "/a/b/c/foo.txt", "/q/werty.txt" }, true)]
            [TestCase("/", new[] { "/**/*.txt" }, new[] { "/a/x/bar.txt", "/a/b/c/foo.txt", "/q/werty.txt" }, false)]
            [TestCase("/", new[] { "/q/werty.txt" }, new[] { "/q/werty.txt" }, true)]
            [TestCase("/", new[] { "/q/werty.txt" }, new[] { "/q/werty.txt" }, false)]
            [TestCase("/", null, new[] { "/a/b/c/foo.txt", "/a/x/bar.txt", "/q/werty.txt" }, false)]
            [TestCase("/", new string[] { }, new[] { "/a/b/c/foo.txt", "/a/x/bar.txt", "/q/werty.txt" }, false)]
            [TestCase("/a", null, new[] { "/a/b/c/foo.txt", "/a/x/bar.txt" }, false)]
            [TestCase("/a", new string[] { }, new[] { "/a/b/c/foo.txt", "/a/x/bar.txt" }, false)]
            [TestCase("/", new string[] { "" }, new string[] { }, false)]
            [TestCase("/a", new string[] { "" }, new string[] { }, false)]
            [TestCase("/", new[] { "/a/b/c/foo.txt", "!**/*.txt" }, new[] { "/a/b/c/foo.txt" }, true)] // Exclusions do not apply to every pattern
            [TestCase("/", new[] { "/a/b/c/foo.txt", "/a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" }, true)] // Distinct results
            public void ShouldReturnExistingFiles(string directory, string[] patterns, string[] expected, bool reverseSlashes)
            {
                // Given
                TestFileProvider fileProvider = GetFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/q");
                fileProvider.AddFile("/q/werty.txt");
                IFileSystem fileSystem = new TestFileSystem(fileProvider);
                IDirectory dir = fileSystem.GetDirectory(directory);

                // When
                IEnumerable<IFile> results = fileSystem.GetFiles(dir, patterns);

                // Then
                Assert.That(results.Select(x => x.Path.FullPath), Is.EquivalentTo(expected));

                if (reverseSlashes)
                {
                    // When
                    results = fileSystem.GetFiles(dir, patterns.Select(x => x.Replace("/", "\\")));

                    // Then
                    Assert.That(results.Select(x => x.Path.FullPath), Is.EquivalentTo(expected));
                }
            }

            [TestCase("/", "b", new[] { "/a/b/c/foo.txt" }, new[] { "/a/b/c/foo.txt" })]
            [TestCase("/", "a", new[] { "/a/b/c/foo.txt" }, new string[] { })]
            [TestCase("/", "a/b/c", new[] { "/a/b/c/foo.txt" }, new string[] { })]
            [TestCase("/", "a/x", new[] { "/a/b/c/foo.txt" }, new string[] { "/a/b/c/foo.txt" })]
            [TestCase("/", "b", new[] { "/**/*.txt" }, new[] { "/a/x/bar.txt", "/a/b/c/foo.txt", "/q/werty.txt" })]
            [TestCase("/", "a", new[] { "/**/*.txt" }, new[] { "/q/werty.txt" })]
            [TestCase("/", "a/b/c", new[] { "/**/*.txt" }, new[] { "/a/x/bar.txt", "/q/werty.txt" })]
            [TestCase("/", "a/b/c", null, new[] { "/a/x/bar.txt", "/q/werty.txt" })]
            [TestCase("/", "a/b/c", new string[] { }, new[] { "/a/x/bar.txt", "/q/werty.txt" })]
            [TestCase("/a", "a/b/c", null, new[] { "/a/x/bar.txt" })]
            [TestCase("/a", "a/b/c", new string[] { }, new[] { "/a/x/bar.txt" })]
            public void ShouldNotReturnExcludedFiles(string directory, string excluded, string[] patterns, string[] expected)
            {
                // Given
                TestFileProvider fileProvider = GetFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/q");
                fileProvider.AddFile("/q/werty.txt");
                IFileSystem fileSystem = new TestFileSystem(fileProvider);
                fileSystem.InputPaths.Clear();
                fileSystem.InputPaths.Add("/"); // Exclusions operate on relative input paths, so the input path must be defined
                fileSystem.ExcludedPaths.Add(excluded);
                IDirectory dir = fileSystem.GetDirectory(directory);

                // When
                IEnumerable<IFile> results = fileSystem.GetFiles(dir, patterns);

                // Then
                Assert.That(results.Select(x => x.Path.FullPath), Is.EquivalentTo(expected));
            }
        }

        private TestFileProvider GetFileProvider()
        {
            TestFileProvider fileProvider = new TestFileProvider
            {
                { "/a/b/c/foo.txt" },
                { "/a/x/bar.txt" }
            };

            // Directories that don't exist as files
            fileProvider.AddDirectory("/a/b/d");
            fileProvider.AddDirectory("/a/y/z");

            return fileProvider;
        }
    }
}
