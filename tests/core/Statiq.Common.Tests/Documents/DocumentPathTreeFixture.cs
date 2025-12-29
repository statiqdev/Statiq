using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Shouldly.Configuration;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class DocumentPathTreeFixture : BaseFixture
    {
        public class GetParentOfTests : DocumentPathTreeFixture
        {
            [TestCase("1.html", "index.html")]
            [TestCase("index.html", null)]
            [TestCase("a/2.html", null)]
            [TestCase("b/4.html", "b/index.html")]
            [TestCase("b/index.html", "index.html")]
            [TestCase("b/c/6.html", null)]
            [TestCase("b/c/d/9.html", "b/c/d/index.html")]
            public void GetsParent(string path, string expected)
            {
                // Given
                IDocument[] documents = GetDocuments();
                DocumentPathTree<IDocument> tree = new DocumentPathTree<IDocument>(documents, x => x.Destination);
                IDocument document = Array.Find(documents, x => x.Destination.Equals(path));

                // When
                IDocument result = tree.GetParentOf(document);

                // Then
                if (expected is null)
                {
                    result.ShouldBeNull();
                }
                else
                {
                    result.Destination.Equals(expected).ShouldBeTrue();
                }
            }
        }

        public class GetChildrenOfTests : DocumentPathTreeFixture
        {
            [TestCase("b/4.html", new string[] { })]
            [TestCase("b/index.html", new string[] { "b/4.html", "b/5.html" })]
            [TestCase("1.html", new string[] { })]
            [TestCase("index.html", new string[] { "1.html", "b/index.html", "c/index.html" })]
            public void GetsChildren(string path, string[] expected)
            {
                // Given
                IDocument[] documents = GetDocuments();
                DocumentPathTree<IDocument> tree = new DocumentPathTree<IDocument>(documents, x => x.Destination);
                IDocument document = Array.Find(documents, x => x.Destination.Equals(path));

                // When
                DocumentList<IDocument> results = tree.GetChildrenOf(document);

                // Then
                results.Select(x => x.Destination).ShouldBe(expected.Select(x => new NormalizedPath(x)));
            }
        }

        public class GetSiblingsOfTests : DocumentPathTreeFixture
        {
            [TestCase("1.html", new string[] { "b/index.html", "c/index.html" }, false)]
            [TestCase("1.html", new string[] { "1.html", "b/index.html", "c/index.html" }, true)]
            [TestCase("index.html", new string[] { }, false)]
            [TestCase("index.html", new string[] { }, true)]
            [TestCase("a/3.html", new string[] { "a/2.html" }, false)]
            [TestCase("a/3.html", new string[] { "a/2.html", "a/3.html" }, true)]
            [TestCase("b/index.html", new string[] { "1.html", "c/index.html" }, false)]
            [TestCase("b/index.html", new string[] { "1.html", "b/index.html", "c/index.html" }, true)]
            public void GetsSiblings(string path, string[] expected, bool includeSelf)
            {
                // Given
                IDocument[] documents = GetDocuments();
                DocumentPathTree<IDocument> tree = new DocumentPathTree<IDocument>(documents, x => x.Destination);
                IDocument document = Array.Find(documents, x => x.Destination.Equals(path));

                // When
                DocumentList<IDocument> results = tree.GetSiblingsOf(document, includeSelf);

                // Then
                results.Select(x => x.Destination).ShouldBe(expected.Select(x => new NormalizedPath(x)));
            }
        }

        public class GetDescendantsOfTests : DocumentPathTreeFixture
        {
            [TestCase("b/4.html", new string[] { }, false)]
            [TestCase("b/4.html", new string[] { "b/4.html" }, true)]
            [TestCase("b/index.html", new string[] { "b/4.html", "b/5.html", "b/c/6.html", "b/c/7.html", "b/c/d/8.html", "b/c/d/9.html", "b/c/d/index.html", "b/e/10.html" }, false)]
            [TestCase("b/index.html", new string[] { "b/4.html", "b/5.html", "b/index.html", "b/c/6.html", "b/c/7.html", "b/c/d/8.html", "b/c/d/9.html", "b/c/d/index.html", "b/e/10.html" }, true)]
            [TestCase("1.html", new string[] { }, false)]
            [TestCase("1.html", new string[] { "1.html" }, true)]
            [TestCase("index.html", new string[] { "1.html", "a/2.html", "a/3.html", "b/4.html", "b/5.html", "b/index.html", "b/c/6.html", "b/c/7.html", "b/c/d/8.html", "b/c/d/9.html", "b/c/d/index.html", "b/e/10.html", "c/index.html" }, false)]
            [TestCase("index.html", new string[] { "1.html", "index.html", "a/2.html", "a/3.html", "b/4.html", "b/5.html", "b/index.html", "b/c/6.html", "b/c/7.html", "b/c/d/8.html", "b/c/d/9.html", "b/c/d/index.html", "b/e/10.html", "c/index.html" }, true)]
            public void GetsDescendants(string path, string[] expected, bool includeSelf)
            {
                // Given
                IDocument[] documents = GetDocuments();
                DocumentPathTree<IDocument> tree = new DocumentPathTree<IDocument>(documents, x => x.Destination);
                IDocument document = Array.Find(documents, x => x.Destination.Equals(path));

                // When
                DocumentList<IDocument> results = tree.GetDescendantsOf(document, includeSelf);

                // Then
                results.Select(x => x.Destination).ShouldBe(expected.Select(x => new NormalizedPath(x)));
            }
        }

        public class GetAncestorsOfTests : DocumentPathTreeFixture
        {
            [TestCase("index.html", new string[] { }, false)]
            [TestCase("index.html", new string[] { "index.html" }, true)]
            [TestCase("1.html", new string[] { "index.html" }, false)]
            [TestCase("1.html", new string[] { "1.html", "index.html" }, true)]
            [TestCase("b/c/d/9.html", new string[] { "b/c/d/index.html", "b/index.html", "index.html" }, false)]
            [TestCase("b/c/d/9.html", new string[] { "b/c/d/9.html", "b/c/d/index.html", "b/index.html", "index.html" }, true)]
            [TestCase("b/c/d/index.html", new string[] { "b/index.html", "index.html" }, false)]
            [TestCase("b/c/d/index.html", new string[] { "b/c/d/index.html", "b/index.html", "index.html" }, true)]
            public void GetsAncestors(string path, string[] expected, bool includeSelf)
            {
                // Given
                IDocument[] documents = GetDocuments();
                DocumentPathTree<IDocument> tree = new DocumentPathTree<IDocument>(documents, x => x.Destination);
                IDocument document = Array.Find(documents, x => x.Destination.Equals(path));

                // When
                DocumentList<IDocument> results = tree.GetAncestorsOf(document, includeSelf);

                // Then
                results.Select(x => x.Destination).ShouldBe(expected.Select(x => new NormalizedPath(x)));
            }
        }

        public IDocument[] GetDocuments() => new string[]
        {
            "1.html",
            "index.html",
            "a/2.html",
            "a/3.html",
            "b/4.html",
            "b/5.html",
            "b/index.html",
            "b/c/6.html",
            "b/c/7.html",
            "b/c/d/8.html",
            "b/c/d/9.html",
            "b/c/d/index.html",
            "b/e/10.html",
            "c/index.html"
        }.Select(x => new TestDocument(new NormalizedPath(x), x)).ToArray();
    }
}
