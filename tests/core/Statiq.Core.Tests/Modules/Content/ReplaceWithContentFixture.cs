using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ReplaceWithContentFixture : BaseFixture
    {
        public class ExecuteTests : ReplaceWithContentFixture
        {
            [Test]
            public async Task KeepsExistingMediaType()
            {
                // Given
                TestDocument document = new TestDocument("ABC", "Foo");
                ReplaceWithContent replace = new ReplaceWithContent("ABC", "123");

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.ContentProvider.MediaType.ShouldBe("Foo");
            }

            [Test]
            public async Task ReplaceWithContent()
            {
                // Given
                const string search = @"<html>
                            <head>
                                <title>Foobar</title>
                            </head>
                            <body>
                                <span>fizzbuzz</span>
                            </body>
                        </html>";
                const string expected = @"<html>
                            <head>
                                <title>Foobar</title>
                            </head>
                            <body>
                                <span>abc123</span>
                            </body>
                        </html>";
                TestDocument document = new TestDocument("abc123");
                ReplaceWithContent replace = new ReplaceWithContent("fizzbuzz", Config.FromValue(search));

                // When
                TestDocument result = await ExecuteAsync(document, replace).SingleAsync();

                // Then
                result.Content.ShouldBe(expected);
            }
        }
    }
}
