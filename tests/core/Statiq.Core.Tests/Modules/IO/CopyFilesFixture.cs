using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class CopyFilesFixture : BaseFixture
    {
        public class ConstructorTests : CopyFilesFixture
        {
            [Test]
            public void ThrowsOnNullPathFunction()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new CopyFiles((Config<IEnumerable<string>>)null));
            }

            [Test]
            public void ThrowsOnNullPatterns()
            {
                // Given, When, Then
                Assert.Throws<ArgumentNullException>(() => new CopyFiles((string[])null));
            }
        }

        public class ExecuteTests : CopyFilesFixture
        {
            [Test]
            public async Task RecursivePatternCopiesFiles()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.That(context.FileSystem.GetOutputFile("test-a.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("test-b.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputDirectory("Subfolder").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("markdown-x.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists, Is.False);
            }

            [Test]
            public async Task CopyFilesInTopDirectoryOnly()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.That(context.FileSystem.GetOutputFile("test-a.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("test-b.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputDirectory("Subfolder").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("markdown-x.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists, Is.False);
            }

            [Test]
            public async Task CopyFilesInSubfolderOnly()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("Subfolder/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.That(context.FileSystem.GetOutputFile("test-a.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("test-b.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputDirectory("Subfolder").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("markdown-x.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists, Is.False);
            }

            [Test]
            public async Task DoesNotCopyFilesAboveInputPath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("../*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.That(context.FileSystem.GetOutputFile("test-a.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("test-b.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputDirectory("Subfolder").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("markdown-x.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("test-above-input.txt").Exists, Is.False);
            }

            [Test]
            public async Task CopyFilesAboveInputPathWithOthers()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("../**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.That(context.FileSystem.GetOutputFile("test-a.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("test-b.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputDirectory("Subfolder").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("markdown-x.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("test-above-input.txt").Exists, Is.False); // Files outside an input path will not be copied
            }

            [Test]
            public async Task CopyFolderFromAbsolutePath()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("/TestFiles/Input/**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.That(context.FileSystem.GetOutputFile("test-a.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("test-b.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputDirectory("Subfolder").Exists, Is.True);
                Assert.That(context.FileSystem.GetOutputFile("markdown-x.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists, Is.False);
            }

            [Test]
            public async Task CopyNonExistingFolder()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("NonExisting/**/*.txt");

                // When
                await ExecuteAsync(context, copyFiles);

                // Then
                Assert.That(context.FileSystem.GetOutputFile("test-a.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("test-b.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/test-c.txt").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputDirectory("Subfolder").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("markdown-x.md").Exists, Is.False);
                Assert.That(context.FileSystem.GetOutputFile("Subfolder/markdown-y.md").Exists, Is.False);
            }

            public async Task ShouldSetSourceAndDestination()
            {
                // Given
                TestExecutionContext context = GetExecutionContext();
                CopyFiles copyFiles = new CopyFiles("**/test-a.txt");

                // When
                TestDocument output = await ExecuteAsync(context, copyFiles).SingleAsync();

                // Then
                output.Source.FullPath.ShouldBe("/TestFiles/Input/test-a.txt");
                output.Destination.FullPath.ShouldBe("test-a.txt");
            }
        }

        protected static TestExecutionContext GetExecutionContext()
        {
            TestFileProvider fileProvider = new TestFileProvider();

            fileProvider.AddDirectory("/");
            fileProvider.AddDirectory("/TestFiles");
            fileProvider.AddDirectory("/TestFiles/Input");
            fileProvider.AddDirectory("/TestFiles/Input/Subfolder");

            fileProvider.AddFile("/TestFiles/test-above-input.txt", "test");
            fileProvider.AddFile("/TestFiles/Input/markdown-x.md", "xxx");
            fileProvider.AddFile("/TestFiles/Input/test-a.txt", "aaa");
            fileProvider.AddFile("/TestFiles/Input/test-b.txt", "bbb");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/markdown-y.md", "yyy");
            fileProvider.AddFile("/TestFiles/Input/Subfolder/test-c.txt", "ccc");

            TestFileSystem fileSystem = new TestFileSystem
            {
                FileProvider = fileProvider,
                RootPath = "/"
            };
            fileSystem.InputPaths.Clear();
            fileSystem.InputPaths.Add("/TestFiles/Input");

            return new TestExecutionContext
            {
                FileSystem = fileSystem
            };
        }
    }
}
