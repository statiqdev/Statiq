using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Meta
{
    [TestFixture]
    public class LazyDocumentMetadataValueFixture : BaseFixture
    {
        public class GetTests : LazyDocumentMetadataValueFixture
        {
            [Test]
            public void FindsDocumentInInputs()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestExecutionContext context = new TestExecutionContext(a, b, c);
                LazyDocumentMetadataValue value = new LazyDocumentMetadataValue(b);

                // When
                object result = value.Get(null, null);

                // Then
                result.ShouldBe(b);
            }

            [Test]
            public void FindsDocumentInChildren()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument b1 = new TestDocument();
                TestDocument b2 = new TestDocument();
                TestDocument b3 = new TestDocument();
                b.Add(Keys.Children, new IDocument[] { b1, b2, b3 });
                TestDocument c = new TestDocument();
                TestExecutionContext context = new TestExecutionContext(a, b, c);
                LazyDocumentMetadataValue value = new LazyDocumentMetadataValue(b2);

                // When
                object result = value.Get(null, null);

                // Then
                result.ShouldBe(b2);
            }

            [Test]
            public void FindsDocumentInParentContext()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestDocument e = new TestDocument();
                TestDocument f = new TestDocument();
                TestExecutionContext parent = new TestExecutionContext(d, e, f);
                TestExecutionContext context = new TestExecutionContext(a, b, c);
                context.Parent = parent;
                LazyDocumentMetadataValue value = new LazyDocumentMetadataValue(e);

                // When
                object result = value.Get(null, null);

                // Then
                result.ShouldBe(e);
                IExecutionContext.Current.ShouldBe(context); // Sanity check
            }

            [Test]
            public void FindsDocumentInChildOfParentContext()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestDocument e = new TestDocument();
                TestDocument e1 = new TestDocument();
                TestDocument e2 = new TestDocument();
                TestDocument e3 = new TestDocument();
                e.Add(Keys.Children, new IDocument[] { e1, e2, e3 });
                TestDocument f = new TestDocument();
                TestExecutionContext parent = new TestExecutionContext(d, e, f);
                TestExecutionContext context = new TestExecutionContext(a, b, c);
                context.Parent = parent;
                LazyDocumentMetadataValue value = new LazyDocumentMetadataValue(e2);

                // When
                object result = value.Get(null, null);

                // Then
                result.ShouldBe(e2);
                IExecutionContext.Current.ShouldBe(context); // Sanity check
            }

            [Test]
            public void DoesNotThrowForNullOriginalDocument()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                LazyDocumentMetadataValue value = new LazyDocumentMetadataValue();

                // When
                object result = value.Get(null, null);

                // Then
                result.ShouldBeNull();
            }

            [Test]
            public void ReturnsOriginalDocumentIfNotFound()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                TestDocument c = new TestDocument();
                TestDocument d = new TestDocument();
                TestExecutionContext context = new TestExecutionContext(a, b, c);
                LazyDocumentMetadataValue value = new LazyDocumentMetadataValue(d);

                // When
                object result = value.Get(null, null);

                // Then
                result.ShouldBe(d);
            }
        }
    }
}
