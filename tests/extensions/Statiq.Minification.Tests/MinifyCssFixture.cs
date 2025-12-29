using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Minification.Tests
{
    [TestFixture]
    public class MinifyCssFixture : BaseFixture
    {
        public class ExecuteTests : MinifyCssFixture
        {
            [Test]
            public async Task Minify()
            {
                // Given
                // Example taken from http://yui.github.io/yuicompressor/css.html
                const string input = @"
/*****
  Multi-line comment
  before a new class name
*****/
.classname {
    /* comment in declaration block */
    font-weight: normal;
}";
                const string output = ".classname{font-weight:normal}";
                TestDocument document = new TestDocument(input);
                MinifyCss minifyCss = new MinifyCss();

                // When
                TestDocument result = await ExecuteAsync(document, minifyCss).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}