using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.App.Tests.Commands
{
    [TestFixture]
    public class SettingsParserFixture : BaseFixture
    {
        public class ParseTests : SettingsParserFixture
        {
            [Test]
            public void KeyOnlyParse()
            {
                // Given
                string[] expected = { "hi", "=hello", "\\=abcd", "key\\=val", "     bjorn  \\=   dad" };

                // When
                IReadOnlyDictionary<string, string> args = SettingsParser.Parse(expected);

                // Then
                expected.Length.ShouldBe(args.Count);
                int i = 0;
                foreach (KeyValuePair<string, string> arg in args)
                {
                    expected[i].Replace("\\=", "=").Trim().ShouldBe(arg.Key);
                    arg.Value.ShouldBe("true");
                    i++;
                }
            }

            [Test]
            public void KeyValueParse()
            {
                // Given
                string[] pairs = { "key=value", "k=v", "except=bro", "awesome====123123", "   keytrimmed    =    value trimmed   " };

                // When
                IReadOnlyDictionary<string, string> args = SettingsParser.Parse(pairs);

                // Then
                pairs.Length.ShouldBe(args.Count);
                foreach (KeyValuePair<string, string> arg in args)
                {
                    arg.Value.ShouldNotBeNull("Argument value should not be null.");
                    arg.Key.ShouldNotStartWith(" ", Case.Insensitive, "Arguments key should be trimmed.");
                    arg.Key.ShouldNotEndWith(" ", "Arguments key should be trimmed.");
                    arg.Value.ShouldNotStartWith(" ", Case.Insensitive, "Arguments value should be trimmed.");
                    arg.Value.ShouldNotEndWith(" ", "Arguments value should be trimmed.");
                }
            }

            [Test]
            public void KeyCollision()
            {
                // Given, When, Then
                Should.Throw<ArgumentException>(
                    () => SettingsParser.Parse(new[] { "hello=world", "hello=exception" }));
            }
        }
    }
}
