using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class MakeLinksAbsoluteFixture : BaseFixture
    {
        public class ExecuteTests : MakeLinksAbsoluteFixture
        {
            [TestCase("/fizz/buzz", "http://statiq.dev/fizz/buzz")]
            [TestCase("//fizz/buzz", "http://statiq.dev/fizz/buzz")]
            [TestCase("fizz/buzz", "http://statiq.dev/foo/bar/fizz/buzz")]
            [TestCase("../fizz/buzz", "http://statiq.dev/foo/fizz/buzz")]
            [TestCase("../fizz/buzz.html", "http://statiq.dev/foo/fizz/buzz.html")]
            public async Task MakesLinksAbsolute(string relative, string absolute)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument document = new TestDocument(
                    NormalizedPath.Null,
                    new NormalizedPath("foo/bar/baz.html"),
                    $@"<html>
                      <body>
                        <div>
                          <p>Foo <a href=""{relative}"">Fizzbuzz</a> Bar</p>
                        </div>
                      </body>
                    </html>");
                MakeLinksAbsolute module = new MakeLinksAbsolute();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    $@"<html><head></head><body>
                        <div>
                          <p>Foo <a href=""{absolute}"">Fizzbuzz</a> Bar</p>
                        </div>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [TestCase("/fizz/buzz.jpg", "http://statiq.dev/fizz/buzz.jpg")]
            [TestCase("//fizz/buzz.png", "http://statiq.dev/fizz/buzz.png")]
            [TestCase("fizz/buzz.gif", "http://statiq.dev/foo/bar/fizz/buzz.gif")]
            [TestCase("../fizz/buzz.jpg", "http://statiq.dev/foo/fizz/buzz.jpg")]
            public async Task MakesImagesAbsolute(string relative, string absolute)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument document = new TestDocument(
                    NormalizedPath.Null,
                    new NormalizedPath("foo/bar/baz.html"),
                    $@"<html>
                      <body>
                        <div>
                          <p>Foo <img src=""{relative}""> Bar</p>
                        </div>
                      </body>
                    </html>");
                MakeLinksAbsolute module = new MakeLinksAbsolute();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    $@"<html><head></head><body>
                        <div>
                          <p>Foo <img src=""{absolute}""> Bar</p>
                        </div>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [TestCase("https://www.google.com")]
            [TestCase("mailto:foo@bar.com")]
            public async Task DoesNotModifyNonRelativeLinks(string href)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                string content = $@"<html>
                      <body>
                        <div>
                          <p>Foo <a href=""{href}"">Fizzbuzz</a> Bar</p>
                        </div>
                      </body>
                    </html>";
                TestDocument document = new TestDocument(
                    NormalizedPath.Null,
                    new NormalizedPath("foo/bar/baz.html"),
                    content);
                MakeLinksAbsolute module = new MakeLinksAbsolute();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(content, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotModifyAnchor()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument document = new TestDocument(
                    NormalizedPath.Null,
                    new NormalizedPath("foo/bar/baz.html"),
                    $@"<html>
                      <body>
                        <div>
                          <p>Foo <a id=""foo""></a> Bar</p>
                        </div>
                      </body>
                    </html>");
                MakeLinksAbsolute module = new MakeLinksAbsolute();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <body>
                        <div>
                          <p>Foo <a id=""foo""></a> Bar</p>
                        </div>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task UsesLinkSettings()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                context.Settings[Keys.LinksUseHttps] = "true";
                context.Settings[Keys.LinkRoot] = "abc";
                TestDocument document = new TestDocument(
                    NormalizedPath.Null,
                    new NormalizedPath("foo/bar/baz.html"),
                    $@"<html>
                      <body>
                        <div>
                          <p>Foo <a href=""/fizz/buzz"">Fizzbuzz</a> Bar</p>
                        </div>
                      </body>
                    </html>");
                MakeLinksAbsolute module = new MakeLinksAbsolute();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head></head><body>
                        <div>
                          <p>Foo <a href=""https://statiq.dev/abc/fizz/buzz"">Fizzbuzz</a> Bar</p>
                        </div>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}