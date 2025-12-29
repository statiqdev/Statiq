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
    public class IMetadataGetExtensionsFixture : BaseFixture
    {
        public class TryGetValueTests : IMetadataGetExtensionsFixture
        {
            [Test]
            public void ReturnsFalseForNullMetadata()
            {
                // Given, When
                bool result = ((IMetadata)null).TryGetValue<object>("Foo", out object value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForNullKey()
            {
                // Given
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue<object>(null, out object value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForMissingKey()
            {
                // Given
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue<object>("Foo", out object value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void ReturnsObjectValue()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", 2 }
                };

                // When
                bool result = metadata.TryGetValue<object>("Foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBeOfType<int>().ShouldBe(2);
            }

            [Test]
            public void ReturnsTypedValue()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", 2 }
                };

                // When
                bool result = metadata.TryGetValue("Foo", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(2);
            }

            [Test]
            public void ReturnsConvertedValue()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", "2" }
                };

                // When
                bool result = metadata.TryGetValue("Foo", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(2);
            }

            [Test]
            public void ReturnsFalseIfNoConversion()
            {
                // Given
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", "abc" }
                };

                // When
                bool result = metadata.TryGetValue("Foo", out TryGetValueTests value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }
            [Test]
            public void EvaluatesSimpleScript()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue("=> 1 + 2", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(3);
            }

            [Test]
            public void EvaluatesSimpleScriptWhenConversionFails()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestMetadata metadata = new TestMetadata();

                // When
                bool result = metadata.TryGetValue("=> 1 + 2", out TryGetValueTests value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBeNull();
            }

            [Test]
            public void EvaluatesScriptWithMetadataAccess()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestMetadata metadata = new TestMetadata()
                {
                    { "Foo", 3 }
                };

                // When
                bool result = metadata.TryGetValue("=> 1 + GetInt(\"Foo\")", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(4);
            }
        }

        public class WithoutSettingsTests : IMetadataGetExtensionsFixture
        {
            [Test]
            public void SettingsNotIncluded()
            {
                // Given
                Common.Settings settings = new Common.Settings
                {
                    { "A", "a" }
                };
                IDocument document = new Document(settings, null, null, null, null);
                IDocument cloned = document.Clone(new MetadataItems { { "A", "b" } });

                // When
                string initialValue = document.WithoutSettings().GetString("A");
                string clonedValue = cloned.WithoutSettings().GetString("A");

                // Then
                initialValue.ShouldBeNull();
                clonedValue.ShouldBe("b");
            }
        }
    }
}
