using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    public class StartProcessFixture : BaseFixture
    {
        public class ExecuteTests : StartProcessFixture
        {
            [Test]
            public async Task LogsOutputToDebugByDefault()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info");

                // When
                await ExecuteAsync(context, startProcess);

                // Then
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Debug).ShouldContain(x => x.FormattedMessage.Contains("Started process"));
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Debug).ShouldContain(x => x.FormattedMessage.Contains("runtimes installed"));
            }

            [Test]
            public async Task LogsOutputToInformationWhenRequested()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info").LogOutput();

                // When
                await ExecuteAsync(context, startProcess);

                // Then
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Information).ShouldContain(x => x.FormattedMessage.Contains("Started process"));
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Information).ShouldContain(x => x.FormattedMessage.Contains("runtimes installed"));
            }

            [Test]
            public async Task ExecutesMultipleTimes()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info");

                // When
                await ExecuteAsync(context, startProcess);
                await ExecuteAsync(context, startProcess);

                // Then
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Debug).ShouldContain(x => x.FormattedMessage.Contains("Started process"), 2);
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Debug).ShouldContain(x => x.FormattedMessage.Contains("runtimes installed"), 2);
            }

            [Test]
            public async Task ExecutesOnceWhenRequested()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info").OnlyOnce();

                // When
                await ExecuteAsync(context, startProcess);
                await ExecuteAsync(context, startProcess);

                // Then
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Debug).ShouldContain(x => x.FormattedMessage.Contains("Started process"), 1);
                context.LogMessages.Where(x => x.LogLevel == LogLevel.Debug).ShouldContain(x => x.FormattedMessage.Contains("runtimes installed"), 1);
            }

            [Test]
            public async Task OutputsDocumentWithProcessOutput()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(context, startProcess);

                // Then
                results.Single().Content.ShouldStartWith(".NET");
                results.Single().Content.ShouldContain(Environment.NewLine);
                ((IDocument)results.Single()).GetInt(StartProcess.ExitCode).ShouldBe(0);
            }

            [Test]
            public async Task KeepsDocumentContentsWhenRequested()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--info").KeepContent();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(context, startProcess);

                // Then
                results.Single().Content.ShouldNotStartWith(".NET");
                ((IDocument)results.Single()).GetInt(StartProcess.ExitCode).ShouldBe(0);
            }

            [Test]
            public async Task ThrowsForNonZeroExitCode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--foo").LogOutput();

                // When, Then
                await Should.ThrowAsync<Exception>(async () => await ExecuteAsync(context, startProcess));
            }

            [Test]
            public async Task ContinuesOnError()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;
                StartProcess startProcess = new StartProcess("dotnet", "--foo").ContinueOnError();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(context, startProcess);

                // Then
                // Note that the output from .NET 6 and previous versions is different:
                // previous versions of the dotnet CLI will report there's no such option,
                // .NET 6 will attempt to match the --foo to a command or file and fail
                ((IDocument)results.Single()).GetInt(StartProcess.ExitCode).ShouldNotBe(0);
            }
        }
    }
}