using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class GatherHeadingsFixture : BaseFixture
    {
        public class ExecuteTests : GatherHeadingsFixture
        {
            [Test]
            public async Task SetsHeadingContent()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings();

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task IgnoresNestedElements()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo <small>Fizz</small></h1>
                            <h1>Bar <span> Bizz</span> Boo</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings();

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar  Boo" });
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
                            <h1>Foo <small>Fizz</small></h1>
                            <h1>Bar <span> Bizz</span> Boo</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings().WithNestedElements();

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo Fizz", "Bar  Bizz Boo" });
            }

            [Test]
            public async Task SetsHeadingMetadata()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings().WithHeadingKey("HContent");

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Select(x => x.GetString("HContent")).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task DoesNotSetHeadingMetadataIfNull()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings();

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Select(x => x.GetString("HContent")).ShouldBe(new string[] { null, null });
            }

            [Test]
            public async Task OnlyGetsFirstLevelByDefault()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings();

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
            }

            [Test]
            public async Task GetsDeeperLevels()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h1>Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings(3);

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Baz", "Bar" });
            }

            [Test]
            public async Task Nesting()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h2>Boz</h2>
                            <h1>Bar</h1>
                            <h2>Boo</h2>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings(3).WithNesting();

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Bar" });
                result.GetDocumentList(Keys.Headings)[0].GetDocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Baz", "Boz" });
                result.GetDocumentList(Keys.Headings)[1].GetDocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Boo" });
            }

            [Test]
            public async Task SetsChildrenWhenNotNesting()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h2>Baz</h2>
                            <h2>Boz</h2>
                            <h1>Bar</h1>
                            <h2>Boo</h2>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings(3);

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo", "Baz", "Boz", "Bar", "Boo" });
                result.GetDocumentList(Keys.Headings)[0].GetDocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Baz", "Boz" });
                result.GetDocumentList(Keys.Headings)[3].GetDocumentList(Keys.Children).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Boo" });
            }

            [Test]
            public async Task SetsHeadingIdAttribute()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo</h1>
                            <h1 id=""bar"">Bar</h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings();

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Select(x => x.GetString(Keys.HeadingId)).ShouldBe(new[] { null, "bar" });
            }

            [Test]
            public async Task GetsTextContentInLink()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo <a href=""bar"">Bar</a></h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings().WithHeadingKey("HContent");

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo Bar" });
                result.GetDocumentList(Keys.Headings).Select(x => x.GetString("HContent")).ShouldBe(new[] { "Foo Bar" });
            }

            [Test]
            public async Task ExcludesNonTextContentInLink()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo <a href=""bar"">Bar <small>Bazz</small></a></h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings().WithHeadingKey("HContent");

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo Bar" });
                result.GetDocumentList(Keys.Headings).Select(x => x.GetString("HContent")).ShouldBe(new[] { "Foo Bar" });
            }

            [Test]
            public async Task IncludesNonTextContentInLink()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foo <a href=""bar"">Bar <small>Bazz</small></a></h1>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                GatherHeadings headings = new GatherHeadings().WithHeadingKey("HContent").WithNestedElements(true);

                // When
                IDocument result = await ExecuteAsync(document, headings).SingleAsync();

                // Then
                result.GetDocumentList(Keys.Headings).Cast<TestDocument>().Select(x => x.Content).ShouldBe(new[] { "Foo Bar Bazz" });
                result.GetDocumentList(Keys.Headings).Select(x => x.GetString("HContent")).ShouldBe(new[] { "Foo Bar Bazz" });
            }
        }
    }
}