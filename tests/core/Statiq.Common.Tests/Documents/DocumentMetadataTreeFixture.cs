using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Documents
{
    [TestFixture]
    public class DocumentMetadataTreeFixture : BaseFixture
    {
        public class GetParentOfTests : DocumentMetadataTreeFixture
        {
            [Test]
            public void GetsParent()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            new TestDocument("B")
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { x, y });

                // When
                IDocument result = tree.GetParentOf(a);

                // Then
                result.ShouldBe(x);
            }

            [Test]
            public void ReturnsNullWhenNoParent()
            {
                // Given
                IDocument a = new TestDocument("A");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            new TestDocument("B")
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { x, y });

                // When
                IDocument result = tree.GetParentOf(a);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public void GetsRecursiveParent()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            new TestDocument("B")
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            new TestDocument("C"),
                            x
                        }
                    }
                };
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { y, z });

                // When
                IDocument result = tree.GetParentOf(a);

                // Then
                result.ShouldBe(x);
            }
        }

        public class GetSiblingsOfTests : DocumentMetadataTreeFixture
        {
            [Test]
            public void GetsSiblings()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument b = new TestDocument("B");
                IDocument c = new TestDocument("C");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            b,
                            c
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            new TestDocument("C"),
                            x
                        }
                    }
                };
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { y, z });

                // When
                DocumentList<IDocument> result = tree.GetSiblingsOf(a);

                // Then
                result.ShouldBe(new IDocument[] { b, c });
            }

            [Test]
            public void GetsSiblingsAndSelf()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument b = new TestDocument("B");
                IDocument c = new TestDocument("C");
                IDocument d = new TestDocument("D");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            b,
                            c
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            d,
                            x
                        }
                    }
                };
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { y, z });

                // When
                DocumentList<IDocument> result = tree.GetSiblingsOf(a, true);

                // Then
                result.ShouldBe(new IDocument[] { a, b, c });
            }
        }

        public class GetDescendantsOfTests : DocumentMetadataTreeFixture
        {
            [Test]
            public void GetsDescendants()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument b = new TestDocument("B");
                IDocument c = new TestDocument("C");
                IDocument d = new TestDocument("D");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            b,
                            c
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            d,
                            x
                        }
                    }
                };
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { y, z });

                // When
                DocumentList<IDocument> result = tree.GetDescendantsOf(z);

                // Then
                result.ShouldBe(new IDocument[] { d, x, a, b, c });
            }

            [Test]
            public void GetsDescendantsAndSelf()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument b = new TestDocument("B");
                IDocument c = new TestDocument("C");
                IDocument d = new TestDocument("D");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            b,
                            c
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            d,
                            x
                        }
                    }
                };
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { y, z });

                // When
                DocumentList<IDocument> result = tree.GetDescendantsOf(z, true);

                // Then
                result.ShouldBe(new IDocument[] { z, d, x, a, b, c });
            }
        }

        public class GetAncestorsOfTests : DocumentMetadataTreeFixture
        {
            [Test]
            public void GetsAncestors()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument b = new TestDocument("B");
                IDocument c = new TestDocument("C");
                IDocument d = new TestDocument("D");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            b,
                            c
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            d,
                            x
                        }
                    }
                };
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { y, z });

                // When
                DocumentList<IDocument> result = tree.GetAncestorsOf(b);

                // Then
                result.ShouldBe(new IDocument[] { x, z });
            }

            [Test]
            public void GetsDescendantsAndSelf()
            {
                // Given
                _ = new TestExecutionContext();
                IDocument a = new TestDocument("A");
                IDocument b = new TestDocument("B");
                IDocument c = new TestDocument("C");
                IDocument d = new TestDocument("D");
                IDocument x = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            a,
                            b,
                            c
                        }
                    }
                };
                IDocument y = new TestDocument("Y");
                IDocument z = new TestDocument("Z")
                {
                    {
                        Keys.Children,
                        new IDocument[]
                        {
                            d,
                            x
                        }
                    }
                };
                DocumentMetadataTree<IDocument> tree = new DocumentMetadataTree<IDocument>(new[] { y, z });

                // When
                DocumentList<IDocument> result = tree.GetAncestorsOf(b, true);

                // Then
                result.ShouldBe(new IDocument[] { b, x, z });
            }
        }
    }
}
