using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class CreateTreeFixture : BaseFixture
    {
        public class ExecuteTests : CreateTreeFixture
        {
            [Test]
            public async Task GetsTreeWithCommonRoot()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/b/3.txt",
                    "root/a/1.txt",
                    "root/b/x/4.txt",
                    "root/c/d/5.txt",
                    "root/6.txt");
                CreateTree tree = new CreateTree().WithNesting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                VerifyTree(
                    results,
                    results.Single(),
                    "index.html",
                    "root/index.html",
                    "root/6.txt",
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/2.txt",
                    "root/b/index.html",
                    "root/b/3.txt",
                    "root/b/x/index.html",
                    "root/b/x/4.txt",
                    "root/c/index.html",
                    "root/c/d/index.html",
                    "root/c/d/5.txt");
            }

            [Test]
            public async Task GetsTree()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "a/2.txt",
                    "b/3.txt",
                    "a/1.txt",
                    "b/x/4.txt",
                    "c/d/5.txt",
                    "6.txt");
                CreateTree tree = new CreateTree().WithNesting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                VerifyTree(
                    results,
                    results.Single(),
                    "index.html",
                    "6.txt",
                    "a/index.html",
                    "a/1.txt",
                    "a/2.txt",
                    "b/index.html",
                    "b/3.txt",
                    "b/x/index.html",
                    "b/x/4.txt",
                    "c/index.html",
                    "c/d/index.html",
                    "c/d/5.txt");
            }

            [Test]
            public async Task GetsPlaceholderWithSource()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "a/2.txt",
                    "a/1.txt");
                CreateTree tree = new CreateTree().WithNesting();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                VerifyTree(
                    results,
                    results.Single(),
                    "index.html",
                    "a/index.html",
                    "a/1.txt",
                    "a/2.txt");
                results.Single().Source.FullPath.ShouldBe("/input/index.html");
            }

            [Test]
            public async Task CollapseRoot()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "a/2.txt",
                    "b/3.txt",
                    "a/1.txt",
                    "b/x/4.txt",
                    "c/d/5.txt",
                    "6.txt");
                CreateTree tree = new CreateTree().WithNesting(true, true);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                results.Count.ShouldBe(4);
                results.Select(x => x.Destination.FullPath)
                    .ShouldBe(new[] { "a/index.html", "b/index.html", "c/index.html", "6.txt" }, true);
            }

            [Test]
            public async Task SplitsTree()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/2.txt",
                    "root/b/index.html",
                    "root/a/1.txt",
                    "root/b/4.txt");
                CreateTree tree = new CreateTree()
                    .WithNesting()
                    .WithRoots(Config.FromDocument(doc => doc.Destination.FullPath.EndsWith("b/index.html")));

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                results.Count.ShouldBe(2);
                VerifyTree(
                    results[0].GetDocumentList(Keys.Children),
                    results[0],
                    "root/b/index.html",
                    "root/b/4.txt");
                VerifyTree(
                    results[1].GetDocumentList(Keys.Children),
                    results[1],
                    "index.html",
                    "root/index.html",
                    "root/a/index.html",
                    "root/a/1.txt",
                    "root/a/2.txt");
            }

            [Test]
            public async Task FlatTree()
            {
                // Given
                TestDocument[] inputs = GetDocumentsFromRelativePaths(
                    "root/a/b/2.txt",
                    "root/a/3.txt",
                    "root/a/1.txt");
                CreateTree tree = new CreateTree();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, tree);

                // Then
                VerifyTreeChildren(
                    results[0],
                    "root/a/b/2.txt");
                VerifyTreeChildren(
                    results[1],
                    "root/a/3.txt");
                VerifyTreeChildren(
                    results[2],
                    "root/a/1.txt");
                VerifyTreeChildren(
                    results[3],
                    "root/a/b/index.html",
                    "root/a/b/2.txt");
                VerifyTreeChildren(
                    results[4],
                    "root/a/index.html",
                    "root/a/b/index.html",
                    "root/a/3.txt",
                    "root/a/1.txt");
                VerifyTreeChildren(
                    results[5],
                    "root/index.html",
                    "root/a/index.html");
            }

            private void VerifyTree(IReadOnlyList<IDocument> documents, IDocument document, params string[] relativeFilePaths)
            {
                foreach (string relativeFilePath in relativeFilePaths)
                {
                    document.ShouldNotBeNull();
                    document.Destination.FullPath.ShouldBe(relativeFilePath);
                    document = documents.GetNext(document);
                    if (document is null)
                    {
                        break;
                    }
                }
            }

            private void VerifyTreeChildren(IDocument parent, string parentPath, params string[] childFilePaths)
            {
                parent.ShouldNotBeNull();
                parent.Destination.FullPath.ShouldBe(parentPath);
                IReadOnlyList<IDocument> children = parent.GetDocumentList(Keys.Children);
                children.Select(x => x.Destination.FullPath).ShouldBe(childFilePaths);
            }

            private TestDocument[] GetDocumentsFromRelativePaths(params string[] relativeFilePaths) =>
                relativeFilePaths.Select(x => new TestDocument(new NormalizedPath(x))).ToArray();
        }
    }
}
