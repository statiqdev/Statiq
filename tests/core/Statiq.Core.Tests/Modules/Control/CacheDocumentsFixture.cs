using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class CacheDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : CacheDocumentsFixture
        {
            [Test]
            public async Task CachesDocuments()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false));

                // When
                _ = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b" });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b" });
            }

            [Test]
            public async Task CachesDocumentsWithSameSource()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a1");
                TestDocument a2 = new TestDocument(new NormalizedPath("/a"), "a2");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false));

                // When
                _ = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a1", "a2", "b" });
                results.Select(x => x.Content).ShouldBe(new[] { "a1", "a2", "b" });
            }

            [Test]
            public async Task InvalidatesAllWithSameSource()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a1");
                TestDocument a2 = new TestDocument(new NormalizedPath("/a"), "a2");
                TestDocument a3 = new TestDocument(new NormalizedPath("/a"), "a3");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false));

                // When
                _ = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, a3, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a1", "a2", "b", "a1", "a3" });
                results.Select(x => x.Content).ShouldBe(new[] { "b", "a1", "a3" });
            }

            [Test]
            public async Task OnlySendsCacheMissesToChildModules()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument b2 = new TestDocument(new NormalizedPath("/b"), "b2");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false));

                // When
                _ = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, b2 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b", "b2" });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b2" });
            }

            [Test]
            public async Task DisablesCache()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument b2 = new TestDocument(new NormalizedPath("/b"), "b2");

                TestExecutionContext executionContext1 = new TestExecutionContext(new[] { a1, b1 });
                executionContext1.Settings.Add(Keys.DisableCache, "true");
                TestExecutionContext executionContext2 = new TestExecutionContext(new[] { a1, b2 });
                executionContext2.Settings.Add(Keys.DisableCache, "true");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false));

                // When
                _ = await ExecuteAsync(executionContext1, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(executionContext2, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b", "a", "b2" });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b2" });
            }

            [Test]
            public async Task ResetsCache()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument b2 = new TestDocument(new NormalizedPath("/b"), "b2");

                TestExecutionContext executionContext1 = new TestExecutionContext(new[] { a1, b1 });
                TestExecutionContext executionContext2 = new TestExecutionContext(new[] { a1, b2 });
                executionContext2.Settings.Add(Keys.ResetCache, "true");
                TestExecutionContext executionContext3 = new TestExecutionContext(new[] { a1, b2 });

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false));

                // When
                _ = await ExecuteAsync(executionContext1, cacheDocuments);
                _ = await ExecuteAsync(executionContext2, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(executionContext3, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b", "a", "b2", });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b2" });
            }

            [Test]
            public async Task CachesDocumentsForSameDependencies()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument d1 = new TestDocument(new NormalizedPath("/d1"), "d1");
                TestDocument d2 = new TestDocument(new NormalizedPath("/d2"), "d2");

                TestExecutionContext executionContext = new TestExecutionContext(new[] { a1, b1 });
                executionContext.Outputs.Dictionary["Foo"] = ImmutableArray.Create<IDocument>(d1, d2);

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .WithPipelineDependencies("Foo");

                // When
                _ = await ExecuteAsync(executionContext, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(executionContext, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b" });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b" });
            }

            [TestCase(Phase.Input, null, new string[] { "a", "b" })]
            [TestCase(Phase.Process, null, new[] { "a", "b", "a", "b" })]
            [TestCase(Phase.PostProcess, null, new[] { "a", "b" })]
            [TestCase(Phase.Output, null, new[] { "a", "b" })]
            [TestCase(Phase.Input, new[] { "Foo" }, new[] { "a", "b", "a", "b" })]
            [TestCase(Phase.Process, new[] { "Foo" }, new[] { "a", "b", "a", "b" })]
            [TestCase(Phase.PostProcess, new[] { "Foo" }, new[] { "a", "b", "a", "b" })]
            [TestCase(Phase.Output, new[] { "Foo" }, new[] { "a", "b", "a", "b" })]
            [TestCase(Phase.Process, new string[] { }, new string[] { "a", "b" })]
            public async Task PipelineDependencies(Phase phase, string[] pipelineDependencies, string[] expectedMissedContent)
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument d1 = new TestDocument(new NormalizedPath("/d1"), "d1");
                TestDocument d2 = new TestDocument(new NormalizedPath("/d2"), "d2");
                TestDocument d3 = new TestDocument(new NormalizedPath("/d2"), "d3");

                TestExecutionContext executionContext1 = new TestExecutionContext(new[] { a1, b1 });
                executionContext1.Outputs.Dictionary["Foo"] = ImmutableArray.Create<IDocument>(d1, d2);
                executionContext1.Phase = phase;
                executionContext1.Pipeline.Dependencies.Add("Foo");
                TestExecutionContext executionContext2 = new TestExecutionContext(new[] { a1, b1 });
                executionContext2.Outputs.Dictionary["Foo"] = ImmutableArray.Create<IDocument>(d1, d3);
                executionContext2.Phase = phase;
                executionContext2.Pipeline.Dependencies.Add("Foo");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .WithPipelineDependencies(pipelineDependencies);

                // When
                _ = await ExecuteAsync(executionContext1, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(executionContext2, cacheDocuments);

                // Then
                missedContent.ShouldBe(expectedMissedContent);
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b" });
            }

            [Test]
            public async Task ExplicitlyInvalidateDocument()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .InvalidateDocumentsWhere(Config.FromDocument(async doc => (await doc.GetContentStringAsync()).Equals("b")));

                // When
                _ = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b", "b" });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b" });
            }

            [Test]
            public async Task ExplicitlyInvalidateDocumentIfAny()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a1");
                TestDocument a2 = new TestDocument(new NormalizedPath("/a"), "a2");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .InvalidateDocumentsWhere(Config.FromDocument(async doc => (await doc.GetContentStringAsync()).Equals("a2")));

                // When
                _ = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a1", "a2", "b", "a1", "a2" });
                results.Select(x => x.Content).ShouldBe(new[] { "b", "a1", "a2" });
            }

            [Test]
            public async Task DocumentDependenciesStayTheSame()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a1");
                TestDocument a2 = new TestDocument(new NormalizedPath("/a"), "a2");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument d1 = new TestDocument(new NormalizedPath("/d1"), "d1");
                TestDocument d2 = new TestDocument(new NormalizedPath("/d2"), "d2");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .WithDocumentDependencies(Config.FromValue<IEnumerable<IDocument>>(new[] { d1, d2 }));

                // When
                _ = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a1", "a2", "b" });
                results.Select(x => x.Content).ShouldBe(new[] { "a1", "a2", "b" });
            }

            [Test]
            public async Task DocumentDependenciesChange()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a1");
                TestDocument a2 = new TestDocument(new NormalizedPath("/a"), "a2");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument d1 = new TestDocument(new NormalizedPath("/d1"), "d1");
                TestDocument d2 = new TestDocument(new NormalizedPath("/d2"), "d2");
                TestDocument d3 = new TestDocument(new NormalizedPath("/d2"), "d3");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .WithDocumentDependencies(Config.FromDocument(async doc =>
                        (await doc.GetContentStringAsync()).Equals("a2")
                            ? (missedContent.Count == 0
                                ? new[] { d1, d2 }
                                : new[] { d1, d3 })
                            : (IEnumerable<IDocument>)null));

                // When
                _ = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a1", "a2", "b", "a1", "a2" });
                results.Select(x => x.Content).ShouldBe(new[] { "b", "a1", "a2" });
            }

            [Test]
            public async Task InvalidatesAllWhenNotSourceMapping()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a1");
                TestDocument a2 = new TestDocument(new NormalizedPath("/a"), "a2");
                TestDocument a3 = new TestDocument(new NormalizedPath("/a"), "a3");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .WithoutSourceMapping();

                // When
                _ = await ExecuteAsync(new[] { a1, a2, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, a3, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a1", "a2", "b", "a1", "a3", "b" });
                results.Select(x => x.Content).ShouldBe(new[] { "a1", "a3", "b" });
            }

            [Test]
            public async Task SendsAllToChildModulesWhenNotSourceMapping()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");
                TestDocument b2 = new TestDocument(new NormalizedPath("/b"), "b2");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                    new ExecuteConfig(Config.FromDocument(async doc =>
                    {
                        missedContent.Add(await doc.GetContentStringAsync());
                        return doc;
                    })).WithParallelExecution(false))
                    .WithoutSourceMapping();

                // When
                _ = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, b2 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b", "a", "b2" });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b2" });
            }

            [Test]
            public async Task ReturnsCachedDocumentsWhenNotSourceMapping()
            {
                // Given
                TestDocument a1 = new TestDocument(new NormalizedPath("/a"), "a");
                TestDocument b1 = new TestDocument(new NormalizedPath("/b"), "b");

                List<string> missedContent = new List<string>();
                CacheDocuments cacheDocuments = new CacheDocuments(
                        new ExecuteConfig(Config.FromDocument(async doc =>
                        {
                            missedContent.Add(await doc.GetContentStringAsync());
                            return doc;
                        })).WithParallelExecution(false))
                    .WithoutSourceMapping();

                // When
                _ = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a1, b1 }, cacheDocuments);

                // Then
                missedContent.ShouldBe(new[] { "a", "b" });
                results.Select(x => x.Content).ShouldBe(new[] { "a", "b" });
            }
        }
    }
}