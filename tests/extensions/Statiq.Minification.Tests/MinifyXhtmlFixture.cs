using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Minification.Tests
{
    [TestFixture]
    public class MinifyXhtmlFixture : BaseFixture
    {
        public class ExecuteTests : MinifyXhtmlFixture
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
                const string output = "<html><head><title>Title</title></head><body><h1>Title</h1><p>This is<br />some text</p></body></html>";
                TestDocument document = new TestDocument(input);
                MinifyXhtml minifyXhtml = new MinifyXhtml();

                // When
                TestDocument result = await ExecuteAsync(document, minifyXhtml).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}