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
    public class PrependContentFixture : BaseFixture
    {
        public class ExecuteTests : PrependContentFixture
        {
            [Test]
            public async Task PrependsContent()
            {
                // Given
                TestDocument input = new TestDocument("ABC");
                PrependContent append = new PrependContent("123");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, append);

                // Then
                results.Single().Content.ShouldBe("123ABC");
            }

            [Test]
            public async Task KeepsSameMediaType()
            {
                // Given
                TestDocument input = new TestDocument("ABC", "Foo");
                PrependContent append = new PrependContent("123");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(input, append);

                // Then
                results.Single().ContentProvider.MediaType.ShouldBe("Foo");
            }
        }
    }
}
