using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Markdown.Tests
{
    [TestFixture]
    public class MarkdownShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : MarkdownShortcodeFixture
        {
            [Test]
            public void RendersMarkdown()
            {
                // Given
                const string content = @"Line 1
*Line 2*
# Line 3";
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[] { };
                MarkdownShortcode shortcode = new MarkdownShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, content, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(
                    @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}