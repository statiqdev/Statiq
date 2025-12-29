using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO.Documents
{
    [TestFixture]
    public class DocumentFileProviderFixture : BaseFixture
    {
        public class ConstructorTests : DocumentFileProviderFixture
        {
            [Test]
            public void FlattensDocuments()
            {
                // Given
                IEnumerable<TestDocument> documents = GetDocuments();

                // When
                DocumentFileProvider provider = new DocumentFileProvider(documents, true);

                // Then
                provider.GetFile("/input/a/a.md").Exists.ShouldBeTrue();
                provider.GetFile("/input/a/b.md").Exists.ShouldBeTrue();
                provider.GetFile("/input/c/d/c.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/a.md").Exists.ShouldBeFalse();
                provider.GetFile("/a/b.md").Exists.ShouldBeFalse();
                provider.GetFile("/c/c.md").Exists.ShouldBeFalse();
            }

            [Test]
            public void DoesNotFlattenDocuments()
            {
                // Given
                IEnumerable<TestDocument> documents = GetDocuments();

                // When
                DocumentFileProvider provider = new DocumentFileProvider(documents, true, false);

                // Then
                provider.GetFile("/input/a/a.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/a/b.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/c/d/c.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/a.md").Exists.ShouldBeFalse();
                provider.GetFile("/a/b.md").Exists.ShouldBeFalse();
                provider.GetFile("/c/c.md").Exists.ShouldBeFalse();
            }

            [Test]
            public void UsesDestination()
            {
                // Given
                IEnumerable<TestDocument> documents = GetDocuments();

                // When
                DocumentFileProvider provider = new DocumentFileProvider(documents, false);

                // Then
                provider.GetFile("/input/a/a.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/a/b.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/c/d/c.md").Exists.ShouldBeFalse();
                provider.GetFile("/a/a.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/b.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/c.md").Exists.ShouldBeTrue();
            }

            [Test]
            public void IgnoresNullDestination()
            {
                // Given
                IEnumerable<TestDocument> documents = GetDocuments()
                    .Concat(new TestDocument((NormalizedPath)"/input/z/z.md", NormalizedPath.Null)
                    {
                        { "Name", "a" },
                        { "A", "a" }
                    });

                // When
                DocumentFileProvider provider = new DocumentFileProvider(documents, false);

                // Then
                provider.GetFile("/input/a/a.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/a/b.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/c/d/c.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/z/z.md").Exists.ShouldBeFalse();
                provider.GetFile("/a/a.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/b.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/c.md").Exists.ShouldBeTrue();
                provider.GetFile(string.Empty).Exists.ShouldBeFalse();
                provider.GetFile("/").Exists.ShouldBeFalse();
            }

            [Test]
            public void EmptyDestinationIsCombinedWithAbsoluteRoot()
            {
                // Given
                IEnumerable<TestDocument> documents = GetDocuments()
                    .Concat(new TestDocument((NormalizedPath)"/input/z/z.md", NormalizedPath.Empty)
                    {
                        { "Name", "a" },
                        { "A", "a" }
                    });

                // When
                DocumentFileProvider provider = new DocumentFileProvider(documents, false);

                // Then
                provider.GetFile("/input/a/a.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/a/b.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/c/d/c.md").Exists.ShouldBeFalse();
                provider.GetFile("/input/z/z.md").Exists.ShouldBeFalse();
                provider.GetFile("/a/a.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/b.md").Exists.ShouldBeTrue();
                provider.GetFile("/a/c.md").Exists.ShouldBeTrue();
                provider.GetFile(string.Empty).Exists.ShouldBeFalse();
                provider.GetFile("/").Exists.ShouldBeTrue();
            }

            [Test]
            public void AddsParentDirectories()
            {
                // Given
                IEnumerable<TestDocument> documents = GetDocuments();

                // When
                DocumentFileProvider provider = new DocumentFileProvider(documents, true);

                // Then
                provider.GetDirectory("/input").Exists.ShouldBeTrue();
                provider.GetDirectory("/input/a").Exists.ShouldBeTrue();
                provider.GetDirectory("/input/c").Exists.ShouldBeTrue();
                provider.GetDirectory("/input/c/d").Exists.ShouldBeTrue();
            }

            public IEnumerable<TestDocument> GetDocuments()
            {
                TestDocument a = new TestDocument((NormalizedPath)"/input/a/a.md", (NormalizedPath)"a/a.md")
                {
                    { "Name", "a" },
                    { "A", "a" }
                };
                TestDocument b = new TestDocument((NormalizedPath)"/input/a/b.md", (NormalizedPath)"a/b.md")
                {
                    { "Name", "b" },
                    { "B", "b" }
                };
                return new TestDocument((NormalizedPath)"/input/c/d/c.md", (NormalizedPath)"a/c.md")
                {
                    { "Name", "c" },
                    { "C", "c" },
                    { Keys.Children, new IDocument[] { a, b } }
                }.Yield();
            }
        }
    }
}