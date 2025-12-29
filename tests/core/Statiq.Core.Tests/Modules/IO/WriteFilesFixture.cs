using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class WriteFilesFixture : BaseFixture
    {
        public class ExecuteTests : WriteFilesFixture
        {
            [Test]
            public async Task ShouldTruncateOldFileOnWrite()
            {
                // Given
                const string fileName = "test.txt";
                const string oldContent = "TestTest";
                const string newContent = "Zxcv";
                TestDocument input = new TestDocument(
                    new NormalizedPath(fileName),
                    newContent);
                TestExecutionContext context = GetExecutionContext(input.Yield());
                IFile file = context.FileSystem.GetOutputFile(fileName);
                await file.WriteAllTextAsync(oldContent);
                WriteFiles writeFiles = new WriteFiles();

                // When
                await ExecuteAsync(context, writeFiles);

                // Then
                IFile outputFile = context.FileSystem.GetOutputFile(fileName);
                (await outputFile.ReadAllTextAsync()).ShouldBe(newContent);
            }

            [Test]
            public async Task OutputDocumentContainsSameContent()
            {
                // Given
                TestDocument input = new TestDocument("Test");
                TestExecutionContext context = GetExecutionContext(input.Yield());
                WriteFiles writeFiles = new WriteFiles();

                // When
                TestDocument result = await ExecuteAsync(context, writeFiles).SingleAsync();

                // Then
                result.Content.ShouldBe("Test");
            }

            [Test]
            public async Task ShouldReturnOriginalDocumentForFailedPredicate()
            {
                // Given
                TestDocument input = new TestDocument("Test");
                TestExecutionContext context = GetExecutionContext(input.Yield());
                WriteFiles writeFiles = new WriteFiles().Where(false);

                // When
                TestDocument result = await ExecuteAsync(context, writeFiles).SingleAsync();

                // Then
                result.ShouldBe(input);
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenOverwritting()
            {
                // Given
                TestDocument[] inputs = new[] { "A", "B", "C", "D", "E" }
                    .Select(x => new TestDocument(new NormalizedPath("output.txt"), x))
                    .ToArray();
                TestExecutionContext context = GetExecutionContext(inputs);
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.Error;
                WriteFiles writeFiles = new WriteFiles();

                // When
                await ExecuteAsync(context, writeFiles);

                // Then
                IFile outputFile = context.FileSystem.GetOutputFile("output.txt");
                outputFile.Exists.ShouldBeTrue();
                (await outputFile.ReadAllTextAsync()).ShouldBe("E");
            }

            [Test]
            public async Task DocumentsWithSameOutputGeneratesWarning()
            {
                // Given
                TestDocument[] inputs = new[] { "A", "B", "C", "D", "E" }
                    .Select(x => new TestDocument(new NormalizedPath("output.txt"), x))
                    .ToArray();
                TestExecutionContext context = GetExecutionContext(inputs);
                WriteFiles writeFiles = new WriteFiles();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(context, writeFiles));
            }

            [Test]
            public async Task InputDocumentsAreEvaluatedInOrderWhenAppending()
            {
                // Given
                TestDocument[] inputs = new[] { "A", "B", "C", "D", "E" }
                    .Select(x => new TestDocument(new NormalizedPath("output.txt"), x))
                    .ToArray();
                TestExecutionContext context = GetExecutionContext(inputs);
                WriteFiles writeFiles = new WriteFiles().Append();

                // When
                await ExecuteAsync(context, writeFiles);

                // Then
                IFile outputFile = context.FileSystem.GetOutputFile("output.txt");
                outputFile.Exists.ShouldBeTrue();
                (await outputFile.ReadAllTextAsync()).ShouldBe("ABCDE");
            }

            [Test]
            public async Task IgnoresEmptyDocuments()
            {
                // Given
                MemoryStream emptyStream = new MemoryStream(new byte[] { });
                TestDocument[] inputs =
                {
                    new TestDocument(
                        new NormalizedPath("Subfolder/write-test"),
                        "Test"),
                    new TestDocument(
                        new NormalizedPath("Subfolder/empty-test"),
                        string.Empty),
                    new TestDocument(
                        new NormalizedPath("Subfolder/stream-test"),
                        emptyStream)
                };
                TestExecutionContext context = GetExecutionContext(inputs);
                WriteFiles writeFiles = new WriteFiles();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, writeFiles);

                // Then
                results.Count.ShouldBe(3);
                context.FileSystem.GetOutputFile("Subfolder/write-test").Exists.ShouldBeTrue();
                context.FileSystem.GetOutputFile("output/Subfolder/empty-test").Exists.ShouldBeFalse();
                context.FileSystem.GetOutputFile("output/Subfolder/stream-test").Exists.ShouldBeFalse();
            }
        }

        protected static TestExecutionContext GetExecutionContext(IEnumerable<TestDocument> inputs)
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

            return new TestExecutionContext(inputs)
            {
                FileSystem = fileSystem
            };
        }
    }
}
