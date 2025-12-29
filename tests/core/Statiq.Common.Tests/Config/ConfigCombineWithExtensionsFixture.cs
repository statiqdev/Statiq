using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Config
{
    [TestFixture]
    public class ConfigCombineWithExtensionsFixture : BaseFixture
    {
        public class CombineWithTests : ConfigCombineWithExtensionsFixture
        {
            [TestCase(false, false)]
            [TestCase(false, true)]
            [TestCase(true, false)]
            [TestCase(true, true)]
            public async Task CombinesPredicateConfigs(bool first, bool second)
            {
                // Given
                Config<bool> firstConfig = Common.Config.FromValue(first);
                Config<bool> secondConfig = Common.Config.FromValue(second);

                // When
                Config<bool> result = firstConfig.CombineWith(secondConfig);

                // Then
                (await result.GetValueAsync(null, null)).ShouldBe(first && second);
            }

            [Test]
            public void ReturnsSecondPredicateWhenFirstIsNull()
            {
                // Given
                Config<bool> firstConfig = Common.Config.FromValue(false);
                Config<bool> secondConfig = null;

                // When
                Config<bool> result = firstConfig.CombineWith(secondConfig);

                // Then
                result.ShouldBe(firstConfig);
            }

            [Test]
            public void ReturnsFirstPredicateWhenSecondIsNull()
            {
                // Given
                Config<bool> firstConfig = null;
                Config<bool> secondConfig = Common.Config.FromValue(false);

                // When
                Config<bool> result = firstConfig.CombineWith(secondConfig);

                // Then
                result.ShouldBe(secondConfig);
            }

            [Test]
            public async Task ReturnsFalseWhenBothPredicatesAreNull()
            {
                // Given
                Config<bool> firstConfig = null;
                Config<bool> secondConfig = null;

                // When
                Config<bool> result = firstConfig.CombineWith(secondConfig);

                // Then
                (await result.GetValueAsync(null, null)).ShouldBeFalse();
            }
        }
    }
}
