using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Util
{
    // The process launcher tests only seem reliable on Windows (though they work intermittently on other platforms so the launcher itself seems okay)
    [NonParallelizable]
    [TestFixture]
    public class ProcessLauncherFixture : BaseFixture
    {
        public class StartNewTests : ProcessLauncherFixture
        {
            [WindowsTest]
            public void ThrowsForSynchronousError()
            {
                // Given
                ProcessLauncher processLauncher = new ProcessLauncher("dotnet", "run foobar");
                StringWriter outputWriter = new StringWriter();
                StringWriter errorWriter = new StringWriter();
                TestLoggerProvider testLoggerProvider = new TestLoggerProvider(LogLevel.None);

                // When, Then
                Should.Throw<Exception>(() => processLauncher.StartNew(outputWriter, errorWriter, testLoggerProvider.CreateLoggerFactory()));
                errorWriter.ToString().ShouldContain("Couldn't find a project to run.");
            }

            [WindowsTest]
            public void DoesNotThrowForBackgroundError()
            {
                // Given
                ProcessLauncher processLauncher = new ProcessLauncher("dotnet", "run foobar")
                {
                    IsBackground = true
                };
                StringWriter outputWriter = new StringWriter();
                StringWriter errorWriter = new StringWriter();
                TestLoggerProvider testLoggerProvider = new TestLoggerProvider(LogLevel.None);

                // When
                int exitCode = processLauncher.StartNew(outputWriter, errorWriter, testLoggerProvider.CreateLoggerFactory());

                // Then
                exitCode.ShouldBe(0);
                int count = 0;
                while (processLauncher.AreAnyRunning)
                {
                    Thread.Sleep(1000);
                    count++;
                    if (count > 10)
                    {
                        throw new Exception("Process never returned: " + outputWriter.ToString());
                    }
                }
                errorWriter.ToString().ShouldContain("Couldn't find a project to run.");
            }

            [WindowsTest]
            public void LaunchesProcess()
            {
                // Given
                NormalizedPath projectPath = new NormalizedPath(typeof(ProcessLauncherFixture).Assembly.Location)
                    .Parent.Parent.Parent.Parent.Parent.Combine("TestConsoleApp");
                ProcessLauncher processLauncher = new ProcessLauncher("dotnet", $"run --project \"{projectPath.FullPath}\"");
                StringWriter outputWriter = new StringWriter();
                StringWriter errorWriter = new StringWriter();
                TestLoggerProvider testLoggerProvider = new TestLoggerProvider(LogLevel.None);

                // When
                int exitCode = processLauncher.StartNew(outputWriter, errorWriter, testLoggerProvider.CreateLoggerFactory());

                // Then
                exitCode.ShouldBe(0);
                outputWriter.ToString().ShouldContain("Finished");
            }

            [WindowsTest]
            public void ReturnsExitCode()
            {
                // Given
                NormalizedPath projectPath = new NormalizedPath(typeof(ProcessLauncherFixture).Assembly.Location)
                    .Parent.Parent.Parent.Parent.Parent.Combine("TestConsoleApp");
                ProcessLauncher processLauncher = new ProcessLauncher("dotnet", $"run --project \"{projectPath.FullPath}\" -- 0 123")
                {
                    IsErrorExitCode = _ => false
                };
                StringWriter outputWriter = new StringWriter();
                StringWriter errorWriter = new StringWriter();
                TestLoggerProvider testLoggerProvider = new TestLoggerProvider(LogLevel.None);

                // When
                int exitCode = processLauncher.StartNew(outputWriter, errorWriter, testLoggerProvider.CreateLoggerFactory());

                // Then
                exitCode.ShouldBe(123);
                outputWriter.ToString().ShouldContain("Finished");
            }

            [WindowsTest]
            public void SupportsCancellation()
            {
                // Given
                NormalizedPath projectPath = new NormalizedPath(typeof(ProcessLauncherFixture).Assembly.Location)
                    .Parent.Parent.Parent.Parent.Parent.Combine("TestConsoleApp");
                ProcessLauncher processLauncher = new ProcessLauncher("dotnet", $"run --project \"{projectPath.FullPath}\" -- 10");
                StringWriter outputWriter = new StringWriter();
                StringWriter errorWriter = new StringWriter();
                CancellationTokenSource cts = new CancellationTokenSource(5000);
                TestLoggerProvider testLoggerProvider = new TestLoggerProvider(LogLevel.None);

                // When
                Should.Throw<Exception>(() => processLauncher.StartNew(outputWriter, errorWriter, testLoggerProvider.CreateLoggerFactory(), cts.Token));

                // Then
                outputWriter.ToString().ShouldNotContain("Finished");
                processLauncher.AreAnyRunning.ShouldBeFalse();
            }

            [WindowsTest]
            public void SupportsBackgroundCancellation()
            {
                // Given
                NormalizedPath projectPath = new NormalizedPath(typeof(ProcessLauncherFixture).Assembly.Location)
                    .Parent.Parent.Parent.Parent.Parent.Combine("TestConsoleApp");
                ProcessLauncher processLauncher = new ProcessLauncher("dotnet", $"run --project \"{projectPath.FullPath}\" -- 10")
                {
                    IsBackground = true
                };
                StringWriter outputWriter = new StringWriter();
                StringWriter errorWriter = new StringWriter();
                CancellationTokenSource cts = new CancellationTokenSource(5000);
                TestLoggerProvider testLoggerProvider = new TestLoggerProvider(LogLevel.None);

                // When
                int exitCode = processLauncher.StartNew(outputWriter, errorWriter, testLoggerProvider.CreateLoggerFactory(), cts.Token);

                // Then
                exitCode.ShouldBe(0);
                int count = 0;
                while (processLauncher.AreAnyRunning)
                {
                    Thread.Sleep(1000);
                    count++;
                    if (count > 10)
                    {
                        throw new Exception("Process never returned: " + outputWriter.ToString());
                    }
                }
                outputWriter.ToString().ShouldNotContain("Finished");
                processLauncher.AreAnyRunning.ShouldBeFalse();
            }
        }
    }
}
