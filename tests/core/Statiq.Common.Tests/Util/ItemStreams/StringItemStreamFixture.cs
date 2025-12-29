using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util.ItemStreams
{
    [TestFixture]
    public class StringItemStreamFixture : BaseFixture
    {
        public class ReadTests : StringStreamFixture
        {
            [Test]
            public void EncodesItemsWithoutPreamble()
            {
                // Given
                string[] source = { "0", string.Empty, "12", null, "3456", "789" };
                Encoding encoding = new UTF8Encoding(false);
                StringItemStream stringItemStream = new StringItemStream(source, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringItemStream.CopyTo(memoryStream);

                // Then
                memoryStream.ToArray().ShouldBe(encoding.GetBytes("0123456789"));
            }

            [TestCase]
            public void EncodesItemsWithPreamble()
            {
                // Given
                string[] source = { "0", string.Empty, "12", null, "3456", "789" };
                Encoding encoding = new UTF8Encoding(true);
                StringItemStream stringItemStream = new StringItemStream(source, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringItemStream.CopyTo(memoryStream);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                memoryStream.ToArray().ShouldBe(encoding.GetPreamble().Concat(encoding.GetBytes("0123456789")).ToArray());
            }

            [Test]
            public void NullItemsWithoutPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(false);
                StringItemStream stringItemStream = new StringItemStream(null, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringItemStream.CopyTo(memoryStream);

                // Then
                memoryStream.ToArray().ShouldBeEmpty();
            }

            [Test]
            public void EmptyItemsWithoutPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(false);
                StringItemStream stringItemStream = new StringItemStream(Array.Empty<string>(), encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringItemStream.CopyTo(memoryStream);

                // Then
                memoryStream.ToArray().ShouldBeEmpty();
            }

            [Test]
            public void NullItemsWithPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(true);
                StringItemStream stringItemStream = new StringItemStream(null, encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringItemStream.CopyTo(memoryStream);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                memoryStream.ToArray().ShouldBe(encoding.GetPreamble());
            }

            [Test]
            public void EmptyItemsWithPreamble()
            {
                // Given
                Encoding encoding = new UTF8Encoding(true);
                StringItemStream stringItemStream = new StringItemStream(Array.Empty<string>(), encoding);
                MemoryStream memoryStream = new MemoryStream();

                // When
                stringItemStream.CopyTo(memoryStream);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                memoryStream.ToArray().ShouldBe(encoding.GetPreamble());
            }

            [Test]
            public void PartialReadAndResetWithoutPreamble()
            {
                // Given
                string[] source = { "0", string.Empty, "12", null, "3456", "789" };
                Encoding encoding = new UTF8Encoding(false);
                StringItemStream stringItemStream = new StringItemStream(source, encoding);
                byte[] buffer = new byte[2];

                // When
                stringItemStream.ReadExactly(buffer, 0, 2);
                stringItemStream.Reset();
                stringItemStream.ReadExactly(buffer, 0, 2);

                // Then
                buffer.ShouldBe(encoding.GetBytes("0123456789").Take(2).ToArray());
            }

            [Test]
            public void PartialReadAndResetWithPreamble()
            {
                // Given
                string[] source = { "0", string.Empty, "12", null, "3456", "789" };
                Encoding encoding = new UTF8Encoding(true);
                StringItemStream stringItemStream = new StringItemStream(source, encoding);
                byte[] buffer = new byte[2];

                // When
                stringItemStream.ReadExactly(buffer, 0, 2);
                stringItemStream.Reset();
                stringItemStream.ReadExactly(buffer, 0, 2);

                // Then
                encoding.GetPreamble().Length.ShouldBeGreaterThan(0);
                buffer.ShouldBe(encoding.GetPreamble().Concat(encoding.GetBytes("0123456789")).Take(2).ToArray());
            }
        }
    }
}
