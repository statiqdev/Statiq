using System;
using System.Globalization;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    [TestFixture]
    public class DateTimeCultureExtensionsFixture : BaseFixture
    {
        public class GetDateTimeInputCultureTests : DateTimeCultureExtensionsFixture
        {
            [SetUp]
            public void SetThreadCulture()
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
            }

            [Test]
            public void GetsCurrentCultureIfNoSetting()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                CultureInfo result = context.GetDateTimeInputCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.CurrentCulture));
            }

            [Test]
            public void GetsSettingCulture()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeInputCulture] = "en-GB";

                // When
                CultureInfo result = context.GetDateTimeInputCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("en-GB")));
            }
        }

        public class GetDateTimeDisplayCultureTests : DateTimeCultureExtensionsFixture
        {
            [SetUp]
            public void SetThreadCulture()
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US");
            }

            [Test]
            public void GetsCurrentCultureIfNoSetting()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.CurrentCulture));
            }

            [Test]
            public void GetsTargetCultureIfNoSetting()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture("fr-FR");

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsSettingCultureIfSettingMatchesDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = "en-US";

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("en-US")));
            }

            [Test]
            public void GetsSettingCultureIfSettingDoesNotMatchDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = "fr-FR";

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsSettingCultureIfSettingDoesNotMatchDefaultAndNeutralTargetSpecified()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = "fr-FR";

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture("fr");

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsSettingCultureIfSettingDoesNotMatchDefaultAndSpecifiedTargetSpecified()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.DateTimeDisplayCulture] = "fr-FR";

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture("fr-LU");

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("fr-FR")));
            }

            [Test]
            public void GetsDefaultCultureIfCurrentCultureDoesNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

                // When
                CultureInfo result = context.GetDateTimeDisplayCulture();

                // Then
                Assert.That(result, Is.EqualTo(CultureInfo.GetCultureInfo("en-GB")));
            }
        }
    }
}
