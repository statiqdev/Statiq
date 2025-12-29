using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Shortcodes.IO
{
    [TestFixture]
    public class IncludeShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : IncludeShortcodeFixture
        {
            [Test]
            public async Task IncludesFile()
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/A");
                fileProvider.AddDirectory("/A/B");
                fileProvider.AddFile("/A/B/c.txt", "foo");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                fileSystem.InputPaths.Clear();
                fileSystem.InputPaths.Add("/A");

                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "B/c.txt")
                };
                IncludeShortcode shortcode = new IncludeShortcode();

                // When
                ShortcodeResult result = await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe("foo");
            }

            [Test]
            public async Task IncludesWebResource()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext
                {
                    Engine = new TestEngine
                    {
                        HttpResponseFunc = (_, __) => new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new System.Net.Http.StringContent("Hello from the other side.")
                        }
                    }
                };
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "http://foo.com/bar")
                };
                IncludeShortcode shortcode = new IncludeShortcode();

                // When
                ShortcodeResult result = await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe("Hello from the other side.");
            }

            [Test]
            public async Task NullResultIfFileDoesNotExist()
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/A");
                fileProvider.AddDirectory("/A/B");
                fileProvider.AddFile("/A/B/c.txt", "foo");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                fileSystem.InputPaths.Clear();
                fileSystem.InputPaths.Add("/A");

                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.Error;
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "B/d.txt")
                };
                IncludeShortcode shortcode = new IncludeShortcode();

                // When
                ShortcodeResult result = await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public async Task IncludesFileRelativeToSource()
            {
                // Given
                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/A");
                fileProvider.AddDirectory("/A/B");
                fileProvider.AddDirectory("/A/D");
                fileProvider.AddFile("/A/B/c.txt", "foo");
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                fileSystem.InputPaths.Clear();
                fileSystem.InputPaths.Add("/A");

                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(new NormalizedPath("/A/D/x.txt"), (NormalizedPath)null);
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "../B/c.txt")
                };
                IncludeShortcode shortcode = new IncludeShortcode();

                // When
                ShortcodeResult result = await shortcode.ExecuteAsync(args, string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe("foo");
            }
        }
    }
}
