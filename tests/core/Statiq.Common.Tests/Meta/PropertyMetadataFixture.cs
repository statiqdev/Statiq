using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    public class PropertyMetadataFixture : BaseFixture
    {
        public class TryGetValueTests : PropertyMetadataFixture
        {
            [Test]
            public void GetsPropertyAsMetadata()
            {
                // Given
                PropertyTest propertyTest = new PropertyTest();
                IMetadata metadata = PropertyMetadata<PropertyTest>.For(propertyTest);

                // When
                bool result = metadata.TryGetValue("Foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(3);
            }

            [Test]
            public void ReturnsFalseForMissingProperty()
            {
                // Given
                PropertyTest propertyTest = new PropertyTest();
                IMetadata metadata = PropertyMetadata<PropertyTest>.For(propertyTest);

                // When
                bool result = metadata.TryGetValue("Bar", out object value);

                // Then
                result.ShouldBeFalse();
                value.ShouldBe(default);
            }

            [Test]
            public void ConvertsValue()
            {
                // Given
                PropertyTest propertyTest = new PropertyTest();
                IMetadata metadata = PropertyMetadata<PropertyTest>.For(propertyTest);

                // When
                bool result = metadata.TryGetValue("Fuzz", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(4);
            }

            [Test]
            public void ConvertsMetadataValue()
            {
                // Given
                PropertyTest propertyTest = new PropertyTest();
                IMetadata metadata = PropertyMetadata<PropertyTest>.For(propertyTest);

                // When
                bool result = metadata.TryGetValue("Fizz", out int value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(5);
            }
        }

        public class PropertyTest
        {
            public int Foo { get; } = 3;
            public string Fuzz { get; } = "4";
            public object Fizz { get; } = new SimpleMetadataValue { Value = "5" };
        }

        private class SimpleMetadataValue : IMetadataValue
        {
            public object Value { get; set; }
            object IMetadataValue.Get(string key, IMetadata metadata) => Value;
        }
    }
}
