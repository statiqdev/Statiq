using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HandlebarsDotNet;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Handlebars.Tests
{
    [TestFixture]
    public class RenderHandlebarsFixture : BaseFixture
    {
        public class ExecuteTests : RenderHandlebarsFixture
        {
            [Test]
            public async Task RendersHandlebars()
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
                RenderHandlebars handlebars = new RenderHandlebars();

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

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
                RenderHandlebars handlebars = new RenderHandlebars().WithModel(new
                {
                    title = "My New Post",
                    body = "This is my first post!"
                });

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsFromMetadata()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderHandlebars handlebars = new RenderHandlebars("meta");

                // When
                IDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.GetString("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsFromMetadataToNewKey()
            {
                // Given
                const string input = "{{title}}";
                const string output = "Hello World!";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input },
                    { "title", "Hello World!" }
                });
                RenderHandlebars handlebars = new RenderHandlebars("meta", "meta2");

                // When
                IDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.GetString("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestDocument document = new TestDocument();
                RenderHandlebars handlebars = new RenderHandlebars("meta");

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task RendersHandlebarsWithPartial()
            {
                // Given
                const string input = @"{{#names}}
  {{> user}}
{{/names}}";
                const string output = @"<strong>Karen</strong><strong>Jon</strong>";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "names", new[]
                            {
                                new
                                {
                                    name = "Karen"
                                },
                                new
                                {
                                    name = "Jon"
                                }
                            }
                        }
                    },
                    input);

                RenderHandlebars handlebars = new RenderHandlebars()
                    .WithPartial("user", Config.FromValue("<strong>{{name}}</strong>"));

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsWithHelper()
            {
                // Given
                const string input = @"Click here: {{link_to}}";
                const string output = @"Click here: <a href='https://github.com/rexm/handlebars.net'>Handlebars.Net</a>";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "url", "https://github.com/rexm/handlebars.net" },
                        { "text", "Handlebars.Net" }
                    },
                    input);

                RenderHandlebars handlebars = new RenderHandlebars()
                    .WithHelper(
                        "link_to",
                        Config.FromValue<HandlebarsHelper>((writer, context, _) =>
                        {
                            dynamic model = context.Value;
                            HandlebarsExtensions.WriteSafeString(writer, $"<a href='{model.url}'>{model.text}</a>");
                        }));

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsWithBlockHelper()
            {
                // Given
                const string input = @"{{#each animals}}
The animal, {{name}}, {{StringEqualityBlockHelper type 'dog'}}is a dog{{else}}is not a dog{{/StringEqualityBlockHelper}}.
{{/each}}";
                const string output = @"The animal, Fluffy, is not a dog.
The animal, Fido, is a dog.
The animal, Chewy, is not a dog.
";

                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "animals", new[]
                            {
                                new { name = "Fluffy", type = "cat" },
                                new { name = "Fido", type = "dog" },
                                new { name = "Chewy", type = "hamster" }
                            }
                        }
                    }, input);

                RenderHandlebars handlebars = new RenderHandlebars()
                    .WithBlockHelper(
                        "StringEqualityBlockHelper",
                        Config.FromValue<HandlebarsBlockHelper>((writer, options, _, arguments) =>
                        {
                            if (arguments.Length != 2)
                            {
                                throw new HandlebarsException("{{StringEqualityBlockHelper}} helper must have exactly two argument");
                            }
                            string left = arguments[0] as string;
                            string right = arguments[1] as string;
                            if (left != right)
                            {
                                options.Template(writer, null);
                            }
                            else
                            {
                                options.Inverse(writer, null);
                            }
                        }));

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsWithPartialUsingConfigure()
            {
                // Given
                const string input = @"{{#names}}
  {{> user}}
{{/names}}";
                const string output = @"<strong>Karen</strong><strong>Jon</strong>";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "names", new[]
                            {
                                new
                                {
                                    name = "Karen"
                                },
                                new
                                {
                                    name = "Jon"
                                }
                            }
                        }
                    },
                    input);

                RenderHandlebars handlebars = new RenderHandlebars()
                    .Configure((_, __, x) => x.RegisterTemplate("user", "<strong>{{name}}</strong>"));

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsWithHelperUsingConfigure()
            {
                // Given
                const string input = @"Click here: {{link_to}}";
                const string output = @"Click here: <a href='https://github.com/rexm/handlebars.net'>Handlebars.Net</a>";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        { "url", "https://github.com/rexm/handlebars.net" },
                        { "text", "Handlebars.Net" }
                    },
                    input);

                RenderHandlebars handlebars = new RenderHandlebars()
                    .Configure((_, __, x) => x.RegisterHelper("link_to",
                        (writer, context, _) =>
                        {
                            dynamic model = context.Value;
                            HandlebarsExtensions.WriteSafeString(writer, $"<a href='{model.url}'>{model.text}</a>");
                        }));

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHandlebarsWithBlockHelperUsingConfigure()
            {
                // Given
                const string input = @"{{#each animals}}
The animal, {{name}}, {{StringEqualityBlockHelper type 'dog'}}is a dog{{else}}is not a dog{{/StringEqualityBlockHelper}}.
{{/each}}";
                const string output = @"The animal, Fluffy, is not a dog.
The animal, Fido, is a dog.
The animal, Chewy, is not a dog.
";

                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            "animals", new[]
                            {
                                new { name = "Fluffy", type = "cat" },
                                new { name = "Fido", type = "dog" },
                                new { name = "Chewy", type = "hamster" }
                            }
                        }
                    }, input);

                RenderHandlebars handlebars = new RenderHandlebars()
                    .Configure((_, __, x) => x.RegisterHelper("StringEqualityBlockHelper", (writer, options, __, arguments) =>
                        {
                            if (arguments.Length != 2)
                            {
                                throw new HandlebarsException("{{StringEqualityBlockHelper}} helper must have exactly two argument");
                            }
                            string left = arguments[0] as string;
                            string right = arguments[1] as string;
                            if (left != right)
                            {
                                options.Template(writer, null);
                            }
                            else
                            {
                                options.Inverse(writer, null);
                            }
                        }));

                // When
                TestDocument result = await ExecuteAsync(document, handlebars).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}