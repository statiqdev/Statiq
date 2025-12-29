using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class ParseJsonFixture : BaseFixture
    {
        private static string _jsonContent = @"{
  ""Email"": ""james@example.com"",
  ""Active"": true,
  ""CreatedDate"": ""2013-01-20T00:00:00Z"",
  ""Roles"": [
    ""User"",
    ""Admin"",
    11
  ],
  ""Description"": {
    ""Height"": 5
  }
}";

        public class ExecuteTests : ParseJsonFixture
        {
            [Test]
            public async Task PopulatesDocument()
            {
                // Given
                TestDocument document = new TestDocument(_jsonContent);
                ParseJson json = new ParseJson();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(document, json);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                result.Count.ShouldBe(8); // Includes property metadata
                result["Email"].ShouldBe("james@example.com");
                result["Active"].ShouldBe(true);
                result["CreatedDate"].ShouldBe("2013-01-20T00:00:00Z");
                result["Roles"].ShouldBe(new object[] { "User", "Admin", 11 });
                IMetadata nestedObject = result.GetMetadata("Description");
                nestedObject.ShouldNotBeNull();
                nestedObject["Height"].ShouldBe(5);
            }

            [Test]
            public async Task PopulatesKey()
            {
                // Given
                TestDocument document = new TestDocument(_jsonContent);
                ParseJson json = new ParseJson("Foo");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(document, json);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                result.Count.ShouldBe(4); // Includes property metadata
                IMetadata metadata = result.GetMetadata("Foo");
                metadata.ShouldNotBeNull();
                metadata["Email"].ShouldBe("james@example.com");
                metadata["Active"].ShouldBe(true);
                metadata["CreatedDate"].ShouldBe("2013-01-20T00:00:00Z");
                metadata["Roles"].ShouldBe(new object[] { "User", "Admin", 11 });
                IMetadata nestedObject = metadata.GetMetadata("Description");
                nestedObject.ShouldNotBeNull();
                nestedObject["Height"].ShouldBe(5);
            }

            [Test]
            public async Task PopulatesDocumentAndKey()
            {
                // Given
                TestDocument document = new TestDocument(_jsonContent);
                ParseJson json = new ParseJson("Foo", true);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(document, json);

                // Then
                TestDocument result = results.ShouldHaveSingleItem();
                result.Count.ShouldBe(9); // Includes property metadata
                result["Email"].ShouldBe("james@example.com");
                result["Active"].ShouldBe(true);
                result["CreatedDate"].ShouldBe("2013-01-20T00:00:00Z");
                result["Roles"].ShouldBe(new object[] { "User", "Admin", 11 });
                IMetadata nestedObject = result.GetMetadata("Description");
                nestedObject.ShouldNotBeNull();
                nestedObject["Height"].ShouldBe(5);
                IMetadata metadata = result.GetMetadata("Foo");
                metadata.ShouldNotBeNull();
                metadata["Email"].ShouldBe("james@example.com");
                metadata["Active"].ShouldBe(true);
                metadata["CreatedDate"].ShouldBe("2013-01-20T00:00:00Z");
                metadata["Roles"].ShouldBe(new object[] { "User", "Admin", 11 });
                nestedObject = metadata.GetMetadata("Description");
                nestedObject.ShouldNotBeNull();
                nestedObject["Height"].ShouldBe(5);
            }
        }
    }
}
