using System;
using NUnit.Framework;
using Statiq.CodeAnalysis.Analysis;
using Statiq.Testing;
using Shouldly;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class WrappingStringBuilderFixture : BaseFixture
    {
        public class IntegrationTests : WrappingStringBuilderFixture
        {
            [Test]
            public void DoesNotWrapIfNoBreakpoints()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc", false);
                builder.Append("def", false);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abcdefghi");
            }

            [Test]
            public void DefaultBehaviorIsNotToWrap()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc");
                builder.Append("def");
                builder.Append("ghi");
                string result = builder.ToString();

                // Then
                result.ShouldBe("abcdefghi");
            }

            [Test]
            public void WrapsIfBreakpoint()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc", false);
                builder.Append("def", false);
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abcdef" + Environment.NewLine + "ghi");
            }

            [Test]
            public void WrapsEarlierIfEarlierBreakpoint()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7);

                // When
                builder.Append("abc", false);
                builder.Append("def", true);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abc" + Environment.NewLine + "defghi");
            }

            [Test]
            public void NewLinesIncludeNewLinePrefix()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(7, "1234");

                // When
                builder.Append("abc", false);
                builder.Append("def", true);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abc" + Environment.NewLine + "1234defghi");
            }

            [Test]
            public void BreakpointCalculationIncludesNewLinePrefix()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(8, "1234");

                // When
                builder.Append("abc", false);
                builder.Append("defxyz", true);
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abc" + Environment.NewLine + "1234defxyz" + Environment.NewLine + "1234ghi");
            }

            [Test]
            public void MultipleWraps()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(2, "1234");

                // When
                builder.Append("abc", true);
                builder.Append("def", true);
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abc" + Environment.NewLine + "1234def" + Environment.NewLine + "1234ghi");
            }

            [Test]
            public void AppendLineBreaksAtEndOfValue()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(8, "1234");

                // When
                builder.Append("abc", false);
                builder.AppendLine("def", false);
                builder.Append("ghi", false);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abcdef" + Environment.NewLine + "1234ghi");
            }

            [Test]
            public void AppendLineWrapsBeforeValue()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5, "1234");

                // When
                builder.Append("abc", false);
                builder.Append("def", false);
                builder.AppendLine("ghi", true);
                builder.Append("xyz", false);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abcdef" + Environment.NewLine + "1234ghi" + Environment.NewLine + "1234xyz");
            }

            [Test]
            public void MultipleWrapsWithDifferentNewLinePrefixes()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5);

                // When
                builder.Append("abc", true);
                builder.Append("def", true);
                builder.NewLinePrefix = "1234";
                builder.Append("ghi", true);
                builder.NewLinePrefix = "5";
                builder.Append("jkl", true);
                builder.Append("m", true);
                builder.Append("n", true);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abc" + Environment.NewLine + "def" + Environment.NewLine + "1234ghi" + Environment.NewLine + "5jklm" + Environment.NewLine + "5n");
            }

            [Test]
            public void NoLeadingBreakWhenFirstSegmentIsBreakable()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5);

                // When
                builder.Append("abc", true);
                builder.Append("def", false);
                builder.Append("g", true);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abcdef" + Environment.NewLine + "g");
            }

            [Test]
            public void DifferentNewLinePrefixesAfterAppendLine()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(5);

                // When
                builder.AppendLine("abc", true);
                builder.Append("def", true);
                builder.NewLinePrefix = "1234";
                builder.Append("g", true);
                builder.Append("hij", true);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abc" + Environment.NewLine + "defg" + Environment.NewLine + "1234hij");
            }

            [Test]
            public void EmptyAppendLine()
            {
                // Given
                WrappingStringBuilder builder = new WrappingStringBuilder(8, "1234");

                // When
                builder.Append("abc", true);
                builder.AppendLine();
                builder.Append("def", true);
                builder.AppendLine();
                builder.AppendLine();
                builder.Append("ghi", true);
                string result = builder.ToString();

                // Then
                result.ShouldBe("abc" + Environment.NewLine + "1234def" + Environment.NewLine + "1234" + Environment.NewLine + "1234ghi");
            }
        }
    }
}