using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    public class ExecuteConfigFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteConfigFixture
        {
            [Test]
            public async Task ContextConfigDoesNotThrowForNullResult()
            {
                // Given
                ExecuteConfig execute = new ExecuteConfig(Config.FromContext(_ => (object)null));

                // When, Then
                await Should.NotThrowAsync(async () => await ExecuteAsync(Array.Empty<TestDocument>(), execute));
            }

            [Test]
            public async Task ContextConfigReturnsInputsForNullResult()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                ExecuteConfig execute = new ExecuteConfig(Config.FromContext(_ => (object)null));

                // When
                IReadOnlyList<TestDocument> outputs = await ExecuteAsync(inputs, execute);

                // Then
                Assert.That(inputs, Is.EqualTo(outputs));
            }

            [Test]
            public async Task ContextConfigReturnsDocumentForSingleResultDocument()
            {
                // Given
                TestDocument document = new TestDocument();
                ExecuteConfig execute = new ExecuteConfig(Config.FromContext(_ => document));

                // When
                TestDocument result = await ExecuteAsync(Array.Empty<TestDocument>(), execute).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task ContextConfigRunsModuleAgainstInputDocuments()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };

                object countLock = new object(); // ExecuteConfig runs in parallel so we need to lock on the counter
                int count = 0;
                ExecuteConfig execute = new ExecuteConfig(Config.FromContext(c =>
                {
                    lock (countLock)
                    {
                        count++;
                    }
                    return (object)null;
                }));

                // When
                await ExecuteAsync(inputs, execute);

                // Then
                count.ShouldBe(1);
            }

            [Test]
            public async Task ValueConfigReturnsNewDocumentWithStringContent()
            {
                // Given
                ExecuteConfig execute = new ExecuteConfig("Foo");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(execute);

                // Then
                results.Single().Content.ShouldBe("Foo");
            }

            [Test]
            public async Task ValueConfigReturnsNewDocumentsWithStringContent()
            {
                // Given
                ExecuteConfig execute = new ExecuteConfig(new[] { "Foo", "Bar" });

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(execute);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task ContextConfigReturnsInputDocumentsForAction()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument("Foo"),
                    new TestDocument("Bar")
                };
                ExecuteConfig execute = new ExecuteConfig(Config.FromContext(__ => { _ = 1 + 1; }));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, execute);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task DocumentConfigDoesNotThrowForNullResult()
            {
                // Given
                ExecuteConfig execute = new ExecuteConfig(Config.FromDocument((_, __) => (object)null));

                // When
                await ExecuteAsync(execute);

                // Then
            }

            [Test]
            public async Task DocumentConfigReturnsInputsForNullResult()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                ExecuteConfig execute = new ExecuteConfig(Config.FromDocument((_, __) => (object)null));

                // When
                IReadOnlyList<TestDocument> outputs = await ExecuteAsync(inputs, execute);

                // Then
                Assert.That(inputs, Is.EqualTo(outputs));
            }

            [Test]
            public async Task DocumentConfigReturnsDocumentForSingleResultDocument()
            {
                // Given
                TestDocument document = new TestDocument();
                CountModule count = new CountModule("A")
                {
                    EnsureInputDocument = true
                };
                ExecuteConfig execute = new ExecuteConfig(Config.FromDocument((_, __) => document));

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(count, execute);

                // Then
                Assert.That(document, Is.EquivalentTo(result.Single()));
            }

            [Test]
            public async Task DocumentConfigSetsNewContent()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument(),
                    new TestDocument()
                };
                object countLock = new object(); // ExecuteConfig runs in parallel so we need to lock on the counter
                int count = 0;
                ExecuteConfig execute = new ExecuteConfig(Config.FromDocument((d, c) =>
                {
                    lock (countLock)
                    {
                        return (object)count++;
                    }
                }));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, execute);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "0", "1" }, true);
            }

            [Test]
            public async Task DocumentConfigReturnsInputDocumentsForAction()
            {
                // Given
                TestDocument[] inputs =
                {
                    new TestDocument("Foo"),
                    new TestDocument("Bar")
                };
                ExecuteConfig execute = new ExecuteConfig(Config.FromDocument((doc, ctx) => { _ = 1 + 1; }));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(inputs, execute);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
            }
        }
    }
}