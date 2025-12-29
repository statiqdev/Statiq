using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class MakeLinksRootRelativeFixture : BaseFixture
    {
        public class ExecuteTests : MakeLinksRootRelativeFixture
        {
            [TestCase("//fizz/buzz", "/fizz/buzz")]
            [TestCase("fizz/buzz", "/foo/bar/fizz/buzz")]
            [TestCase("../fizz/buzz", "/foo/fizz/buzz")]
            [TestCase("../fizz/buzz.html", "/foo/fizz/buzz.html")]
            public async Task MakesLinksRootRelative(string relative, string absolute)
            {
                // Given
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
                MakeLinksRootRelative module = new MakeLinksRootRelative();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    $@"<html><head></head><body>
                        <div>
                          <p>Foo <a href=""{absolute}"">Fizzbuzz</a> Bar</p>
                        </div>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [TestCase("//fizz/buzz.png", "/fizz/buzz.png")]
            [TestCase("../fizz/buzz.jpg", "/foo/fizz/buzz.jpg")]
            [TestCase("fizz/buzz.png", "/foo/bar/fizz/buzz.png")]
            public async Task MakesImagesRootRelative(string relative, string absolute)
            {
                // Given
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
                MakeLinksRootRelative module = new MakeLinksRootRelative();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    $@"<html><head></head><body>
                        <div>
                          <p>Foo <img src=""{absolute}""> Bar</p>
                        </div>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task UsesVirtualPath()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkRoot] = "/virtual";
                TestDocument document = new TestDocument(
                    NormalizedPath.Null,
                    new NormalizedPath("foo/bar/baz.html"),
                    $@"<html>
                      <body>
                        <div>
                          <p>Foo <a href=""../fizz/buzz"">Fizzbuzz</a> Bar</p>
                        </div>
                      </body>
                    </html>");
                MakeLinksRootRelative module = new MakeLinksRootRelative();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    $@"<html><head></head><body>
                        <div>
                          <p>Foo <a href=""/virtual/foo/fizz/buzz"">Fizzbuzz</a> Bar</p>
                        </div>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [TestCase("https://www.google.com")]
            [TestCase("mailto:foo@bar.com")]
            public async Task DoesNotModifyNonRelativeLinks(string href)
            {
                // Given
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
                MakeLinksRootRelative module = new MakeLinksRootRelative();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(content, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotModifyAnchor()
            {
                // Given
                string content = $@"<html>
                      <body>
                        <div>
                          <p>Foo <a id=""foo""></a> Bar</p>
                        </div>
                      </body>
                    </html>";
                TestDocument document = new TestDocument(
                    NormalizedPath.Null,
                    new NormalizedPath("foo/bar/baz.html"),
                    content);
                MakeLinksRootRelative module = new MakeLinksRootRelative();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(content, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}