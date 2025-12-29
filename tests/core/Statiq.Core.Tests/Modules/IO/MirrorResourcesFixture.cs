using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class MirrorResourcesFixture : BaseFixture
    {
        public class ExecuteTests : MirrorResourcesFixture
        {
            [Test]
            public async Task ReplacesScriptResource()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/mirror/cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ReplacesScriptResourceForDifferentHost()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, "www.foo.com");
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/mirror/cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ReplacesScriptResourceForDifferentDocumentHost()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                document.Add(Keys.Host, "www.foo.com");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/mirror/cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [TestCase("cdn.jsdelivr.net")]
            [TestCase("CDN.JsDeliVR.net")]
            public async Task DoesNotReplaceScriptResourceForSameHost(string host)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings.Add(Keys.Host, host);
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [TestCase("cdn.jsdelivr.net")]
            [TestCase("CDN.JsDeliVR.net")]
            public async Task DoesNotReplaceScriptResourceForSameDocumentHost(string host)
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                document.Add(Keys.Host, host);
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task KeepsIndexFileName()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.LinkHideIndexPages] = "true";
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/index.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/mirror/cdn.jsdelivr.net/npm/es6-promise/dist/index.min.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ReplacesLinkResource()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <link rel=""stylesheet"" href=""/mirror/cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"">
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotReplaceCanonicalLinkResource()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""canonical"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <link rel=""canonical"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ReplacesLinkResourceWithMultipleRel()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""prefetch stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <link rel=""prefetch stylesheet"" href=""/mirror/cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"">
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotReplaceLinkResourceWithMultipleRel()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""prefetch help"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <link rel=""prefetch help"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" />
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotReplaceRelativePaths()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <link rel=""apple-touch-icon"" sizes=""120x120"" href=""/apple-touch-icon.png"">
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <link rel=""apple-touch-icon"" sizes=""120x120"" href=""/apple-touch-icon.png"">
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotReplaceDataNoMirrorAttribute()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js"" integrity=""sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8wC"" crossorigin=""anonymous"" data-no-mirror></script>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" integrity=""sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8wC"" crossorigin=""anonymous"" data-no-mirror />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js"" integrity=""sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8wC"" crossorigin=""anonymous"" data-no-mirror></script>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"" integrity=""sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8wC"" crossorigin=""anonymous"" data-no-mirror />
                      </head>
                      <body>
                      </body>
                    </html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task UsesCustomMirrorPath()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources(_ => new NormalizedPath("/foo/bar.js"));

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/foo/bar.js""></script>
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RemovesSubresourceIntegrityValues()
            {
                // Given
                TestDocument document = new TestDocument(
                    @"<html>
                      <head>
                        <script src=""https://cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js"" integrity=""sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8wC"" crossorigin=""anonymous""></script>
                        <link rel=""stylesheet"" href=""https://cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css""  integrity=""sha384-oqVuAfXRKap7fdgcCY5uykM6+R9GqQ8K/uxy9rx7HNQlGYl1kPzQho1wx4JwY8wC"" crossorigin=""anonymous"" />
                      </head>
                      <body>
                      </body>
                    </html>");
                MirrorResources module = new MirrorResources();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"<html><head>
                        <script src=""/mirror/cdn.jsdelivr.net/npm/es6-promise/dist/es6-promise.auto.min.js""></script>
                        <link rel=""stylesheet"" href=""/mirror/cdn.jsdelivr.net/npm/@@progress/kendo-theme-bootstrap@3.2.0/dist/all.min.css"">
                      </head>
                      <body>
                      
                    </body></html>", StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}