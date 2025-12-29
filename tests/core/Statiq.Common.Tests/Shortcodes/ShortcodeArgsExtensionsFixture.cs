using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Shortcodes
{
    [TestFixture]
    public class ShortcodeArgsExtensionsFixture : BaseFixture
    {
        public class ToDictionaryTests : ShortcodeArgsExtensionsFixture
        {
            [Test]
            public void MatchesInCorrectOrder()
            {
                // Given
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>(null, "2"),
                    new KeyValuePair<string, string>(null, "3")
                };

                // When
                IMetadataDictionary dictionary = args.ToDictionary("A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void MatchesNamedArguments()
            {
                // Given
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("B", "2"),
                    new KeyValuePair<string, string>("A", "1"),
                    new KeyValuePair<string, string>("C", "3")
                };

                // When
                IMetadataDictionary dictionary = args.ToDictionary("A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void MatchesPositionalAndNamedArguments()
            {
                // Given
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>("B", "2")
                };

                // When
                IMetadataDictionary dictionary = args.ToDictionary("A", "B", "C");

                // Then
                dictionary.ShouldBe(
                    new KeyValuePair<string, object>[]
                    {
                        new KeyValuePair<string, object>("A", "1"),
                        new KeyValuePair<string, object>("B", "2"),
                        new KeyValuePair<string, object>("C", "3")
                    },
                    true);
            }

            [Test]
            public void ThrowsForPositionalAfterNamedArguments()
            {
                // Given
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>(null, "2")
                };

                // When, Then
                Should.Throw<ShortcodeArgumentException>(() => args.ToDictionary("A", "B", "C"));
            }

            [Test]
            public void ThrowsForDuplicateNamedArguments()
            {
                // Given
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "1"),
                    new KeyValuePair<string, string>("C", "3"),
                    new KeyValuePair<string, string>("A", "2")
                };

                // When, Then
                Should.Throw<ShortcodeArgumentException>(() => args.ToDictionary("A", "B", "C"));
            }
        }
    }
}
