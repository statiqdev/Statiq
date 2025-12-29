using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Minification.Tests
{
    [TestFixture]
    public class MinifyXmlFixture : BaseFixture
    {
        public class ExecuteTests : MinifyXmlFixture
        {
            [Test]
            public async Task Minify()
            {
                // Given
                const string input = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
                        <urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">
                            <!-- Homepage -->
                            <url>
                                <loc>https://statiq.dev/</loc>
                                <changefreq>weekly</changefreq>
                                <priority>0.9</priority>
                            </url>
                            <!-- Content Page -->
                            <url>
                                <loc>https://statiq.dev/modules/minifyxml</loc>
                                <changefreq>monthly</changefreq>
                                <priority>0.7</priority>
                            </url>
                        </urlset>";
                const string output = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?><urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""><url><loc>https://statiq.dev/</loc><changefreq>weekly</changefreq><priority>0.9</priority></url><url><loc>https://statiq.dev/modules/minifyxml</loc><changefreq>monthly</changefreq><priority>0.7</priority></url></urlset>";
                TestDocument document = new TestDocument(input);
                MinifyXml minifyXml = new MinifyXml();

                // When
                TestDocument result = await ExecuteAsync(document, minifyXml).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}