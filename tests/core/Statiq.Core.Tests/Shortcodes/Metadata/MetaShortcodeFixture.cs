using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Shortcodes.Metadata
{
    [TestFixture]
    public class MetaShortcodeFixture : BaseFixture
    {
        public class ExecuteTests : MetaShortcodeFixture
        {
            [Test]
            public void RendersMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "Bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "Foo")
                };
                MetaShortcode shortcode = new MetaShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                result.ContentProvider.GetStream().ReadToEnd().ShouldBe("Bar");
            }

            [Test]
            public void EmptyForMissingMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "Bar" }
                });
                KeyValuePair<string, string>[] args = new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>(null, "Fizz")
                };
                MetaShortcode shortcode = new MetaShortcode();

                // When
                ShortcodeResult result = shortcode.Execute(args, string.Empty, document, context);

                // Then
                result.ShouldBeNull();
            }
        }
    }
}
