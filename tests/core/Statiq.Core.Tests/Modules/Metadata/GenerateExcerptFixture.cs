using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class GenerateExcerptFixture : BaseFixture
    {
        public class ExecuteTests : GenerateExcerptFixture
        {
            [Test]
            public async Task KeepsExisting()
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
                TestDocument document = new TestDocument(input)
                {
                    { Keys.Excerpt, "Foobar" }
                };
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("Foobar");
            }

            [Test]
            public async Task ExcerptFirstParagraph()
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
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some Foobar text</p>");
            }

            [Test]
            public async Task ExcerptAlternateQuerySelector()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <div>This is some other text</div>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt("div");

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<div>This is some other text</div>");
            }

            [Test]
            public async Task ExcerptAlternateMetadataKey()
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
                GenerateExcerpt excerpt = new GenerateExcerpt().WithMetadataKey("Baz");

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Baz"].ShouldBe("<p>This is some Foobar text</p>");
            }

            [Test]
            public async Task ExcerptInnerHtml()
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
                GenerateExcerpt excerpt = new GenerateExcerpt().WithOuterHtml(false);

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("This is some Foobar text");
            }

            [Test]
            public async Task NoExcerptReturnsSameDocument()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <div>This is some Foobar text</div>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt("p");

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task SeparatorInsideParagraph()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- excerpt --> Foobar text</p>
                            <p>This is other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some </p>");
            }

            [Test]
            public async Task SeparatorBetweenParagraphs()
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
                            <!-- excerpt -->
                            <p>This is some more text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ToString().ShouldBe(
                    @"<p>This is some Foobar text</p>
                            <p>This is some other text</p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SeparatorInsideParagraphWithSiblings()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This <b>is</b> some <!-- excerpt --><i>other</i> text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ToString().ShouldBe(
                    @"<p>This is some Foobar text</p>
                            <p>This <b>is</b> some </p>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateSeparatorComment()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- foo --> Foobar text</p>
                            <p>This is other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt().WithSeparators(new[] { "foo" });

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some </p>");
            }

            [Test]
            public async Task MultipleSeparatorComments()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <!-- excerpt --> Foobar text</p>
                            <p>This is <!-- excerpt --> other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some </p>");
            }

            [Test]
            public async Task IncludesNestedElements()
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
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some <b>Foobar</b> text</p>");
            }

            [Test]
            public async Task DoesNotEncodeHtmlEntities()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some ""Foobar"" text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some \"Foobar\" text</p>");
            }

            // https://github.com/statiqdev/Statiq.Web/issues/981
            [Test]
            public async Task DoesNotDoubleEncodeHtmlEntitiesOnSerialization()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some &quot;Foobar&quot; text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GenerateExcerpt excerpt = new GenerateExcerpt();

                // When
                TestDocument result = await ExecuteAsync(document, excerpt).SingleAsync();

                // Then
                result["Excerpt"].ShouldBe("<p>This is some &quot;Foobar&quot; text</p>");
            }
        }
    }
}