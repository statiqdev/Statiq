using System;
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
    public class QueryHtmlFixture : BaseFixture
    {
        public class ExecuteTests : QueryHtmlFixture
        {
            [Test]
            public async Task GetOuterHtml()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetOuterHtml("Key");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task GetOuterHtmlWithAttributes()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some Foobar text</p>
                            <p foo=""baz"" foo=""bat"" a=""A"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetOuterHtml("Key");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    @"<p foo=""bar"">This is some Foobar text</p>",
                    @"<p foo=""baz"" a=""A"">This is some other text</p>"
                });
            }

            [Test]
            public async Task GetOuterHtmlForFirst()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetOuterHtml("Key")
                    .First();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>"
                });
            }

            [Test]
            public async Task GetInnerHtml()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetInnerHtml("Key");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x["Key"].ToString()).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public async Task GetInnerHtmlAndOuterHtml()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetInnerHtml("InnerHtmlKey")
                    .GetOuterHtml("OuterHtmlKey");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x["InnerHtmlKey"].ToString()).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
                results.Select(x => x["OuterHtmlKey"].ToString()).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task SetOuterHtmlContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .SetContent();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x.Content).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task SetInnerHtmlContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .SetContent(false);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x.Content).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public async Task SetOuterHtmlContentWithMetadata()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .SetContent()
                    .GetInnerHtml("InnerHtmlKey")
                    .GetOuterHtml("OuterHtmlKey");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => ((TestDocument)x).Content).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
                results.Select(x => x.GetString("InnerHtmlKey")).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
                results.Select(x => x.GetString("OuterHtmlKey")).ShouldBe(new[]
                {
                    "<p>This is some Foobar text</p>",
                    "<p>This is some other text</p>"
                });
            }

            [Test]
            public async Task GetTextContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <b>Foobar</b> text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetTextContent("TextContentKey");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x.GetString("TextContentKey")).ShouldBe(new[]
                {
                    "This is some Foobar text",
                    "This is some other text"
                });
            }

            [Test]
            public async Task GetAttributeValue()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetAttributeValue("foo", "Foo");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x.GetString("Foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public async Task GetAttributeValueWithImplicitKey()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetAttributeValue("foo");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x.GetString("foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public async Task GetAttributeValueWithMoreThanOneMatch()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetAttributeValue("foo");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, query);

                // Then
                results.Select(x => x.GetString("foo")).ShouldBe(new[]
                {
                    "bar",
                    "baz"
                });
            }

            [Test]
            public async Task GetAttributeValues()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"" x=""X"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetAttributeValues();

                // When
                List<IOrderedEnumerable<KeyValuePair<string, object>>> results = (await ExecuteAsync(document, query))
                    .Select(x => x.OrderBy(y => y.Key, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                // Then
                results.Count.ShouldBe(2);
                new[]
                {
                    new KeyValuePair<string, object>("a", "A"),
                    new KeyValuePair<string, object>("b", "B"),
                    new KeyValuePair<string, object>("foo", "bar")
                }.ShouldBeSubsetOf(results[0]);
                new[]
                {
                    new KeyValuePair<string, object>("foo", "baz"),
                    new KeyValuePair<string, object>("x", "X")
                }.ShouldBeSubsetOf(results[1]);
            }

            [Test]
            public async Task GetAll()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p foo=""bar"" foo=""bat"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>
                            <p foo=""baz"" x=""X"">This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                QueryHtml query = new QueryHtml("p")
                    .GetAll();

                // When
                List<IOrderedEnumerable<KeyValuePair<string, object>>> results = (await ExecuteAsync(document, query))
                    .Select(x => x.OrderBy(y => y.Key, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                // Then
                results.Count.ShouldBe(2);
                new[]
                {
                    new KeyValuePair<string, object>("a", "A"),
                    new KeyValuePair<string, object>("b", "B"),
                    new KeyValuePair<string, object>("foo", "bar"),
                    new KeyValuePair<string, object>("InnerHtml", "This is some <b>Foobar</b> text"),
                    new KeyValuePair<string, object>("OuterHtml", @"<p foo=""bar"" a=""A"" b=""B"">This is some <b>Foobar</b> text</p>"),
                    new KeyValuePair<string, object>("TextContent", "This is some Foobar text")
                }.ShouldBeSubsetOf(results[0]);
                new[]
                {
                    new KeyValuePair<string, object>("foo", "baz"),
                    new KeyValuePair<string, object>("InnerHtml", "This is some other text"),
                    new KeyValuePair<string, object>("OuterHtml", @"<p foo=""baz"" x=""X"">This is some other text</p>"),
                    new KeyValuePair<string, object>("TextContent", "This is some other text"),
                    new KeyValuePair<string, object>("x", "X")
                }.ShouldBeSubsetOf(results[1]);
            }
        }
    }
}