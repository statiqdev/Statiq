using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    public class IMetadataJsonExtensionsFixture : BaseFixture
    {
        public class ToJsonTests : IMetadataJsonExtensionsFixture
        {
            [Test]
            public void SerializesMetadata()
            {
                // Given
                TestMetadata metadata = new TestMetadata
                {
                    { "A", "a" },
                    { "B", "b" }
                };

                // When
                string json = metadata.ToJson();

                // Then
                json.ShouldBe(@"{""A"":""a"",""B"":""b""}");
            }

            [Test]
            public void SerializesNullMetadata()
            {
                // Given
                TestMetadata metadata = null;

                // When
                string json = metadata.ToJson();

                // Then
                json.ShouldBe("null");
            }

            [Test]
            public void SerializesMetadataEnumerable()
            {
                // Given
                TestMetadata[] metadata = new TestMetadata[]
                {
                    new TestMetadata
                    {
                        { "A", "a" },
                        { "B", "b" }
                    },
                    new TestMetadata
                    {
                        { "X", "x" },
                        { "Y", "y" }
                    }
                };

                // When
                string json = metadata.ToJson();

                // Then
                json.ShouldBe(@"[{""A"":""a"",""B"":""b""},{""X"":""x"",""Y"":""y""}]");
            }

            [Test]
            public void SerializesNullMetadataEnumerable()
            {
                // Given
                TestMetadata[] metadata = null;

                // When
                string json = metadata.ToJson();

                // Then
                json.ShouldBe("null");
            }

            [Test]
            public void SerializesMetadataEnumerableWithNull()
            {
                // Given
                TestMetadata[] metadata = new TestMetadata[]
                {
                    new TestMetadata
                    {
                        { "A", "a" },
                        { "B", "b" }
                    },
                    null
                };

                // When
                string json = metadata.ToJson();

                // Then
                json.ShouldBe(@"[{""A"":""a"",""B"":""b""},null]");
            }
        }
    }
}
