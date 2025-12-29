using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Minification.Tests
{
    [TestFixture]
    public class MinifyHtmlFixture : BaseFixture
    {
        public class ExecuteTests : MinifyHtmlFixture
        {
            [Test]
            public async Task Minify()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Title</title>
                        </head>
                        <body>
                            <!-- FOO -->
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                const string output = "<html><head><title>Title</title><body><h1>Title</h1><p>This is<br>some text";
                TestDocument document = new TestDocument(input);
                MinifyHtml minifyHtml = new MinifyHtml();

                // When
                TestDocument result = await ExecuteAsync(document, minifyHtml).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task MinifyWithCustomSettings()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Title</title>
                        </head>
                        <body>
                            <!-- FOO -->
                            <h1>Title</h1>
                            <p>This is<br />some text</p>
                        </body>
                    </html>";
                const string output = "<html><head><title>Title</title></head><body><!-- FOO --><h1>Title</h1><p>This is<br>some text</p></body></html>";
                TestDocument document = new TestDocument(input);
                MinifyHtml minifyHtml = new MinifyHtml()
                    .WithSettings(settings =>
                    {
                        settings.RemoveOptionalEndTags = false;
                        settings.RemoveHtmlComments = false;
                    });

                // When
                TestDocument result = await ExecuteAsync(document, minifyHtml).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}