using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;
using Statiq.Testing.JavaScript;

namespace Statiq.Highlight.Tests
{
    [TestFixture]
    public class HighlightShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : HighlightCodeFixture
        {
            [TestCase(true, true)]
            [TestCase(false, false)]
            [TestCase(null, false)]
            public void ShouldAddPreElement(bool? addPre, bool expectedPre)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };
                TestDocument document = new TestDocument();
                Dictionary<string, string> args = new Dictionary<string, string>();
                if (addPre.HasValue)
                {
                    args.Add("AddPre", addPre.Value.ToString());
                }
                HighlightShortcode shortcode = new HighlightShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args.ToArray(), "int foo = 3;", document, context);

                // Then
                string expected = "<code class=\"language-cpp hljs\"><span class=\"hljs-type\">int</span> foo = <span class=\"hljs-number\">3</span>;</code>";
                if (expectedPre)
                {
                    expected = $"<pre>{expected}</pre>";
                }
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(expected);
            }

            [TestCase(true, true)]
            [TestCase(false, false)]
            [TestCase(null, true)]
            public void ShouldAddPreElementWhenNewLine(bool? addPre, bool expectedPre)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };
                TestDocument document = new TestDocument();
                Dictionary<string, string> args = new Dictionary<string, string>();
                if (addPre.HasValue)
                {
                    args.Add("AddPre", addPre.Value.ToString());
                }
                HighlightShortcode shortcode = new HighlightShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args.ToArray(), "int foo = 3;\r\nint bar = 6;", document, context);

                // Then
                string expected = "<code class=\"language-cpp hljs\"><span class=\"hljs-type\">int</span> foo = <span class=\"hljs-number\">3</span>;\n<span class=\"hljs-type\">int</span> bar = <span class=\"hljs-number\">6</span>;</code>";
                if (expectedPre)
                {
                    expected = $"<pre>{expected}</pre>";
                }
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public void ShouldChangeLanguage()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };
                TestDocument document = new TestDocument();
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "Language", "csharp" }
                };
                HighlightShortcode shortcode = new HighlightShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args.ToArray(), "int foo = 3;", document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(
                    "<code class=\"language-csharp hljs\"><span class=\"hljs-built_in\">int</span> foo = <span class=\"hljs-number\">3</span>;</code>");
            }

            [Test]
            public void ShouldChangeWrappingElement()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    JsEngineFunc = () => new TestJsEngine()
                };
                TestDocument document = new TestDocument();
                Dictionary<string, string> args = new Dictionary<string, string>
                {
                    { "Element", "div" }
                };
                HighlightShortcode shortcode = new HighlightShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args.ToArray(), "int foo = 3;", document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe(
                    "<div class=\"language-cpp hljs\"><span class=\"hljs-type\">int</span> foo = <span class=\"hljs-number\">3</span>;</div>");
            }
        }
    }
}