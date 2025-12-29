using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    public class TypeHelperFixture : BaseFixture
    {
        public class TryConvertTests : TypeHelperFixture
        {
            [Test]
            public void ArrayConvertsToArray()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out Array result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)value);
            }

            [Test]
            public void ConvertsEnumerableToIReadOnlyList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IReadOnlyList<int> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)value);
            }

            [Test]
            public void ConvertsEnumerableToIList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IList<int> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)value);
            }

            [Test]
            public void ConvertsEnumerableToList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out List<int> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)value);
            }

            [Test]
            public void ConvertsEnumerableToArray()
            {
                // Given
                List<int> value = new List<int> { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out int[] result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)value);
            }

            [Test]
            public void ConvertsEnumerableToIEnumerable()
            {
                // Given
                Array value = new[] { 1.0, 2.0 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IEnumerable<int> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)value);
            }

            [Test]
            public void ConvertsSingleArrayToItemOfIReadOnlyList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IReadOnlyList<Array> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)new[] { value });
            }

            [Test]
            public void ConvertsSingleArrayToItemOfIList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IList<Array> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)new[] { value });
            }

            [Test]
            public void ConvertsSingleArrayToItemOfList()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out List<Array> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)new[] { value });
            }

            [Test]
            public void ConvertsSingleEnumerableToItemOfArray()
            {
                // Given
                List<int> value = new List<int> { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out List<int>[] result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)new[] { value });
            }

            [Test]
            public void ConvertsSingleEnumerableToItemOfIEnumerable()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IEnumerable<Array> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)new[] { value });
            }

            [Test]
            public void ConvertsArrayOfEnumerablesToItemsInList()
            {
                // Given
                Array value = new[]
                {
                    new List<int> { 1, 2, 3 },
                    new List<int> { 4, 5, 6 }
                };

                // When
                bool convert = TypeHelper.TryConvert(value, out IList<IEnumerable<int>> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)value);
            }

            [Test]
            public void ConvertsArrayOfEnumerablesToItemInListOfArrays()
            {
                // Given
                Array value = new[]
                {
                    new List<int> { 1, 2, 3 },
                    new List<int> { 4, 5, 6 }
                };

                // When
                bool convert = TypeHelper.TryConvert(value, out IList<IList<IEnumerable<int>>> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe((IEnumerable)new[] { value });
                result[0][0].ShouldBe((IEnumerable)value.GetValue(0));
                result[0][1].ShouldBe((IEnumerable)value.GetValue(1));
            }

            [Test]
            public void ConvertsArrayOfStringsToFirstString()
            {
                // Given
                Array value = new[] { "Red", "Green", "Blue" };

                // When
                bool convert = TypeHelper.TryConvert(value, out string result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe("Red");
            }

            [Test]
            public void ConvertsArrayOfIntsToFirstInt()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out int result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe(1);
            }

            [Test]
            public void ConvertsArrayOfIntsToFirstString()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out string result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe("1");
            }

            [Test]
            public void ConvertsArrayOfStringsToFirstInt()
            {
                // Given
                Array value = new[] { "1", "2", "3" };

                // When
                bool convert = TypeHelper.TryConvert(value, out int result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe(1);
            }

            [Test]
            public void ConvertsArrayOfObjectsToFirstInt()
            {
                // Given
                Array value = new object[] { "1", 2, 3.0 };

                // When
                bool convert = TypeHelper.TryConvert(value, out int result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe(1);
            }

            [Test]
            public void ConvertsArrayOfObjectsToFirstIntWhenFirstItemNotConvertible()
            {
                // Given
                Array value = new object[] { "a", 2, 3.0 };

                // When
                bool convert = TypeHelper.TryConvert(value, out int result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe(2);
            }

            [Test]
            public void ArrayOfIntConvertsToEnumerableOfString()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IEnumerable<string> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe(new[] { "1", "2", "3" });
            }

            [Test]
            public void ArrayOfIntConvertsToEnumerableOfObject()
            {
                // Given
                Array value = new[] { 1, 2, 3 };

                // When
                bool convert = TypeHelper.TryConvert(value, out IEnumerable<object> result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBe(value.Cast<object>());
            }

            [Test]
            public void StringConvertsToUri()
            {
                // Given
                const string value = "http://google.com/";

                // When
                bool convert = TypeHelper.TryConvert(value, out Uri uri);

                // Then
                convert.ShouldBeTrue();
                uri.ToString().ShouldBe(value);
            }

            [Test]
            public void StringConvertsToDateTime()
            {
                // Given
                const string value = "2016-10-17 08:00";

                // When
                bool convert = TypeHelper.TryConvert(value, out DateTime dateTime);

                // Then
                convert.ShouldBeTrue();
                dateTime.ShouldBe(new DateTime(2016, 10, 17, 8, 0, 0));
            }

            [Test]
            public void Iso8601StringConvertsToDateTimeOffset()
            {
                // Given
                const string value = "2013-02-03T04:05:06.0070000+04:00";

                // When
                bool convert = TypeHelper.TryConvert(value, out DateTimeOffset dateTime);

                // Then
                convert.ShouldBeTrue();
                dateTime.ShouldBe(new DateTimeOffset(2013, 2, 3, 4, 5, 6, 7, TimeSpan.FromHours(4)));
            }

            [Test]
            public void SimpleIso8601StringConvertsToDateTimeOffset()
            {
                // Given
                const string value = "2013-02-03T04:05:06Z";

                // When
                bool convert = TypeHelper.TryConvert(value, out DateTimeOffset dateTime);

                // Then
                convert.ShouldBeTrue();
                dateTime.ShouldBe(new DateTimeOffset(2013, 2, 3, 4, 5, 6, TimeSpan.Zero));
            }

            [Test]
            public void NullConvertsToNullable()
            {
                // Given
                object value = null;

                // When
                bool convert = TypeHelper.TryConvert(value, out DateTime? result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBeNull();
            }

            [Test]
            public void NullDoesNotConvertToValueType()
            {
                // Given
                object value = null;

                // When
                bool convert = TypeHelper.TryConvert(value, out int result);

                // Then
                convert.ShouldBeFalse();
            }

            [Test]
            public void NullConvertsToReferenceType()
            {
                // Given
                object value = null;

                // When
                bool convert = TypeHelper.TryConvert(value, out object result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldBeNull();
            }

            [Test]
            public void DateTimeConvertstoNullable()
            {
                // Given
                DateTime value = new DateTime(2015, 1, 1);

                // When
                bool convert = TypeHelper.TryConvert(value, out DateTime? result);

                // Then
                convert.ShouldBeTrue();
                result.HasValue.ShouldBeTrue();
                result.Value.ShouldBe(value);
            }

            [Test]
            public void IMetadataConvertsToIDocument()
            {
                // Given
                IMetadata value = new MetadataDictionary
                {
                    { "Foo", "bar" },
                    { "A", 1 }
                };

                // When
                bool convert = TypeHelper.TryConvert(value, out IDocument result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldNotBeNull();
                result["Foo"].ShouldBe("bar");
                result["A"].ShouldBe(1);
            }

            [Test]
            public void IDocumentConvertsToIMetadata()
            {
                // Given
                IDocument value = new TestDocument
                {
                    { "Foo", "bar" },
                    { "A", 1 }
                };

                // When
                bool convert = TypeHelper.TryConvert(value, out IMetadata result);

                // Then
                convert.ShouldBeTrue();
                result.ShouldNotBeNull();
                result["Foo"].ShouldBe("bar");
                result["A"].ShouldBe(1);
            }

            [Test]
            public void ConvertsToType()
            {
                // Given
                const string value = "2016-10-17 08:00";

                // When
                bool convert = TypeHelper.TryConvert(value, typeof(DateTime), out object dateTime);

                // Then
                convert.ShouldBeTrue();
                dateTime.ShouldBeOfType<DateTime>().ShouldBe(new DateTime(2016, 10, 17, 8, 0, 0));
            }

            public enum ConvertEnum
            {
                Apple = 3,
                Orange = 5
            }

            [Test]
            public void ConvertsStringToEnum()
            {
                // Given
                const string value = "Orange";

                // When
                bool convert = TypeHelper.TryConvert<ConvertEnum>(value, out ConvertEnum converted);

                // Then
                convert.ShouldBeTrue();
                converted.ShouldBe(ConvertEnum.Orange);
            }

            [Test]
            public void ConvertsIntToEnum()
            {
                // Given
                const int value = 5;

                // When
                bool convert = TypeHelper.TryConvert<ConvertEnum>(value, out ConvertEnum converted);

                // Then
                convert.ShouldBeTrue();
                converted.ShouldBe(ConvertEnum.Orange);
            }

            [Test]
            public void DoesNotConvertUnmatchedStringToEnum()
            {
                // Given
                const string value = "Banana";

                // When
                bool convert = TypeHelper.TryConvert<ConvertEnum>(value, out ConvertEnum converted);

                // Then
                convert.ShouldBeFalse();
                converted.ShouldBe(default(ConvertEnum));
            }

            // This behavior is a little strange, so documented as a test
            // Any int can be converted to an enum, even if a value doesn't exist for it
            [Test]
            public void ConvertsUnmatchedIntToEnum()
            {
                // Given
                const int value = 4;

                // When
                bool convert = TypeHelper.TryConvert<ConvertEnum>(value, out ConvertEnum converted);

                // Then
                convert.ShouldBeTrue();
                ((int)converted).ShouldBe(4);
            }

            [Test]
            public void ConvertsEnumToString()
            {
                // Given
                const ConvertEnum value = ConvertEnum.Orange;

                // When
                bool convert = TypeHelper.TryConvert<string>(value, out string converted);

                // Then
                convert.ShouldBeTrue();
                converted.ShouldBe("Orange");
            }

            [Test]
            public void ConvertsEnumToInt()
            {
                // Given
                const ConvertEnum value = ConvertEnum.Orange;

                // When
                bool convert = TypeHelper.TryConvert<int>(value, out int converted);

                // Then
                convert.ShouldBeTrue();
                converted.ShouldBe(5);
            }
        }
    }
}
