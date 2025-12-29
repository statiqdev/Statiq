using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ReplaceInContentFixture : BaseFixture
    {
        public class ExecuteTests : ReplaceInContentFixture
        {
            [Test]
            public async Task RecursiveReplaceWithContentFinder()
            {
                // Given
                const string input = @"<html>
                            <head>
                                <title>Foobar</title>
                            </head>
                            <body>
                                <span>foo<span>bar</span></span>
                            </body>
                        </html>";
                const string expected = @"<html>
                            <head>
                                <title>Foobar</title>
                            </head>
                            <body>
                                <span>baz</span>
                            </body>
                        </html>";
                TestDocument document = new TestDocument(input);
                ReplaceInContent replace = new ReplaceInContent(@"(<span>.*<\/span>)", _ => "<span>baz</span>");

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.Content.ShouldBe(expected);
            }

            [Test]
            public async Task KeepsExistingMediaType()
            {
                // Given
                TestDocument document = new TestDocument("ABC", "Foo");
                ReplaceInContent replace = new ReplaceInContent("ABC", "123");

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.Content.ShouldBe("123");
                result.ContentProvider.MediaType.ShouldBe("Foo");
            }

            [Test]
            public async Task ReplaceWithContentFinderUsingDocument()
            {
                // Given
                const string input = @"<html>
                            <head>
                                <title>Foobar</title>
                            </head>
                            <body>
                                <span>foo<span>bar</span></span>
                            </body>
                        </html>";
                const string expected = @"<html>
                            <head>
                                <title>Foobar</title>
                            </head>
                            <body>
                                <div>Buzz</div>
                            </body>
                        </html>";
                TestDocument document = new TestDocument(input)
                {
                    { "Fizz", "Buzz" }
                };
                ReplaceInContent replace = new ReplaceInContent(@"(<span>.*<\/span>)", (_, doc) => $"<div>{doc["Fizz"]}</div>");

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.Content.ShouldBe(expected);
            }
        }
    }
}
