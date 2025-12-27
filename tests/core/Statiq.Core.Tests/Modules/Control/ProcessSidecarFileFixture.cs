using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ProcessSidecarFileFixture : BaseFixture
    {
        public class ExecuteTests : ProcessSidecarFileFixture
        {
            [Test]
            public async Task LoadsSidecarFile()
            {
                // Given
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton<IFileCleaner>(new TestFileCleaner());
                Engine engine = new Engine(services);
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                ProcessSidecarFile sidecar = new ProcessSidecarFile(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    lodedSidecarContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(inputs, context, sidecar);

                // Then
                Assert.That(lodedSidecarContent, Is.EqualTo("data: a1"));
                Assert.That(documents.Single().Content, Is.EqualTo("File a1"));
            }

            [Test]
            public async Task LoadsCustomSidecarFile()
            {
                // Given
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton<IFileCleaner>(new TestFileCleaner());
                Engine engine = new Engine(services);
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                string lodedSidecarContent = null;
                ProcessSidecarFile sidecar = new ProcessSidecarFile(".other", new ExecuteConfig(Config.FromDocument(async x =>
                {
                    lodedSidecarContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(inputs, context, sidecar);

                // Then
                Assert.That(lodedSidecarContent, Is.EqualTo("data: other"));
                Assert.That(documents.Single().Content, Is.EqualTo("File a1"));
            }

            [Test]
            public async Task ReturnsOriginalDocumentForMissingSidecarFile()
            {
                // Given
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton<IFileCleaner>(new TestFileCleaner());
                Engine engine = new Engine(services);
                TestExecutionContext context = GetExecutionContext(engine);
                TestDocument[] inputs =
                {
                    GetDocument("a/1.md", "File a1")
                };
                bool executedSidecarModules = false;
                ProcessSidecarFile sidecar = new ProcessSidecarFile(".foo", new ExecuteConfig(Config.FromDocument(x =>
                {
                    executedSidecarModules = true;
                    return (object)new[] { x };
                })));

                // When
                IReadOnlyList<TestDocument> documents = await ExecuteAsync(inputs, context, sidecar);

                // Then
                Assert.That(executedSidecarModules, Is.False);
                Assert.That(documents.First(), Is.EqualTo(inputs.First()));
            }

            private TestDocument GetDocument(string source, string content) =>
                new TestDocument(
                    new NormalizedPath("/" + source),
                    new NormalizedPath(source),
                    content);

            private TestExecutionContext GetExecutionContext(Engine engine) =>
                new TestExecutionContext
                {
                    Namespaces = new TestNamespacesCollection(engine.Namespaces.ToArray()),
                    FileSystem = GetFileSystem()
                };

            private TestFileSystem GetFileSystem() =>
                new TestFileSystem
                {
                    InputPaths = new PathCollection(NormalizedPath.AbsoluteRoot),
                    FileProvider = GetFileProvider()
                };

            private TestFileProvider GetFileProvider() =>
                new TestFileProvider
                {
                    { "/a/1.md", "File a1" },
                    { "/a/1.md.meta", "data: a1" },
                    { "/a/1.md.other", "data: other" },
                    { "/a/2.md", "File a2" },
                    { "/a/2.md.meta", "data: a2" },
                    { "/b/1.md", "File b1" },
                    { "/b/1.md.meta", "data: b1" }
                };
        }
    }
}