using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.App.Tests.Bootstrapper
{
    [TestFixture]
    [NonParallelizable]
    public class BootstrapperFixture : BaseFixture
    {
        public class RunTests : BootstrapperFixture
        {
            [Test]
            public async Task NoPipelinesWarning()
            {
                // Given
                string[] args = new[] { "build" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.Create(args);
                bootstrapper.AddCommand<PipelinesCommand<EngineCommandSettings>>("build");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(LogLevel.None);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.LogMessages.ShouldContain(x =>
                    x.LogLevel == LogLevel.Warning
                    && x.FormattedMessage == "No pipelines are configured or specified for execution.");
            }

            [TestCase("Trace", 22)] // Includes module start/finish
            [TestCase("Debug", 21)] // Include modules start/finish
            [TestCase("Information", 4)] // Includes pipeline finish
            [TestCase("Warning", 3)]
            [TestCase("Error", 2)]
            [TestCase("Critical", 1)]
            public async Task SetsLogLevel(string logLevel, int expected)
            {
                // Given
                string[] args = new[] { "build", "-l", logLevel };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .Create(args)
                    .AddSetting(Keys.FailureLogLevel, LogLevel.None)
                    .AddCommand<PipelinesCommand<EngineCommandSettings>>("build")
                    .AddPipeline(
                        "Foo",
                        new LogMessage(LogLevel.Trace, "A"),
                        new LogMessage(LogLevel.Debug, "B"),
                        new LogMessage(LogLevel.Information, "C"),
                        new LogMessage(LogLevel.Warning, "D"),
                        new LogMessage(LogLevel.Error, "E"),
                        new LogMessage(LogLevel.Critical, "F"));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(LogLevel.None);

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.LogMessages.Count(x => x.FormattedMessage.StartsWith("Foo/Process")).ShouldBe(expected);
            }

            [TestCase("Trace", false)]
            [TestCase("Debug", false)]
            [TestCase("Information", false)]
            [TestCase("Warning", false)]
            [TestCase("Error", true)]
            [TestCase("Critical", true)]
            public async Task DefaultFailureLogLevel(LogLevel logLevel, bool expectedFailure)
            {
                // Given
                string[] args = new[] { "build" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .Create(args)
                    .AddCommand<PipelinesCommand<EngineCommandSettings>>("build")
                    .AddPipeline("Foo", new LogMessage(logLevel, "A"));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(LogLevel.None);

                // Then
                result.ExitCode.ShouldBe(expectedFailure ? (int)ExitCode.LogLevelFailure : (int)ExitCode.Normal);
            }

            [TestCase("Trace", false)]
            [TestCase("Debug", false)]
            [TestCase("Information", false)]
            [TestCase("Warning", true)]
            [TestCase("Error", true)]
            [TestCase("Critical", true)]
            public async Task SetFailureLogLevel(LogLevel logLevel, bool expectedFailure)
            {
                // Given
                string[] args = new[] { "build" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory
                    .Create(args)
                    .AddSetting(Keys.FailureLogLevel, LogLevel.Warning)
                    .AddCommand<PipelinesCommand<EngineCommandSettings>>("build")
                    .AddPipeline("Foo", new LogMessage(logLevel, "A"));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync(LogLevel.None);

                // Then
                result.ExitCode.ShouldBe(expectedFailure ? (int)ExitCode.LogLevelFailure : (int)ExitCode.Normal);
            }

            [Test]
            public async Task CatalogsType()
            {
                // Given
                string[] args = new[] { "build", "-l", "Debug" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.Create(args);
                bootstrapper.AddCommand<PipelinesCommand<EngineCommandSettings>>("build");
                bootstrapper.AddPipeline("Foo");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                bootstrapper.ClassCatalog.GetTypesAssignableTo<BootstrapperFixture>().Count().ShouldBe(1);
                result.LogMessages.ShouldContain(x => x.FormattedMessage.StartsWith("Cataloging types in assembly"));
            }

            [Test]
            public async Task AddsComputedMetadataFromCommandLine()
            {
                // Given
                string[] args = new[] { "--setting", "Foo = => 1 + 1" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                object value = null;
                bootstrapper.AddPipeline(
                    "Test",
                    new ExecuteConfig(Config.FromSettings(x => value = x["Foo"])));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                value.ShouldBe(2);
            }

            [Test]
            public async Task CommandLineSettingsAreCaseInsensitive()
            {
                // Given
                string[] args = new[] { "--setting", "foo=bar" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                bootstrapper.AddPipeline("Test");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                result.Engine.Settings["Foo"].ShouldBe("bar");
            }
        }

        public class CreateDefaultTests : BootstrapperFixture
        {
            [Test]
            public async Task EnvironmentVariableConfiguration()
            {
                // Given
                string[] args = new string[] { };
                Environment.SetEnvironmentVariable(nameof(EnvironmentVariableConfiguration), "Foo");
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                object variable = null;
                bootstrapper.AddPipeline("Foo", new ExecuteConfig(Config.FromContext(x => variable = x.Settings[nameof(EnvironmentVariableConfiguration)])));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                variable.ShouldBe("Foo");
            }

            [Test]
            public async Task CommandLineSettingTakesPrecedenceOverEnvironmentVariables()
            {
                // Given
                string[] args = new string[] { "-s", $"{nameof(CommandLineSettingTakesPrecedenceOverEnvironmentVariables)}=Bar" };
                Environment.SetEnvironmentVariable(nameof(CommandLineSettingTakesPrecedenceOverEnvironmentVariables), "Foo");
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                object variable = null;
                bootstrapper.AddPipeline("Foo", new ExecuteConfig(Config.FromContext(x => variable = x.Settings[nameof(CommandLineSettingTakesPrecedenceOverEnvironmentVariables)])));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                variable.ShouldBe("Bar");
            }

            [Test]
            public async Task SetsDefaultSettings()
            {
                // Given
                string[] args = new string[] { };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                object variable = null;
                bootstrapper.AddPipeline("Foo", new ExecuteConfig(Config.FromContext(x => variable = x.Settings[Keys.LinkHideIndexPages])));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                variable.ShouldBe(true);
            }

            [Test]
            public async Task CommandLineSettingTakesPrecedenceOverDefaultSettings()
            {
                // Given
                string[] args = new string[] { "-s", $"{Keys.LinkHideIndexPages}=false" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                object variable = null;
                bootstrapper.AddPipeline("Foo", new ExecuteConfig(Config.FromContext(x => variable = x.Settings[Keys.LinkHideIndexPages])));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                variable.ShouldBe("false");
            }

            [Test]
            public async Task SetsFlagFromCommandLine()
            {
                // Given
                string[] args = new string[] { "--noclean" };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                object value = null;
                bootstrapper.AddPipeline("Foo", new ExecuteConfig(Config.FromContext(x => value = x.Settings[Keys.CleanMode])));

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                value.ShouldBe(CleanMode.None);
            }
        }

        public class ConfigureSettingsTests : BootstrapperFixture
        {
            [Test]
            public async Task CanReadConfigurationValues()
            {
                // Given
                string[] args = new string[] { };
                Environment.SetEnvironmentVariable(nameof(CanReadConfigurationValues), "Foo");
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                object variable = null;
                bootstrapper.ConfigureSettings(x => variable = x[nameof(CanReadConfigurationValues)]);
                bootstrapper.AddPipeline("Foo");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.ExitCode.ShouldBe((int)ExitCode.Normal);
                variable.ShouldBe("Foo");
            }
        }
    }
}
