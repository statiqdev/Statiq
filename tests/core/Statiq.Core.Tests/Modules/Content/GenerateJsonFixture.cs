using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class GenerateJsonFixture : BaseFixture
    {
        private class Account
        {
            public string Email { get; set; }
            public bool Active { get; set; }
            public DateTime CreatedDate { get; set; }
            public IList<string> Roles { get; set; }
        }

        private static Account _jsonObject = new Account
        {
            Email = "james@example.com",
            Active = true,
            CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
            Roles = new List<string>
            {
                "User",
                "Admin"
            }
        };

        private static string _jsonContent = @"{
  ""Email"": ""james@example.com"",
  ""Active"": true,
  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
  ""Roles"": [
    ""User"",
    ""Admin""
  ]
}";

        private static string _camelCaseJsonContent = @"{
  ""email"": ""james@example.com"",
  ""active"": true,
  ""createdDate"": ""2013-01-20T00:00:00Z"",
  ""roles"": [
    ""User"",
    ""Admin""
  ]
}";

        public class ExecuteTests : GenerateJsonFixture
        {
            [Test]
            public async Task GetsObjectFromMetadata()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject");

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task GetsObjectFromContextDelegate()
            {
                // Given
                TestDocument document = new TestDocument();
                GenerateJson generateJson = new GenerateJson(_jsonObject);

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task GetsObjectFromDocumentDelegate()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson(Config.FromDocument(doc => doc.Get("JsonObject")));

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SetsMetadataKey()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject", "OutputKey");

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result["OutputKey"].ToString().ShouldBe(_jsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotIndent()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject").WithIndenting(false);
                string nonIndentedJsonContent = _jsonContent
                    .Replace(" ", string.Empty)
                    .Replace("\r\n", string.Empty)
                    .Replace("\n", string.Empty);

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(nonIndentedJsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task GeneratesCamelCasePropertyNames()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "JsonObject", _jsonObject }
                });
                GenerateJson generateJson = new GenerateJson("JsonObject").WithCamelCase();

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(_camelCaseJsonContent, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataKeys()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(new[] { "Bar" });

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"{
  ""Bar"": ""baz""
}",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataKeysWithCamelCase()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(new[] { "Bar" }).WithCamelCase();

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"{
  ""bar"": ""baz""
}",
                    StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataObject()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(Config.FromDocument(doc => (object)doc.FilterMetadata("Bar")));

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

            // Then
                result.Content.ShouldBe(
                @"{
  ""Bar"": ""baz""
}",
                StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task SerializesMetadataObjectWithCamelCase()
            {
                // Given
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "Foo", "fuz" },
                    { "Bar", "baz" }
                });
                GenerateJson generateJson = new GenerateJson(Config.FromDocument(doc => (object)doc.FilterMetadata("Bar"))).WithCamelCase();

                // When
                TestDocument result = await ExecuteAsync(document, generateJson).SingleAsync();

                // Then
                result.Content.ShouldBe(
                    @"{
  ""bar"": ""baz""
}",
                    StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}
