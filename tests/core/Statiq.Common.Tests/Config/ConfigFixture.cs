using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Config
{
    [TestFixture]
    public class ConfigFixture : BaseFixture
    {
        public class CastOperatorTests : ConfigFixture
        {
#pragma warning disable CS0618 // Type or member is obsolete
            [Test]
            public async Task CastsToMatchingType()
            {
                // Given, When
                Config<int> config = 10;

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsToObject()
            {
                // Given, When
                Config<object> config = 10;

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromDocumentConfigToObject()
            {
                // Given, When
                Config<object> config = new Config<int>((_, __) => Task.FromResult(10));

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(10);
            }

            [Test]
            public async Task CastsFromDocumentConfigToObjectEnumerable()
            {
                // Given, When
                Config<IEnumerable<object>> config = new Config<int>((_, __) => Task.FromResult(10));

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(new object[] { 10 });
            }

            [Test]
            public async Task CastsFromDocumentConfigOfEnumerableToObjectEnumerable()
            {
                // Given, When
                Config<IEnumerable<object>> config =
                    new Config<IEnumerable<int>>((_, __) => Task.FromResult((IEnumerable<int>)new[] { 8, 9, 10 }));

                // Then
                (await config.GetAndTransformValueAsync(null, null)).ShouldBe(new object[] { 8, 9, 10 });
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
