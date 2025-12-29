using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class AppendContentFixture : BaseFixture
    {
        public class ExecuteTests : AppendContentFixture
        {
            [Test]
            public async Task AppendsContent()
            {
                // Given
                TestDocument input = new TestDocument("ABC");
                AppendContent append = new AppendContent("123");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, append);

                // Then
                results.Single().Content.ShouldBe("ABC123");
            }

            [Test]
            public async Task KeepsSameMediaType()
            {
                // Given
                TestDocument input = new TestDocument("ABC", "Foo");
                AppendContent append = new AppendContent("123");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, append);

                // Then
                results.Single().ContentProvider.MediaType.ShouldBe("Foo");
            }
        }
    }
}
