using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Shortcodes.Control
{
    [TestFixture]
    public class ForEachShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : ForEachShortcodeFixture
        {
            [Test]
            public void ThrowsForInvalidValueKey()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", string.Empty)
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When, Then
                Should.Throw<ShortcodeArgumentException>(() => shortcode.Execute(args, string.Empty, document, context));
            }

            [Test]
            public void IteratesItems()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", new int[] { 5, 6, 7 } }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<ShortcodeResult> result = shortcode.Execute(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldAllBe(x => x.ContentProvider.GetStream().ReadToEnd() == "Fizzbuzz");
                result.Select(x => x.NestedMetadata["Bar"]).ShouldBe(new object[] { 5, 6, 7 });
            }

            [Test]
            public void IteratesSingleItem()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", 5 }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<ShortcodeResult> result = shortcode.Execute(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldAllBe(x => x.ContentProvider.GetStream().ReadToEnd() == "Fizzbuzz");
                result.Select(x => x.NestedMetadata["Bar"]).ShouldBe(new object[] { 5 });
            }

            [Test]
            public void MissingKey()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument();
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "Foo"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<ShortcodeResult> result = shortcode.Execute(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public void IteratesScript()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", 5 }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("Key", "=> new int[] { 1, 2, (int)Get(\"Foo\") + 1 }"),
                    new KeyValuePair<string, string>("ValueKey", "Bar")
                };
                ForEachShortcode shortcode = new ForEachShortcode();

                // When
                IEnumerable<ShortcodeResult> result = shortcode.Execute(args, "Fizzbuzz", document, context);

                // Then
                result.ShouldAllBe(x => x.ContentProvider.GetStream().ReadToEnd() == "Fizzbuzz");
                result.Select(x => x.NestedMetadata["Bar"]).ShouldBe(new object[] { 1, 2, 6 });
            }
        }
    }
}
