using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    [TestFixture]
    public class SpanExtensionsFixture : BaseFixture
    {
        public class RemoveTests : SpanExtensionsFixture
        {
            [TestCase(0, 1, "bcde")]
            [TestCase(1, 3, "ae")]
            [TestCase(0, 5, "")]
            [TestCase(3, 2, "abc")]
            public void RemovesFromSpan(int startIndex, int length, string expected)
            {
                // Given
                const string str = "abcde";
                Span<char> chars = new char[str.Length];
                str.AsSpan().CopyTo(chars);

                // When
                Span<char> removed = chars.Remove(startIndex, length);

                // Then
                removed.ToString().ShouldBe(expected);
            }
        }
    }
}
