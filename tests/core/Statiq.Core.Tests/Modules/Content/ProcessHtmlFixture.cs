using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ProcessHtmlFixture : BaseFixture
    {
        public class ExecuteTests : ProcessHtmlFixture
        {
            [Test]
            public async Task DoesNotCloneDocumentIfNothingChanged()
            {
                // Given
                const string input =
                    @"<html>
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
                ConcurrentBag<string> content = new ConcurrentBag<string>();
                ProcessHtml process = new ProcessHtml("p", x => content.Add(x.TextContent));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, process);

                // Then
                content.ShouldBe(
                    new[]
                    {
                        "This is some Foobar text",
                        "This is some other text"
                    },
                    true);
                TestDocument result = results.ShouldHaveSingleItem();
                result.ShouldBe(document);
            }

            [Test]
            public async Task ChangesContent()
            {
                // Given
                const string input =
                    @"<html>
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
                ProcessHtml process = new ProcessHtml("p", x => x.Insert(AngleSharp.Dom.AdjacentPosition.AfterEnd, "Fuzz"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, process);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                result.ShouldNotBe(document);
                result.Content.ShouldBe(
                    @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>Fuzz
                            <p>This is some other text</p>Fuzz
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task PreservesShortcode()
            {
                // Given
                const string input =
                    @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p><?! Foo /?></p>
                            <p>This is some other text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                ProcessHtml process = new ProcessHtml("p", x => x.Insert(AngleSharp.Dom.AdjacentPosition.AfterEnd, "Fuzz"));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, process);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                result.ShouldNotBe(document);
                result.Content.ShouldBe(
                    @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p><?! Foo /?></p>Fuzz
                            <p>This is some other text</p>Fuzz
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AddsMetadata()
            {
                // Given
                const string input =
                    @"<html>
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
                int c = 1;
                ProcessHtml process = new ProcessHtml("p", (e, m) => m.Add("Foo" + c++.ToString(), e.TextContent));

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, process);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                result.ShouldNotBe(document);
                result.Content.ShouldBe(
                    @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                            <p>This is some other text</p>
                        </body>
                    </html>",
                    StringCompareShould.IgnoreLineEndings);
                result["Foo1"].ShouldBe("This is some Foobar text");
                result["Foo2"].ShouldBe("This is some other text");
            }

            [Test]
            public async Task ChangesContentAndMetadata()
            {
                // Given
                const string input =
                    @"<html>
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
                int c = 1;
                ProcessHtml process = new ProcessHtml("p", (e, m) =>
                {
                    e.Insert(AngleSharp.Dom.AdjacentPosition.AfterEnd, "Fuzz");
                    m.Add("Foo" + c++.ToString(), e.TextContent);
                });

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, process);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                result.ShouldNotBe(document);
                result.Content.ShouldBe(
                    @"<html><head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>Fuzz
                            <p>This is some other text</p>Fuzz
                        
                    </body></html>",
                    StringCompareShould.IgnoreLineEndings);
                result["Foo1"].ShouldBe("This is some Foobar text");
                result["Foo2"].ShouldBe("This is some other text");
            }
        }
    }
}