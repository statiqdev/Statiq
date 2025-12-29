using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Scriban.Parsing;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Scriban.Tests
{
    [TestFixture]
    public class RenderScribanFixture : BaseFixture
    {
        public class ExecuteTests : RenderScribanFixture
        {
            [Test]
            public async Task RendersScriban()
            {
                // Given
                const string input = @"<div class=""entry"">
  <h1>{{title}}</h1>
  <div class=""body"">
    {{body}}
  </div>
</div>";
                const string output = @"<div class=""entry"">
  <h1>My New Post</h1>
  <div class=""body"">
    This is my first post!
  </div>
</div>";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { Keys.Title, "My New Post" },
                        { "Body", "This is my first post!" }
                    },
                    input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateModel()
            {
                // Given
                const string input = @"<div class=""entry"">
  <h1>{{title}}</h1>
  <div class=""body"">
    {{body}}
  </div>
</div>";
                const string output = @"<div class=""entry"">
  <h1>My New Post</h1>
  <div class=""body"">
    This is my first post!
  </div>
</div>";
                TestDocument document = new TestDocument(input);
                RenderScriban scriban = new RenderScriban().WithModel(new
                {
                    title = "My New Post",
                    body = "This is my first post!"
                });

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateModelAsDocument()
            {
                // Given
                const string input = @"<div class=""entry"">
  <h1>{{title}}</h1>
  <div class=""body"">
    {{body}}
  </div>
</div>";
                const string output = @"<div class=""entry"">
  <h1>My New Post</h1>
  <div class=""body"">
    This is my first post!
  </div>
</div>";
                TestDocument document = new TestDocument(input);
                RenderScriban scriban = new RenderScriban().WithModel(new TestDocument(new MetadataItems
                {
                    { Keys.Title, "My New Post" },
                    { "Body", "This is my first post!" }
                }));

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task AlternateComplexModel()
            {
                // Given
                const string input = @"
{{~ for post in posts ~}}
{{ post.title }}
{{ post.description }}
{{ post.custom.value }}
{{ end ~}}";
                const string output = @"
title1
description1
custom_value1
title2
description2
custom_value2
";

                TestDocument document = new TestDocument(input);
                RenderScriban scriban = new RenderScriban().WithModel(new
                {
                    Posts = new[]
                    {
                        new
                        {
                            Title = "title1",
                            Description = "description1",
                            Custom = new
                            {
                                Value = "custom_value1"
                            }
                        },
                        new
                        {
                            Title = "title2",
                            Description = "description2",
                            Custom = new
                            {
                                Value = "custom_value2"
                            }
                        }
                    }
                });

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ComplexModelMetadata()
            {
                // Given
                const string input = @"
{{~ for post in posts ~}}
{{ post.title }}
{{ post.description }}
{{ post.custom.value }}
{{ end ~}}";
                const string output = @"
title1
description1
custom_value1
title2
description2
custom_value2
";

                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "posts", new[]
                            {
                                new
                                {
                                    Title = "title1",
                                    Description = "description1",
                                    Custom = new
                                    {
                                        Value = "custom_value1"
                                    }
                                },
                                new
                                {
                                    Title = "title2",
                                    Description = "description2",
                                    Custom = new
                                    {
                                        Value = "custom_value2"
                                    }
                                }
                            }
                        }
                    },
                    input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ComplexModelMetadataWithChildDocuments()
            {
                // Given
                const string input = @"
{{~ for post in posts ~}}
{{ post.title }}
{{ post.description }}
{{ post.custom.value }}
{{ post.custom.typed.foo }}
{{ end ~}}";
                const string output = @"
title1
description1
custom_value1
Bar
title2
description2
custom_value2
Bar2
";

                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "posts", new[]
                            {
                                new TestDocument(new MetadataItems
                                {
                                    { "Title", "title1" },
                                    { "Description", "description1" },
                                    {
                                        "Custom", new TestDocument(new MetadataItems
                                        {
                                            { "Value", "custom_value1" },
                                            { "Typed", new { Foo = "Bar" } }
                                        })
                                    }
                                }),
                                new TestDocument(new MetadataItems
                                {
                                    { "Title", "title2" },
                                    { "Description", "description2" },
                                    {
                                        "Custom", new TestDocument(new MetadataItems
                                        {
                                            { "Value", "custom_value2" },
                                            { "Typed", new { Foo = "Bar2" } }
                                        })
                                    }
                                })
                            }
                        }
                    },
                    input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ComplexModelMetadataWithChildDocumentsAndCustomRenamer()
            {
                // Given
                const string input = @"
{{~ for post in POSTS ~}}
{{ post.TITLE }}
{{ post.DESCRIPTION }}
{{ post.CUSTOM.VALUE }}
{{ post.CUSTOM.TYPED.FOO }}
{{ end ~}}";
                const string output = @"
title1
description1
custom_value1
Bar
title2
description2
custom_value2
Bar2
";

                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "posts", new[]
                            {
                                new TestDocument(new MetadataItems
                                {
                                    { "Title", "title1" },
                                    { "Description", "description1" },
                                    {
                                        "Custom", new TestDocument(new MetadataItems
                                        {
                                            { "Value", "custom_value1" },
                                            { "Typed", new { Foo = "Bar" } }
                                        })
                                    }
                                }),
                                new TestDocument(new MetadataItems
                                {
                                    { "Title", "title2" },
                                    { "Description", "description2" },
                                    {
                                        "Custom", new TestDocument(new MetadataItems
                                        {
                                            { "Value", "custom_value2" },
                                            { "Typed", new { Foo = "Bar2" } }
                                        })
                                    }
                                })
                            }
                        }
                    },
                    input);
                RenderScriban scriban = new RenderScriban()
                    .WithMemberRenamer(x => x.Name.ToUpperInvariant());

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersScribanFromMetadata()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderScriban scriban = new RenderScriban("meta");

                // When
                IDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.GetString("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersScribanFromMetadataToNewKey()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderScriban scriban = new RenderScriban("meta", "meta2");

                // When
                IDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.GetString("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestDocument document = new TestDocument();
                RenderScriban scriban = new RenderScriban("meta");

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task ShouldRenderBuiltInProperties()
            {
                // Given
                const string input = @"id: {{ id }}
count: {{ count }}
keys:
{{~ for key in keys ~}}
  {{ key }}
{{~ end ~}}
values:
{{~ for value in values ~}}
  {{ value }}
{{~ end ~}}
{{ content_provider.media_type }}";
                const string output = @"id: {0}
count: 3
keys:
  source
  destination
  content_provider
values:
  /input/index.html
  index.html
  Statiq.Common.MemoryContent
text/html";

                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/index.html"),
                    new NormalizedPath("index.html"),
                    input,
                    "text/html");
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(string.Format(output, document.Id), StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldSetLocalVariable()
            {
                // Given
                const string input = @"{{ x = 5 ~}}
{{ x }}
{{ x + 1 }}";
                const string output = "5\n6";
                TestDocument document = new TestDocument(input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task IncludesTemplateFromFileSystem()
            {
                // Given
                const string include = @"{{ y = y + 1 ~}}
This is a string with the value {{ y }}";
                const string input = @"{{ y = 0 ~}}
{{include '../include/myinclude.html' }}
{{
x = include '../include/myinclude.html'
x + "" modified""
}}";
                const string output = @"This is a string with the value 1
This is a string with the value 2 modified";

                TestFileProvider fileProvider = new TestFileProvider();
                fileProvider.AddDirectory("/");
                fileProvider.AddDirectory("/input");
                fileProvider.AddDirectory("/input/include");
                fileProvider.AddDirectory("/input/docs");
                fileProvider.AddFile("/input/include/myinclude.html", include);
                fileProvider.AddFile("/input/docs/test.html", input);
                TestFileSystem fileSystem = new TestFileSystem
                {
                    FileProvider = fileProvider
                };
                TestExecutionContext context = new TestExecutionContext
                {
                    FileSystem = fileSystem
                };
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/docs/test.html"),
                    new NormalizedPath("docs/test.html"),
                    input);
                RenderScriban scriban = new RenderScriban();

                // When
                TestDocument result = await ExecuteAsync(document, context, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CustomLexerOptions()
            {
                // Given
                const string input = "title";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems { { "title", "Hello World!" } }, input);
                RenderScriban scriban = new RenderScriban()
                    .WithLexerOptions(new LexerOptions
                    {
                        Mode = ScriptMode.ScriptOnly,
                    });

                // When
                TestDocument result = await ExecuteAsync(document, scriban).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CustomParserOptions()
            {
                // Given
                const string input = @"
{{ for x in 1..10 }}
    {{ for x in 1..10 }}
        {{ for x in 1..10 }}
            We should not get here.
        {{ end }}
    {{ end }}
{{ end }}";
                TestDocument document = new TestDocument(input);
                RenderScriban scriban = new RenderScriban()
                    .WithParserOptions(new ParserOptions
                    {
                        ExpressionDepthLimit = 2
                    });

                // When, Then
                (await Should.ThrowAsync<Exception>(async () =>
                        await ExecuteAsync(document, scriban))).
                    Message.ShouldContain(
                        "<input>(2,13) : error : The statement depth limit `2` was reached when parsing this statement");
            }
        }
    }
}