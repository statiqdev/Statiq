using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class GenerateSitemapFixture : BaseFixture
    {
        public class ExecuteTests : GenerateSitemapFixture
        {
            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
            public async Task SitemapGeneratedWithSitemapItem(string hostname, string formatterString, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkHideExtensions] = "true";
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    context.Settings[Keys.Host] = hostname;
                }

                TestDocument doc = new TestDocument(new NormalizedPath("sub/testfile.html"), "Test");
                IDocument[] inputs = { doc };

                SetMetadata m = new SetMetadata(
                    Keys.SitemapItem,
                    Config.FromDocument(d => new SitemapItem(d.Destination.FullPath)));

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                GenerateSitemap sitemap = new GenerateSitemap(formatter);

                // When
                TestDocument result = await ExecuteAsync(doc, context, m, sitemap).SingleAsync();

                // Then
                result.Content.ShouldContain($"<loc>{expected}</loc>");
            }

            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com/{0}", "http://www.example.com/sub/testfile.html")]
            public async Task SitemapGeneratedWithSitemapItemAsString(string hostname, string formatterString, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkHideExtensions] = "true";
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    context.Settings[Keys.Host] = hostname;
                }

                TestDocument doc = new TestDocument(new NormalizedPath("sub/testfile.html"), "Test");
                IDocument[] inputs = { doc };

                SetMetadata m = new SetMetadata(
                    Keys.SitemapItem,
                    Config.FromDocument(d => d.Destination.FullPath));

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                GenerateSitemap sitemap = new GenerateSitemap(formatter);

                // When
                TestDocument result = await ExecuteAsync(doc, context, m, sitemap).SingleAsync();

                // Then
                result.Content.ShouldContain($"<loc>{expected}</loc>");
            }

            [TestCase("www.example.org", null, "http://www.example.org/sub/testfile")]
            [TestCase(null, "http://www.example.com", "http://www.example.com")]
            [TestCase("www.example.org", "http://www.example.com{0}", "http://www.example.com/sub/testfile")]
            public async Task SitemapGeneratedWhenNoSitemapItem(string hostname, string formatterString, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkHideExtensions] = "true";
                if (!string.IsNullOrWhiteSpace(hostname))
                {
                    context.Settings[Keys.Host] = hostname;
                }

                TestDocument doc = new TestDocument(new NormalizedPath("sub/testfile.html"), "Test");

                Func<string, string> formatter = null;

                if (!string.IsNullOrWhiteSpace(formatterString))
                {
                    formatter = f => string.Format(formatterString, f);
                }

                GenerateSitemap sitemap = new GenerateSitemap(formatter);

                // When
                TestDocument result = await ExecuteAsync(doc, context, sitemap).SingleAsync();

                // Then
                result.Content.ShouldContain($"<loc>{expected}</loc>");
            }

            [Test]
            public async Task DoesNotIncludeDuplicatedItems()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkHideExtensions] = "true";
                context.Settings[Keys.Host] = "www.example.org";
                TestDocument[] inputs =
                {
                    new TestDocument(new NormalizedPath("sub/testfile.html"), "Test"),
                    new TestDocument(new NormalizedPath("sub/testfile2.html"), "Test2"),
                    new TestDocument(new NormalizedPath("sub/testfile.html"), "Test")
                };
                GenerateSitemap sitemap = new GenerateSitemap();

                // When
                TestDocument result = await ExecuteAsync(inputs, context, sitemap).SingleAsync();

                // Then
                result.Content.ShouldBe(@"<?xml version=""1.0"" encoding=""UTF-8""?><urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""><url><loc>http://www.example.org/sub/testfile</loc></url><url><loc>http://www.example.org/sub/testfile2</loc></url></urlset>");
            }

            [Test]
            public async Task LinkRootSettingsSiteMap()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkHideExtensions] = "true";
                context.Settings[Keys.Host] = "www.example.org";
                context.Settings[Keys.LinkRoot] = "linkRoot";
                TestDocument[] inputs =
                {
                    new TestDocument(new NormalizedPath("sub/testfile.html"), "Test"),
                    new TestDocument(new NormalizedPath("sub/testfile2.html"), "Test2"),
                    new TestDocument(new NormalizedPath("sub/testfile.html"), "Test")
                };
                GenerateSitemap sitemap = new GenerateSitemap();

                // When
                TestDocument result = await ExecuteAsync(inputs, context, sitemap).SingleAsync();

                // Then
                result.Content.ShouldBe(@"<?xml version=""1.0"" encoding=""UTF-8""?><urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9""><url><loc>http://www.example.org/linkRoot/sub/testfile</loc></url><url><loc>http://www.example.org/linkRoot/sub/testfile2</loc></url></urlset>");
            }
        }
    }
}