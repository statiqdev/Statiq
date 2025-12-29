using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class InsertLinksFixture : BaseFixture
    {
        public class ExecuteTests : InsertLinksFixture
        {
            [Test]
            public async Task NoReplacementReturnsSameDocument()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobaz", "http://www.google.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.ShouldBeSameAs(document);
            }

            [Test]
            public async Task AddsLink()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.google.com"">Foobar</a> text</p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsLinkWithoutImpactingEscapes()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title &lt; 2 < 3 &#64; 4 @ 5</h1>
                            <p>This A&lt;string, List&lt;B&gt;&gt; is &#64; fizz @ fuzz < some Foobar text</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title &lt; 2 < 3 &#64; 4 @ 5</h1>
                            <p>This A&lt;string, List&lt;B&gt;&gt; is &#64; fizz @ fuzz &lt; some <a href=""http://www.google.com"">Foobar</a> text</p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [TestCase(
                "<html><head></head><body><p>&#64; a Foobar b</p></body></html>",
                @"<html><head></head><body><p>&#64; a <a href=""http://www.google.com"">Foobar</a> b</p></body></html>")]
            [TestCase(
                "<html><head></head><body><p>a &#64;Foobar b</p></body></html>",
                @"<html><head></head><body><p>a &#64;<a href=""http://www.google.com"">Foobar</a> b</p></body></html>")]
            [TestCase(
                "<html><head></head><body><p>a Foobar&#64; b</p></body></html>",
                @"<html><head></head><body><p>a <a href=""http://www.google.com"">Foobar</a>&#64; b</p></body></html>")]
            [TestCase(
                "<html><head></head><body><p>a Foobar b&#64;</p></body></html>",
                @"<html><head></head><body><p>a <a href=""http://www.google.com"">Foobar</a> b&#64;</p></body></html>")]
            [TestCase(
                "<html><head></head><body><p>&#64;a &#64;Foobar&#64; b&#64;</p></body></html>",
                @"<html><head></head><body><p>&#64;a &#64;<a href=""http://www.google.com"">Foobar</a>&#64; b&#64;</p></body></html>")]
            public async Task PreservesCharacterEscapes(string input, string output)
            {
                // Given
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsLinkWithAlternateQuerySelector()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <baz>This is some Foobar text</baz>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <baz>This is some <a href=""http://www.google.com"">Foobar</a> text</baz>
                            <p>This is some Foobar text</p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                }).WithQuerySelector("baz");

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsLinkWhenContainerHasChildElements()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some Foobar <b>text</b></p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <a href=""http://www.google.com"">Foobar</a> <b>text</b></p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsLinkWhenInsideChildElement()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <i>Foobar</i> <b>text</b></p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This <i>is</i> some <i><a href=""http://www.google.com"">Foobar</a></i> <b>text</b></p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotReplaceInAttributes()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1 title=""Foobar"">Title</h1>
                            <p attr=""Foobar"">This is some Foobar <b ref=""Foobar"">text</b></p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1 title=""Foobar"">Title</h1>
                            <p attr=""Foobar"">This is some <a href=""http://www.google.com"">Foobar</a> <b ref=""Foobar"">text</b></p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsMultipleLinksInSameElement()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsMultipleLinksInDifferentElements()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foobaz</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                            <p>Another Foobaz paragraph</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Foobaz</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                            <p>Another <a href=""http://www.bing.com"">Foobaz</a> paragraph</p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotAddLinksInExistingLinkElements()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <a href=""http://www.yahoo.com"">Foobar</a> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsMultipleLinksWhenFirstIsSubstring()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foo</a>bar</i> text <a href=""http://www.bing.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddLinkMethodTakesPrecedence()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foobar</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foobar</a></i> text <a href=""http://www.yahoo.com"">Foobaz</a></p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foobar", "http://www.google.com" },
                    { "Foobaz", "http://www.bing.com" }
                }).WithLink("Foobaz", "http://www.yahoo.com");

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task IgnoreSubstringIfSearchingForWholeWords()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i>Foo</i> text Foobaz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some <i><a href=""http://www.google.com"">Foo</a></i> text Foobaz</p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                }).WithMatchOnlyWholeWord();

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AdjacentWords()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc Foo(baz) xyz</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc <a href=""http://www.google.com"">Foo</a>(<a href=""http://www.yahoo.com"">baz</a>) xyz</p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                });

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task WordAtTheEndAfterPreviousWord()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>sdfg asdf aasdf asf asdf asdf asdf aabc Fuzz bazz def efg baz x</p>
                        </body>
                    </html>";
                const string output = @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>sdfg asdf aasdf asf asdf asdf asdf aabc <a href=""http://www.google.com"">Fuzz</a> bazz def efg <a href=""http://www.yahoo.com"">baz</a> x</p>
                        
                    </body></html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Fuzz", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                }).WithMatchOnlyWholeWord();

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task NonWholeWords()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>abc Foo(baz) xyz</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                InsertLinks autoLink = new InsertLinks(new Dictionary<string, string>()
                {
                    { "Foo", "http://www.google.com" },
                    { "baz", "http://www.yahoo.com" },
                }).WithMatchOnlyWholeWord();

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.ShouldBeSameAs(document);
            }

            [TestCase("<li>Foo</li>", "<li>Foo</li>")]
            [TestCase("<li>Foo&lt;T&gt;</li>", "<li>Foo&lt;T&gt;</li>")]
            [TestCase("<li><code>Foo</code></li>", @"<li><code><a href=""http://www.foo.com"">Foo</a></code></li>")]
            [TestCase("<li><code>Foo&lt;T&gt;</code></li>", @"<li><code><a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a></code></li>")]
            [TestCase("<li><code>Foo&lt;Foo&gt;</code></li>", @"<li><code>Foo&lt;<a href=""http://www.foo.com"">Foo</a>&gt;</code></li>")]
            [TestCase("<li><code>Foo&lt;Foo&lt;T&gt;&gt;</code></li>", @"<li><code>Foo&lt;<a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a>&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;Foo&gt;</code></li>", @"<li><code>IEnumerable&lt;<a href=""http://www.foo.com"">Foo</a>&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;Foo&lt;T&gt;&gt;</code></li>", @"<li><code>IEnumerable&lt;<a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a>&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;IEnumerable&lt;Foo&gt;&gt;</code></li>", @"<li><code>IEnumerable&lt;IEnumerable&lt;<a href=""http://www.foo.com"">Foo</a>&gt;&gt;</code></li>")]
            [TestCase("<li><code>IEnumerable&lt;IEnumerable&lt;Foo&lt;T&gt;&gt;&gt;</code></li>", @"<li><code>IEnumerable&lt;IEnumerable&lt;<a href=""http://www.fooOfT.com"">Foo&lt;T&gt;</a>&gt;&gt;</code></li>")]
            public async Task AddLinksToGenericWordsInsideAngleBrackets(string input, string expected)
            {
                // Given
                string inputContent = $"<html><head></head><body><foo></foo><ul>{input}</ul></body></html>";
                string expectedContent = $"<html><head></head><body><foo></foo><ul>{expected}</ul></body></html>";
                TestDocument document = new TestDocument(inputContent);
                Dictionary<string, string> links = new Dictionary<string, string>()
                {
                    { "Foo&lt;T&gt;", "http://www.fooOfT.com" },
                    { "Foo", "http://www.foo.com" },
                };
                InsertLinks autoLink = new InsertLinks(links)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
                    .WithStartWordSeparators('<')
                    .WithEndWordSeparators('>');

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(expectedContent);
            }

            [Test]
            public async Task DoesNotRewriteOutsideQuerySelectorWhenNoReplacements()
            {
                // Given
                string inputContent = $"<div>@x.Select(x => x) <code>Foo bar</code></div>";
                string expectedContent = $"<div>@x.Select(x => x) <code>Foo bar</code></div>";
                TestDocument document = new TestDocument(inputContent);
                Dictionary<string, string> links = new Dictionary<string, string>();
                InsertLinks autoLink = new InsertLinks(links)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
                    .WithStartWordSeparators('<')
                    .WithEndWordSeparators('>');

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.Content.ShouldBe(expectedContent);
            }

            [Test]
            public async Task NoReplacementWithQuerySelectorReturnsSameDocument()
            {
                // Given
                string inputContent = $"<div>@x.Select(x => x) <code>Foo bar</code></div>";
                TestDocument document = new TestDocument(inputContent);
                Dictionary<string, string> links = new Dictionary<string, string>();
                InsertLinks autoLink = new InsertLinks(links)
                    .WithQuerySelector("code")
                    .WithMatchOnlyWholeWord()
                    .WithStartWordSeparators('<')
                    .WithEndWordSeparators('>');

                // When
                TestDocument result = await ExecuteAsync(document, autoLink).SingleAsync();

                // Then
                result.ShouldBeSameAs(document);
            }
        }
    }
}