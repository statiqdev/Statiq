using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Lunr.Tests
{
    [TestFixture]
    public class GenerateLunrIndexFixture : BaseFixture
    {
        public class ExecuteTests : GenerateLunrIndexFixture
        {
            [Test]
            public async Task ScriptIncludesRefAsDocumentKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""}}");
            }

            [Test]
            public async Task DoesNotIncludeHostInLinkByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"""link"":""/a/a.html""");
            }

            [Test]
            public async Task IncludesHostInLink()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .IncludeHostInLinks()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"""link"":""http://fizzbuzz.com/a/a.html""");
            }

            [Test]
            public async Task ScriptContainsIndexFile()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain($@"indexFile: '/{GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.gz")}'");
            }

            [Test]
            public async Task ScriptContainsResultsFile()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain($@"resultsFile: '/{GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json")}'");
            }

            [TestCase(false)]
            [TestCase(true)]
            public async Task ScriptDoesNotContainTypeaheadCodeWhenStemming(bool stemming)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithStemming(stemming);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                if (stemming)
                {
                    scriptDocument.Content.ShouldNotContain("lunr.Query.wildcard.TRAILING");
                }
                else
                {
                    scriptDocument.Content.ShouldContain("lunr.Query.wildcard.TRAILING");
                }
            }

            [Test]
            public async Task CanChangeClientObjectName()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithClientName("foobar");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldContain("const foobar = {");
            }

            [Test]
            public async Task ChangeScriptPath()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithScriptPath("foo.js");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.ShouldHaveSingleWithDestination("foo.js");
            }

            [Test]
            public async Task ChangeIndexPath()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithIndexPath("bar.gz");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.gz"),
                        "bar.gz"
                    },
                    true);
            }

            [Test]
            public async Task FilesUseCustomScriptPathByDefault()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithScriptPath("foo.js");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(new NormalizedPath[] { "foo.js", "foo.results.gz", "foo.index.gz" }, true);
            }

            [Test]
            public async Task FilesUseScriptPathByDefault()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.gz"),
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.gz")
                    },
                    true);
            }

            [Test]
            public async Task ChangeResultsPath()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithResultsPath("bar.gz");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.gz"),
                        "bar.gz"
                    },
                    true);
            }

            [Test]
            public async Task DoesNotZipResultsFile()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.gz"),
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json")
                    },
                    true);
            }

            [Test]
            public async Task DoesNotZipIndexFile()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.Select(x => x.Destination).ShouldBe(
                    new NormalizedPath[]
                    {
                        GenerateLunrIndex.DefaultScriptPath,
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"),
                        GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.gz")
                    },
                    true);
            }

            [Test]
            public async Task CanCustomizeScript()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .CustomizeScript((_, __) => "foobar");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldBe("foobar");
            }

            [TestCase((string)null)]
            [TestCase("")]
            public async Task DoesNotOutputScriptForNullOrEmptyCustomize(string script)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .CustomizeScript((_, __) => script);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath);
            }

            [TestCase("AAA", "aAA")]
            [TestCase("Aaa", "aaa")]
            [TestCase("aaa", "aaa")]
            [TestCase("aAA", "aAA")]
            public async Task WithAdditionalFieldAsCamelCase(string fieldName, string expectedName)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "AAA", "bbb" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField(fieldName, FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain($@"""{expectedName}"":""bbb""");
            }

            [TestCase("Title")]
            [TestCase("title")]
            public async Task WithoutField(string fieldName)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithoutField(fieldName)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html""}}");
            }

            [Test]
            public async Task WithoutAnyFields()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithoutAnyFields()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldNotContain("resultsFile");
            }

            [Test]
            public async Task ScriptDocumentsHasMultipleItems()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""-1722774156"":{""link"":""/b.html"",""title"":""Bar""}}");
            }

            [Test]
            public async Task AdditionalFieldsIncludedOnlyWhenAvailable()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    { "AAA", "bbb" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("aaa", FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""-53465798"":{""aaa"":""bbb"",""link"":""/b.html"",""title"":""Bar""}}");
            }

            [Test]
            public async Task OmitsFromSearch()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    { LunrKeys.OmitFromSearch, true }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldNotContain(@"""title"":""Bar""");
            }

            [TestCase((int)123, "\"123\"")]
            [TestCase(-123.45F, "\"-123.45\"")]
            [TestCase(true, "\"true\"")]
            [TestCase(new string[] { "123", "456" }, "[\"123\",\"456\"]")]
            public async Task ConvertsResultField(object value, string expected)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "AAA", value }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("aaa", FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain($@"""aaa"":{expected}");
            }

            [TestCase((int)123, "\"123\"")]
            [TestCase(-123.45F, "\"123.45\"")] // The "-" gets removed in the index
            [TestCase(true, "\"true\"")]
            public async Task ConvertsSearchableField(object value, string expected)
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "AAA", value }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("aaa", FieldType.Searchable)
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldContain(expected);
            }

            [Test]
            public async Task ConvertsSearchableArray()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "AAA", new string[] { "123", "456 789" } }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("aaa", FieldType.Searchable)
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldContain("123");
                indexDocument.Content.ShouldContain("456");
                indexDocument.Content.ShouldContain("789");
            }

            [Test]
            public async Task MissingSearchableFieldInOneDocument()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    { "AAA", "bbb" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("aaa", FieldType.Searchable)
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldContain("aaa");
                indexDocument.Content.ShouldContain("bbb");
            }

            [Test]
            public async Task MissingSearchableFieldInAllDocuments()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("aaa", FieldType.Searchable)
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldNotContain("aaa");
                indexDocument.Content.ShouldNotContain("bbb");
            }

            [Test]
            public async Task IncludeRefInFields()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("ref", FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"""ref"":""-1843551964""");
            }

            [Test]
            public async Task DocumentDefinesRef()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "ref", "abcd" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""abcd"":{");
            }

            [Test]
            public async Task AlternateRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "foo", "abcd" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithReferenceKey("foo")
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""abcd"":{");
            }

            // When using the default search metadata delegate, the document cache key will be used as
            // the reference value if a document doesn't have a value for a custom reference key
            [Test]
            public async Task DefaultRefValueIfNoAlternateRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { "fizz", "abcd" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithReferenceKey("fizz")
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""abcd"":{""link"":""/a/a.html"",""title"":""Foo""},""-1722774156"":{""link"":""/b.html"",""title"":""Bar""}}");
            }

            [Test]
            public async Task AlternateSearchItems()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", "d" }
                        },
                        new Dictionary<string, object>
                        {
                            { "link", "e" },
                            { "title", "f" },
                            { "content", "g" },
                            { "ref", "h" }
                        }
                    }))
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""},""h"":{""link"":""e"",""title"":""f""}}");
            }

            [Test]
            public async Task AlternateSearchItemsUsesDocumentMetadata()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }))
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}}");
            }

            [Test]
            public async Task AlternateSearchItemsMultipleDocuments()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", doc.GetTitle() }
                        }
                    }))
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""Foo"":{""link"":""a"",""title"":""Foo""},""Bar"":{""link"":""a"",""title"":""Bar""}}");
            }

            [Test]
            public async Task AlternateSearchItemsSkipsNullItem()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", "d" }
                        },
                        null
                    }))
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}}");
            }

            [Test]
            public async Task AlternateSearchItemsWithoutRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", doc.GetTitle() },
                            { "content", "c" },
                            { "ref", "d" }
                        },
                        new Dictionary<string, object>
                        {
                            { "link", "e" },
                            { "title", "f" },
                            { "content", "g" }
                        }
                    }))
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}");
            }

            [Test]
            public async Task AlternateSearchItemsAreCaseInsensitive()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItems(Config.FromDocument<IEnumerable<IEnumerable<KeyValuePair<string, object>>>>(doc => new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "LINK", "a" },
                            { "TITLE", doc.GetTitle() },
                            { "CONTENT", "c" },
                            { "REF", "d" }
                        }
                    }))
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}");
            }

            [Test]
            public async Task MetadataSearchItems()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new[]
                        {
                            new Dictionary<string, object>
                            {
                                { "link", "a" },
                                { "title", "b" },
                                { "content", "c" },
                                { "ref", "d" }
                            },
                            new Dictionary<string, object>
                            {
                                { "link", "e" },
                                { "title", "f" },
                                { "content", "g" },
                                { "ref", "h" }
                            }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""b""},""h"":{""link"":""e"",""title"":""f""}}");
            }

            [Test]
            public async Task MetadataSearchItemsUsesDocumentMetadata()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""Foo""}}");
            }

            [Test]
            public async Task MetadataSearchItemsForOneDocument()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"b.html", "Buzz")
                {
                    { Keys.Title, "Bar" },
                    {
                        LunrKeys.SearchItems,
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", "b" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a, b }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""-1843551964"":{""link"":""/a/a.html"",""title"":""Foo""},""d"":{""link"":""a"",""title"":""b""}}");
            }

            [Test]
            public async Task NullMetadataSearchItemsFallsBackToDocument()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    { LunrKeys.SearchItems, null }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""184806200"":{""link"":""/a/a.html"",""title"":""Foo""}}");
            }

            // A ref value must be provided for custom search items
            [Test]
            public async Task MetadataSearchItemsWithoutRefKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new Dictionary<string, object>
                        {
                            { "link", "e" },
                            { "title", "f" },
                            { "content", "g" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.gz"));
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldNotContain("resultsFile");
            }

            [Test]
            public async Task MetadataSearchItemsAreCaseInsensitive()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        LunrKeys.SearchItems,
                        new MetadataItems
                        {
                            { "LINK", "a" },
                            { "TITLE", "b" },
                            { "CONTENT", "c" },
                            { "REF", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""b""}");
            }

            [Test]
            public async Task CustomMetadataSearchItemsKey()
            {
                // Given
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz")
                {
                    { Keys.Title, "Foo" },
                    {
                        "Fizzbuzz",
                        new Dictionary<string, object>
                        {
                            { "link", "a" },
                            { "title", "b" },
                            { "content", "c" },
                            { "ref", "d" }
                        }
                    }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithSearchItemsKey("Fizzbuzz")
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"{""d"":{""link"":""a"",""title"":""b""}}");
            }

            [Test]
            public async Task EmptySearchIndex()
            {
                // Given
                GenerateLunrIndex module = new GenerateLunrIndex();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new TestDocument[] { }, module);

                // Then
                results.ShouldNotHaveDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.gz"));
                TestDocument scriptDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath);
                scriptDocument.Content.ShouldNotContain("resultsFile");
            }

            [Test]
            public async Task UsesStopWords()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Bizz Fizz Buzz Bazz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithStopWords(new[] { "fizz", "fuzz" })
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldNotContain("fizz");
                indexDocument.Content.ShouldContain("bizz");
            }

            [Test]
            public async Task UsesEnglishStopWordsByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Bizz Fizz And Buzz Bazz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldNotContain("and");
                indexDocument.Content.ShouldContain("fizz");
            }

            [Test]
            public async Task EmptyStopWordsUsesAllWords()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Bizz Fizz And Buzz Bazz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithStopWords(Array.Empty<string>())
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldContain("and");
                indexDocument.Content.ShouldContain("fizz");
            }

            [Test]
            public async Task StemmingUsesEnglishStemmerByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz cry buzz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithStemming(true)
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldNotContain("\"cry\"");
                indexDocument.Content.ShouldContain("\"cri\"");
            }

            [Test]
            public async Task CustomStemmer()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz cry buzz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithStemming(x => x == "cry" ? "abcd" : x)
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldNotContain("\"cry\"");
                indexDocument.Content.ShouldNotContain("\"cri\"");
                indexDocument.Content.ShouldContain("\"abcd\"");
            }

            [Test]
            public async Task DoesNotStemByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument((NormalizedPath)"a/a.html", "Fizz cry buzz")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldContain("\"cry\"");
                indexDocument.Content.ShouldNotContain("\"cri\"");
            }

            [Test]
            public async Task DoesNotAllowPositionMetadataByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument(
                    (NormalizedPath)"a/a.html")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldNotContain(@"""position"":[[0,3]]");
            }

            [Test]
            public async Task AllowsPositionMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument a = new TestDocument(
                    (NormalizedPath)"a/a.html")
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .AllowPositionMetadata()
                    .ZipIndexFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument indexDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".index.json"));
                indexDocument.Content.ShouldContain(@"""position"":[[0,3]]");
            }

            [Test]
            public async Task RemovesHtmlFromHtmlContentByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument(
                    (NormalizedPath)"a/a.html",
                    @"<html>
    <head>
        <title>Foo</title>
    </head>
    <body>
        <h1>This is the header</h1>
        <p>
            Lorum ipsum
        </p>
    </body>
</html>",
                    MediaTypes.Html)
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("content", FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"""content"":""Foo This is the header Lorum ipsum""");
            }

            [Test]
            public async Task RemovesHtmlFromHtmlFragmentContentByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument(
                    (NormalizedPath)"a/a.html",
                    @"<h1>This is the header</h1>
<p>
    Lorum ipsum
</p>",
                    MediaTypes.HtmlFragment)
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("content", FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"""content"":""This is the header Lorum ipsum""");
            }

            [Test]
            public async Task DoesNotRemoveHtmlFromNonHtmlContent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument(
                    (NormalizedPath)"a/a.html",
                    @"<html>
    <head>
        <title>Foo</title>
    </head>
    <body>
        <h1>This is the header</h1>
        <p>
            Lorum ipsum
        </p>
    </body>
</html>",
                    MediaTypes.Xml)
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .WithField("content", FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"""content"":""\u003Chtml\u003E");
            }

            [Test]
            public async Task DoesNotRemoveHtmlWhenFalse()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "fizzbuzz.com");
                TestDocument a = new TestDocument(
                    (NormalizedPath)"a/a.html",
                    @"<html>
    <head>
        <title>Foo</title>
    </head>
    <body>
        <h1>This is the header</h1>
        <p>
            Lorum ipsum
        </p>
    </body>
</html>",
                    MediaTypes.Html)
                {
                    { Keys.Title, "Foo" }
                };
                GenerateLunrIndex module = new GenerateLunrIndex()
                    .RemoveHtml(false)
                    .WithField("content", FieldType.Result)
                    .ZipResultsFile(false);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { a }, context, module);

                // Then
                TestDocument resultsDocument = results.ShouldHaveSingleWithDestination(GenerateLunrIndex.DefaultScriptPath.ChangeExtension(".results.json"));
                resultsDocument.Content.ShouldContain(@"""content"":""\u003Chtml\u003E");
            }
        }
    }
}
