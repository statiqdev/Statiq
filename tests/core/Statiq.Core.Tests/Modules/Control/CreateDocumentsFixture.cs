using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class CreateDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : CreateDocumentsFixture
        {
            [Test]
            public async Task CountReturnsCorrectDocuments()
            {
                // Given
                CreateDocuments create = new CreateDocuments(5);

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(create);

                // Then
                results.Count.ShouldBe(5);
            }

            [Test]
            public async Task ContentReturnsCorrectDocuments()
            {
                // Given
                CreateDocuments create = new CreateDocuments("A", "B", "C", "D");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "A", "B", "C", "D" });
            }

            [Test]
            public async Task ContextConfigContentReturnsCorrectDocument()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add("Foo", "Bar");
                CreateDocuments create =
                    new CreateDocuments(Config.FromContext(x => $"{x.Settings.GetString("Foo")}1"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Bar1" });
            }

            [Test]
            public async Task ContextConfigContentReturnsCorrectDocuments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add("Foo", "Bar");
                CreateDocuments create =
                    new CreateDocuments(Config.FromContext(x => (IEnumerable<string>)new[]
                    {
                        $"{x.Settings.GetString("Foo")}1",
                        $"{x.Settings.GetString("Foo")}2"
                    }));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Bar1", "Bar2" });
            }

            [Test]
            public async Task ContextConfigContentReturnsCorrectDocumentWhenInputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add("Foo", "Bar");
                CreateDocuments create =
                    new CreateDocuments(Config.FromContext(x => $"{x.Settings.GetString("Foo")}1"));
                TestDocument input = new TestDocument("Baz");
                input.TestMetadata.Add("ABC", 123);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, context, create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Bar1" });
                results.Select(x => x.ContainsKey("ABC")).ShouldBe(new bool[] { false });
            }

            [Test]
            public async Task ContextConfigContentReturnsCorrectDocumentsWhenInputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add("Foo", "Bar");
                CreateDocuments create =
                    new CreateDocuments(Config.FromContext(x => (IEnumerable<string>)new[]
                    {
                        $"{x.Settings.GetString("Foo")}1",
                        $"{x.Settings.GetString("Foo")}2"
                    }));
                TestDocument input = new TestDocument("Baz");
                input.TestMetadata.Add("ABC", 123);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, context, create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Bar1", "Bar2" });
                results.Select(x => x.ContainsKey("ABC")).ShouldBe(new bool[] { false, false });
            }

            [Test]
            public async Task DocumentConfigContentReturnsCorrectDocumentWhenInputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add("Foo", "Bar");
                CreateDocuments create =
                    new CreateDocuments(Config.FromDocument((doc, ctx) => $"{ctx.Settings.GetString("Foo")}{doc.GetString("ABC")}1"));
                TestDocument input = new TestDocument("Baz");
                input.TestMetadata.Add("ABC", 123);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, context, create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Bar1231" });
                results.Select(x => x.ContainsKey("ABC")).ShouldBe(new bool[] { false });
            }

            [Test]
            public async Task DocumentConfigContentReturnsCorrectDocumentsWhenInputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add("Foo", "Bar");
                CreateDocuments create =
                    new CreateDocuments(Config.FromDocument((doc, ctx) => (IEnumerable<string>)new[]
                    {
                        $"{ctx.Settings.GetString("Foo")}{doc.GetString("ABC")}1",
                        $"{ctx.Settings.GetString("Foo")}{doc.GetString("ABC")}2"
                    }));
                TestDocument input = new TestDocument("Baz");
                input.TestMetadata.Add("ABC", 123);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, context, create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "Bar1231", "Bar1232" });
                results.Select(x => x.ContainsKey("ABC")).ShouldBe(new bool[] { false, false });
            }

            [Test]
            public async Task MetadataReturnsCorrectDocuments()
            {
                // Given
                CreateDocuments create = new CreateDocuments(
                    new Dictionary<string, object> { { "Foo", "a" } },
                    new Dictionary<string, object> { { "Foo", "b" } },
                    new Dictionary<string, object> { { "Foo", "c" } });

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(create);

                // Then
                results.Select(x => x["Foo"]).ShouldBe(new[] { "a", "b", "c" });
            }

            [Test]
            public async Task ContentAndMetadataReturnsCorrectDocuments()
            {
                // Given
                List<string> content = new List<string>();
                List<object> values = new List<object>();
                CreateDocuments create = new CreateDocuments(
                    Tuple.Create("A", new Dictionary<string, object> { { "Foo", "a" } }.AsEnumerable()),
                    Tuple.Create("B", new Dictionary<string, object> { { "Foo", "b" } }.AsEnumerable()),
                    Tuple.Create("C", new Dictionary<string, object> { { "Foo", "c" } }.AsEnumerable()));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(create);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "A", "B", "C" });
                results.Select(x => x["Foo"]).ShouldBe(new[] { "a", "b", "c" });
            }
        }
    }
}
