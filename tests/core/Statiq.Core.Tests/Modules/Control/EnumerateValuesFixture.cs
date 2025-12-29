using System;
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
    public class EnumerateValuesFixture : BaseFixture
    {
        public class ExecuteTests : EnumerateValuesFixture
        {
            [Test]
            public async Task EnumeratesSimpleValues()
            {
                // Given
                EnumerateValues module = new EnumerateValues(Config.FromValues(1, 2, 3));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(module);

                // Then
                results.Select(x => x[Keys.Current]).ShouldBe(new object[] { 1, 2, 3 }, true);
            }

            [Test]
            public async Task EnumeratesSingleValue()
            {
                // Given
                EnumerateValues module = new EnumerateValues(Config.FromValue(1));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(module);

                // Then
                results.Select(x => x[Keys.Current]).ShouldBe(new object[] { 1 }, true);
            }

            [Test]
            public async Task EnumeratesValuesFromMetadata()
            {
                // Given
                TestDocument[] inputs = new[]
                {
                    new TestDocument("Foo")
                    {
                        { Keys.Enumerate, 1 }
                    },
                    new TestDocument("Bar")
                    {
                        { Keys.Enumerate, new string[] { "Blue", "Green" } }
                    },
                    new TestDocument("Fizz")
                };
                EnumerateValues module = new EnumerateValues();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(inputs, module);

                // Then
                results
                    .Select(x => Tuple.Create(x.Content, x.Get(Keys.Current)))
                    .ShouldBe(
                        new Tuple<string, object>[]
                        {
                            Tuple.Create("Foo", (object)1),
                            Tuple.Create("Bar", (object)"Blue"),
                            Tuple.Create("Bar", (object)"Green"),
                            Tuple.Create("Fizz", (object)null)
                        },
                        true);
            }

            [Test]
            public async Task EnumeratesValueFromDefaultMetadataKey()
            {
                // Given
                TestDocument input = new TestDocument("Foo")
                {
                    { Keys.Enumerate, new int[] { 1, 2 } }
                };
                EnumerateValues module = new EnumerateValues();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Select(x => x[Keys.Current]).ShouldBe(new object[] { 1, 2 }, true);
            }

            [Test]
            public async Task EnumeratesValueFromCustomMetadataKey()
            {
                // Given
                TestDocument input = new TestDocument("Foo")
                {
                    { "Bar", new int[] { 1, 2 } }
                };
                EnumerateValues module = new EnumerateValues("Bar");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Select(x => x[Keys.Current]).ShouldBe(new object[] { 1, 2 }, true);
            }

            [Test]
            public async Task SetsCustomCurrentKey()
            {
                // Given
                TestDocument input = new TestDocument("Foo")
                {
                    { Keys.Enumerate, new int[] { 1, 2 } }
                };
                EnumerateValues module = new EnumerateValues().WithCurrentKey("Bar");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Select(x => x.Get(Keys.Current)).ShouldBe(new object[] { null, null });
                results.Select(x => x["Bar"]).ShouldBe(new object[] { 1, 2 }, true);
            }

            [Test]
            public async Task CombinesAllValuesWithAllInputs()
            {
                // Given
                TestDocument[] inputs = new[]
                {
                    new TestDocument("Foo"),
                    new TestDocument("Bar")
                };
                EnumerateValues module = new EnumerateValues(Config.FromValues(1, 2));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(inputs, module);

                // Then
                results
                    .Select(x => Tuple.Create(x.Content, x.Get(Keys.Current)))
                    .ShouldBe(
                        new Tuple<string, object>[]
                        {
                            Tuple.Create("Foo", (object)1),
                            Tuple.Create("Foo", (object)2),
                            Tuple.Create("Bar", (object)1),
                            Tuple.Create("Bar", (object)2)
                        },
                        true);
            }

            [Test]
            public async Task DoesNotOutputForEmptyEnumerable()
            {
                // Given
                TestDocument[] inputs = new[]
                {
                    new TestDocument("Foo"),
                    new TestDocument("Bar")
                };
                EnumerateValues module = new EnumerateValues(Config.FromValues<int>());

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(inputs, module);

                // Then
                results.ShouldBeEmpty();
            }

            [Test]
            public async Task OutputsOriginalDocumentsForNullEnumerable()
            {
                // Given
                TestDocument[] inputs = new[]
                {
                    new TestDocument("Foo"),
                    new TestDocument("Bar")
                };
                EnumerateValues module = new EnumerateValues(Config.FromValue((IEnumerable<object>)null));

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(inputs, module);

                // Then
                results
                    .Select(x => Tuple.Create(x.Content, x.Get(Keys.Current)))
                    .ShouldBe(
                        new Tuple<string, object>[]
                        {
                            Tuple.Create("Foo", (object)null),
                            Tuple.Create("Bar", (object)null)
                        },
                        true);
            }

            [Test]
            public async Task DoesNotIncludeInputDocumentIfNotAvailable()
            {
                // Given
                EnumerateValues module = new EnumerateValues(Config.FromValues(1, 2, 3)).WithInputDocument(true);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(module);

                // Then
                results.Select(x => x[Keys.Current]).ShouldBe(new object[] { 1, 2, 3 }, true);
            }

            [Test]
            public async Task IncludesInputDocument()
            {
                // Given
                TestDocument input = new TestDocument("Foo")
                {
                    { Keys.Enumerate, new int[] { 1, 2 } },
                    { Keys.EnumerateWithInput, true }
                };
                EnumerateValues module = new EnumerateValues();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Select(x => x.GetInt(Keys.Current, -1)).ShouldBe(new int[] { -1, 1, 2 }, true);
            }
        }
    }
}
