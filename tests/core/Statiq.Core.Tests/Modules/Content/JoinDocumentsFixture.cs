using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class JoinDocumentsFixture : BaseFixture
    {
        [Test]
        public async Task TwoDocumentsWithNoDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            results.Single().Content.ShouldBe("TestTest2");
        }

        [Test]
        public async Task ThreeDocumentsWithNoDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            TestDocument third = new TestDocument("Test3");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("TestTest2Test3");
        }

        [Test]
        public async Task ThreeDocumentsWithDelimiterAndNoContent()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument();
            TestDocument third = new TestDocument("Test3");
            JoinDocuments join = new JoinDocuments(",");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("Test,Test3");
        }

        [Test]
        public async Task ResultHasSameMediaTypeWhenSame()
        {
            // Given
            TestDocument first = new TestDocument("Test", "Foo");
            TestDocument second = new TestDocument();
            TestDocument third = new TestDocument("Test3", "Foo");
            JoinDocuments join = new JoinDocuments(",");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("Test,Test3");
            results.Single().ContentProvider.MediaType.ShouldBe("Foo");
        }

        [Test]
        public async Task ResultHasNoMediaTypeWhenDifferent()
        {
            // Given
            TestDocument first = new TestDocument("Test", "Foo");
            TestDocument second = new TestDocument();
            TestDocument third = new TestDocument("Test3", "Bar");
            JoinDocuments join = new JoinDocuments(",");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("Test,Test3");
            results.Single().ContentProvider.MediaType.ShouldBe(null);
        }

        [Test]
        public async Task ThreeDocumentsWithNoDelimiterWhenFirstIsNull()
        {
            // Given
            TestDocument first = null;
            TestDocument second = new TestDocument("Test2");
            TestDocument third = new TestDocument("Test3");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("Test2Test3");
        }

        [Test]
        public async Task ThreeDocumentsWithNoDelimiterWhenSecondIsNull()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = null;
            TestDocument third = new TestDocument("Test3");
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second, third }, join);

            // Then
            results.Single().Content.ShouldBe("TestTest3");
        }

        [Test]
        public async Task NullPassedInAsDocumentList()
        {
            // Given
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(join);

            // Then
            results.Single().Content.ShouldBeEmpty();
        }

        [Test]
        public async Task TwoDocumentsJoinWithCommaDelimiter()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments(",");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            results.Single().Content.ShouldBe("Test,Test2");
        }

        [Test]
        public async Task TwoDocumentsJoinWithDelimiterInText()
        {
            // Given
            TestDocument first = new TestDocument("Test");
            TestDocument second = new TestDocument("Test2");
            JoinDocuments join = new JoinDocuments("Test");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            results.Single().Content.ShouldBe("TestTestTest2");
        }

        [Test]
        public async Task TwoDocumentsWithKeepFirstMetaDataReturnKeepsFirstMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.FirstDocument);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Keys.ShouldContain("one");
            result.Keys.ShouldNotContain("three");
        }

        [Test]
        public async Task TwoDocumentsWithMetaDataReturnDefaultMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Keys.ShouldNotContain("one");
            result.Keys.ShouldNotContain("three");
        }

        [Test]
        public async Task TwoDocumentsWithKeepLastMetaDataReturnKeepsLastMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.LastDocument);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Keys.ShouldNotContain("one");
            result.Keys.ShouldContain("three");
        }

        [Test]
        public async Task TwoDocumentsWithAllKeepFirstMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "one", "seven" },
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.AllWithFirstDuplicates);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Values.ShouldContain("two");
            result.Values.ShouldNotContain("seven");
            result.Keys.ShouldContain("three");
        }

        [Test]
        public async Task TwoDocumentsWithAllKeepLastMetaData()
        {
            // Given
            TestDocument first = new TestDocument("Test")
            {
                { "one", "two" }
            };
            TestDocument second = new TestDocument("Test2")
            {
                { "one", "seven" },
                { "three", "four" }
            };
            JoinDocuments join = new JoinDocuments(JoinedMetadata.AllWithLastDuplicates);

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { first, second }, join);

            // Then
            TestDocument result = results.Single();
            result.Values.ShouldNotContain("two");
            result.Values.ShouldContain("seven");
            result.Keys.ShouldContain("three");
        }

        [Test]
        public async Task EmptyListDoesNotError()
        {
            // Given
            JoinDocuments join = new JoinDocuments();

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(join);

            // Then
            results.Single().Content.ShouldBeEmpty();
        }

        [Test]
        public async Task EmptyListWithDelimitorDoesNotError()
        {
            // Given
            JoinDocuments join = new JoinDocuments(",");

            // When
            ImmutableArray<TestDocument> results = await ExecuteAsync(join);

            // Then
            results.Single().Content.ShouldBeEmpty();
        }
    }
}
