using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class OrderDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : OrderDocumentsFixture
        {
            [Test]
            public async Task OrdersInAscendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                ConcatDocuments concat = new ConcatDocuments
                {
                    count2
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument<int>("A"));
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        return (IDocument)null;
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, concat, orderBy, gatherData);

                // Then
                content.Count.ShouldBe(20);
                content.ShouldBe(new[] { "1", "11", "2", "12", "3", "13", "4", "24", "5", "25", "26", "37", "38", "39", "410", "411", "412", "513", "514", "515" });
            }

            [Test]
            public async Task OrdersInDescendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                ConcatDocuments concat = new ConcatDocuments(count2);
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument<int>("A")).Descending();
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        return (IDocument)null;
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, concat, orderBy, gatherData);

                // Then
                content.Count.ShouldBe(20);
                content.ShouldBe(new[] { "515", "514", "513", "412", "411", "410", "39", "38", "37", "26", "5", "25", "4", "24", "3", "13", "2", "12", "1", "11" });
            }

            [Test]
            public async Task OrdersThenByInAscendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument<int>("A"))
                    .ThenBy(Config.FromDocument(d => d.GetInt("B")));
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        return (IDocument)null;
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.That(content.Count, Is.EqualTo(10)); // (4+1) * (21+1)
                Assert.That(content, Is.EqualTo(new[] { "11", "12", "23", "24", "35", "36", "47", "48", "59", "510" }));
            }

            [Test]
            public async Task OrdersThenByInDescendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument<int>("A"))
                    .ThenBy(Config.FromDocument(d => d.GetInt("B")))
                    .Descending();
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        return (IDocument)null;
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.That(content.Count, Is.EqualTo(10)); // (4+1) * (21+1)
                Assert.That(content, Is.EqualTo(new[] { "12", "11", "24", "23", "36", "35", "48", "47", "510", "59" }));
            }

            [Test]
            public async Task OrdersDescendingThenByInDescendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument<int>("A"))
                    .Descending()
                    .ThenBy(Config.FromDocument(d => d.GetInt("B")))
                    .Descending();
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        return (IDocument)null;
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.That(content.Count, Is.EqualTo(10)); // (4+1) * (21+1)
                Assert.That(content, Is.EqualTo(new[] { "510", "59", "48", "47", "36", "35", "24", "23", "12", "11" }));
            }

            [Test]
            public async Task OrdersDescendingThenByInAscendingOrder()
            {
                // Given
                List<string> content = new List<string>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 4,
                    EnsureInputDocument = true
                };
                CountModule count2 = new CountModule("B")
                {
                    AdditionalOutputs = 1
                };
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument<int>("A"))
                    .Descending()
                    .ThenBy(Config.FromDocument(d => d.GetInt("B")));
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        content.Add(await d.GetContentStringAsync());
                        return (IDocument)null;
                    })).ForEachDocument();

                // When
                await ExecuteAsync(count, count2, orderBy, gatherData);

                // Then
                Assert.That(content.Count, Is.EqualTo(10)); // (4+1) * (21+1)
                Assert.That(content, Is.EqualTo(new[] { "59", "510", "47", "48", "35", "36", "23", "24", "11", "12" }));
            }

            [Test]
            public async Task OrdersUsingMetadataKey()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Foo", 5 }
                };
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument
                {
                    { "Foo", 1 }
                };
                OrderDocuments order = new OrderDocuments("Foo");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { b, c, a });
            }

            [Test]
            public async Task OrdersUsingTypedComparer()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Foo", 5 }
                };
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument
                {
                    { "Foo", "1" }
                };
                OrderDocuments order = new OrderDocuments(Config.FromDocument(x => x.Get("Foo")))
                    .WithComparer<int>(Comparer<int>.Default);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { b, c, a });
            }

            [Test]
            public async Task OrdersUsingTypedComparison()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Foo", 5 }
                };
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument
                {
                    { "Foo", "1" }
                };
                OrderDocuments order = new OrderDocuments(Config.FromDocument(x => x.Get("Foo")))
                    .WithComparison<int>((x, y) => Comparer<int>.Default.Compare(x, y));

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { b, c, a });
            }

            [Test]
            public async Task OrdersUsingCommonTypeConversion()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    // The comparison is based on the first type seen, so test with DateTime first to ensure consistency across cultures
                    { "Foo", new DateTime(2010, 1, 1) }
                };
                TestDocument c = new TestDocument
                {
                    // Convert a DateTime into a string (instead of just using a string) to ensure a culture match when converting back to DateTime
                    { "Foo", new DateTime(2009, 1, 1).ToShortDateString() }
                };
                OrderDocuments order = new OrderDocuments("Foo");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, c }, order);

                // Then
                results.ShouldBe(new[] { c, a });
            }

            [Test]
            public async Task OrdersByIndexByDefault()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Index", 1 }
                };
                TestDocument b = new TestDocument
                {
                    { "Index", 10 }
                };
                TestDocument c = new TestDocument
                {
                    { "Index", 2 }
                };
                OrderDocuments order = new OrderDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { a, c, b });
            }

            [Test]
            public async Task OrdersByOrderByDefault()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Order", 1 }
                };
                TestDocument b = new TestDocument
                {
                    { "Order", 10 }
                };
                TestDocument c = new TestDocument
                {
                    { "Order", 2 }
                };
                OrderDocuments order = new OrderDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { a, c, b });
            }

            [Test]
            public async Task OrdersByIndexThenOrderByDefault()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Index", 2 },
                    { "Order", 1 }
                };
                TestDocument b = new TestDocument
                {
                    { "Index", 1 },
                    { "Order", 2 }
                };
                TestDocument c = new TestDocument
                {
                    { "Index", 1 },
                    { "Order", 1 }
                };
                OrderDocuments order = new OrderDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { c, b, a });
            }

            [Test]
            public async Task OrdersByOrderAsIntByDefault()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Order", "1" }
                };
                TestDocument b = new TestDocument
                {
                    { "Order", "10" }
                };
                TestDocument c = new TestDocument
                {
                    { "Order", "2" }
                };
                OrderDocuments order = new OrderDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { a, c, b });
            }

            [Test]
            public async Task OrdersByIndexAsIntByDefault()
            {
                // Given
                TestDocument a = new TestDocument
                {
                    { "Index", "1" }
                };
                TestDocument b = new TestDocument
                {
                    { "Index", "10" }
                };
                TestDocument c = new TestDocument
                {
                    { "Index", "2" }
                };
                OrderDocuments order = new OrderDocuments();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(new[] { a, b, c }, order);

                // Then
                results.ShouldBe(new[] { a, c, b });
            }
        }
    }
}
