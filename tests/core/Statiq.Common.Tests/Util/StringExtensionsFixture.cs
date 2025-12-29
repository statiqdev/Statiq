using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    [TestFixture]
    public class StringExtensionsFixture : BaseFixture
    {
        public class RemoveHtmlAndSpecialCharsTests : StringExtensionsFixture
        {
            [TestCase("abcde", "abcde")]
            [TestCase("  abcde ", "abcde")]
            [TestCase("<p>abcd</p>", "abcd")]
            [TestCase("<p>abcd", "abcd")]
            [TestCase("abc<p>de", "abc de")]
            [TestCase("abc<p>d</p>e", "abc d e")]
            [TestCase("abc de  fg", "abc de fg")]
            [TestCase("abc <p>de</p> fg", "abc de fg")]
            [TestCase("<xyz>a b c</xyz> d e", "a b c d e")]
            [TestCase("ab\rcd\nef\tg h\r\nij", "ab cd ef g h ij")]
            [TestCase("ab\r\n<p>cd</p>\r\n<h1>e f</h1><small>gh</small>\r\ni", "ab cd e f gh i")]
            [TestCase("<img src=\"123.jpg\">", "")]
            [TestCase("<p>", "")]
            [TestCase("<p></p>", "")]
            [TestCase("<img src=\"123.jpg\">foo</img>", "foo")]
            public void RemovesHtmlAndSpecialChars(string value, string expected)
            {
                // Given, When
                string result = value.RemoveHtmlAndSpecialChars();

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("abcde", "abcde")]
            [TestCase("  abcde ", "abcde")]
            [TestCase("<p>abcd</p>", "()abcd()")]
            [TestCase("<p>abcd", "()abcd")]
            [TestCase("abc<p>de", "abc()de")]
            [TestCase("abc<p>d</p>e", "abc()d()e")]
            [TestCase("abc de  fg", "abc de fg")]
            [TestCase("abc <p>de</p> fg", "abc ()de() fg")]
            [TestCase("<xyz>a b c</xyz> d e", "()a b c() d e")]
            [TestCase("ab\rcd\nef\tg h\r\nij", "ab()cd()ef()g h()()ij")]
            [TestCase("ab\r\n<p>c d</p>\r\n<h1>ef</h1><small>gh</small>\r\ni", "ab()()()c d()()()()ef()()gh()()()i")]
            [TestCase("<img src=\"123.jpg\">", "()")]
            [TestCase("<p>", "()")]
            [TestCase("<p></p>", "()()")]
            [TestCase("<img src=\"123.jpg\">foo</img>", "()foo()")]
            public void ReplacesWithAlternateString(string value, string expected)
            {
                // Given, When
                string result = value.RemoveHtmlAndSpecialChars("()");

                // Then
                result.ShouldBe(expected);
            }
        }

        public class RemoveStartTests : StringExtensionsFixture
        {
            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "FooBar")]
            [TestCase("FooBar", "Foo", "Bar")]
            [TestCase("FooBar", "foo", "FooBar")]
            [TestCase("FooBar", "f", "FooBar")]
            [TestCase("FooBar", "F", "ooBar")]
            public void WithoutStringComparison(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveStart(value);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "FooBar")]
            [TestCase("FooBar", "Foo", "Bar")]
            [TestCase("FooBar", "foo", "Bar")]
            [TestCase("FooBar", "f", "ooBar")]
            [TestCase("FooBar", "F", "ooBar")]
            public void OrdinalIgnoreCase(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveStart(value, StringComparison.OrdinalIgnoreCase);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class RemoveEndTests : StringExtensionsFixture
        {
            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "Foo")]
            [TestCase("FooBar", "Foo", "FooBar")]
            [TestCase("FooBar", "bar", "FooBar")]
            [TestCase("FooBar", "R", "FooBar")]
            [TestCase("FooBar", "r", "FooBa")]
            public void WithoutStringComparison(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveEnd(value);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("FooBar", null, "FooBar")]
            [TestCase("FooBar", "", "FooBar")]
            [TestCase(null, null, null)]
            [TestCase(null, "", null)]
            [TestCase(null, "Foo", null)]
            [TestCase("", null, "")]
            [TestCase("", "Foo", "")]
            [TestCase("FooBar", "Bar", "Foo")]
            [TestCase("FooBar", "Foo", "FooBar")]
            [TestCase("FooBar", "bar", "Foo")]
            [TestCase("FooBar", "R", "FooBa")]
            [TestCase("FooBar", "r", "FooBa")]
            public void OrdinalIgnoreCase(string input, string value, string expected)
            {
                // Given, When
                string result = input.RemoveEnd(value, StringComparison.OrdinalIgnoreCase);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class ToLowerCamelCaseTests : StringExtensionsFixture
        {
            [TestCase("FooBar", "fooBar")]
            [TestCase("fooBar", "fooBar")]
            [TestCase("foobar", "foobar")]
            [TestCase("", "")]
            [TestCase("F", "f")]
            [TestCase("f", "f")]
            public void ToLowerCamelCaseCorrectlyConvertsString(string input, string expected)
            {
                // Given, When
                string result = input.ToLowerCamelCase();

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}
