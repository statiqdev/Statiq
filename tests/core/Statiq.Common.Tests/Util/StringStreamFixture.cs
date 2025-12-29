using System;
using System.IO;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    [TestFixture]
    public class StringStreamFixture : BaseFixture
    {
        public class ReadTests : StringStreamFixture
        {
            [TestCase(1)]
            [TestCase(5)]
            [TestCase(10)]
            [TestCase(50)]
            public void EncodesStringWithoutPreamble(int bufferCharCount)
            {
                // Given
                string source = "0123456789";
                Encoding encoding = new UTF8Encoding(false);
                StringStream stringStream = new StringStream(source, encoding, bufferCharCount);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringStream.CopyTo(memoryStream);

                // Then
                memoryStream.ToArray().ShouldBe(encoding.GetBytes(source));
            }

            [TestCase(1)]
            [TestCase(5)]
            [TestCase(10)]
            [TestCase(50)]
            public void EncodesStringWithPreamble(int bufferCharCount)
            {
                // Given
                string source = "0123456789";
                Encoding encoding = new UTF8Encoding(true);
                StringStream stringStream = new StringStream(source, encoding, bufferCharCount);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringStream.CopyTo(memoryStream);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                memoryStream.ToArray().ShouldBe(encoding.GetPreamble().Concat(encoding.GetBytes(source)).ToArray());
            }

            [Test]
            public void NullSourceStringWithoutPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(false);
                StringStream stringStream = new StringStream((string)null, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringStream.CopyTo(memoryStream);

                // Then
                memoryStream.ToArray().ShouldBeEmpty();
            }

            [Test]
            public void EmptySourceStringWithoutPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(false);
                StringStream stringStream = new StringStream(string.Empty, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringStream.CopyTo(memoryStream);

                // Then
                memoryStream.ToArray().ShouldBeEmpty();
            }

            [Test]
            public void NullSourceStringWithPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(true);
                StringStream stringStream = new StringStream((string)null, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringStream.CopyTo(memoryStream);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                memoryStream.ToArray().ShouldBe(encoding.GetPreamble());
            }

            [Test]
            public void EmptySourceStringWithPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(true);
                StringStream stringStream = new StringStream(string.Empty, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringStream.CopyTo(memoryStream);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                memoryStream.ToArray().ShouldBe(encoding.GetPreamble());
            }

            [TestCase(1)]
            [TestCase(5)]
            [TestCase(10)]
            [TestCase(50)]
            public void PartialReadAndResetWithoutPreamble(int bufferCharCount)
            {
                // Given
                string source = "0123456789";
                Encoding encoding = new UTF8Encoding(false);
                StringStream stringStream = new StringStream(source, encoding, bufferCharCount);
                byte[] buffer = new byte[2];

                // When
                stringStream.ReadExactly(buffer, 0, 2);
                stringStream.Reset();
                stringStream.ReadExactly(buffer, 0, 2);

                // Then
                buffer.ShouldBe(encoding.GetBytes(source).Take(2).ToArray());
            }

            [TestCase(1)]
            [TestCase(5)]
            [TestCase(10)]
            [TestCase(50)]
            public void PartialReadAndResetWithPreamble(int bufferCharCount)
            {
                // Given
                string source = "0123456789";
                Encoding encoding = new UTF8Encoding(true);
                StringStream stringStream = new StringStream(source, encoding, bufferCharCount);
                byte[] buffer = new byte[2];

                // When
                stringStream.ReadExactly(buffer, 0, 2);
                stringStream.Reset();
                stringStream.ReadExactly(buffer, 0, 2);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                buffer.ShouldBe(encoding.GetPreamble().Concat(encoding.GetBytes(source)).Take(2).ToArray());
            }
        }
    }
}
