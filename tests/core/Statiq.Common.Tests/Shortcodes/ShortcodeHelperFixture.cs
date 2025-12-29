using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Shortcodes
{
    [TestFixture]
    public class ShortcodeHelperFixture : BaseFixture
    {
        public class SplitArgumentsTests : ShortcodeHelperFixture
        {
            [Test]
            public void ShouldIgnoreLeadingAndTrailingWhiteSpace()
            {
                // Given, When
                KeyValuePair<string, string>[] result = ShortcodeHelper.SplitArguments("  foo  fizz=buzz  ", 0).ToArray();

                // Then
                result.ShouldBe(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "foo"),
                    new KeyValuePair<string, string>("fizz", "buzz")
                });
            }

            [TestCase("foo", null, "foo")]
            [TestCase("=foo", null, "foo")]
            [TestCase("foo=bar", "foo", "bar")]
            [TestCase("\"fizz buzz\"=bar", "fizz buzz", "bar")]
            [TestCase("foo=\"bar baz\"", "foo", "bar baz")]
            [TestCase("\"fizz buzz\"=\"bar baz\"", "fizz buzz", "bar baz")]
            [TestCase("\"fizz \\\" buzz\"=bar", "fizz \" buzz", "bar")]
            [TestCase("foo=\"bar \\\" baz\"", "foo", "bar \" baz")]
            [TestCase("\"fizz \\\" buzz\"=\"bar \\\" baz\"", "fizz \" buzz", "bar \" baz")]
            [TestCase("\"fizz = buzz\"=bar", "fizz = buzz", "bar")]
            [TestCase("foo=\"bar = baz\"", "foo", "bar = baz")]
            [TestCase("\"fizz = buzz\"=\"bar = baz\"", "fizz = buzz", "bar = baz")]
            public void ShouldSplitArguments(string arguments, string expectedKey, string expectedValue)
            {
                // Given, When
                KeyValuePair<string, string>[] result = ShortcodeHelper.SplitArguments(arguments, 0).ToArray();

                // Then
                result.ShouldBe(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(expectedKey, expectedValue)
                });
            }

            [Test]
            public void ShouldSplitComplexArguments()
            {
                // Given, When
                KeyValuePair<string, string>[] result = ShortcodeHelper.SplitArguments("foo \"abc 123\" fizz=buzz  \"qwe\"=\"try\"\r\nxyz=\"zyx\"  \"678=987\" goo=boo", 0).ToArray();

                // Then
                result.ShouldBe(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "foo"),
                    new KeyValuePair<string, string>(null, "abc 123"),
                    new KeyValuePair<string, string>("fizz", "buzz"),
                    new KeyValuePair<string, string>("qwe", "try"),
                    new KeyValuePair<string, string>("xyz", "zyx"),
                    new KeyValuePair<string, string>(null, "678=987"),
                    new KeyValuePair<string, string>("goo", "boo")
                });
            }
        }
    }
}
