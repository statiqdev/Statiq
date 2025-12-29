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
    public class BootstrapperPipelineExtensionsFixture : BaseFixture
    {
        public class AddPipelineTests : BootstrapperPipelineExtensionsFixture
        {
            [Test]
            public async Task AddsNamedPipelineByTypeWithGeneric()
            {
                // Given
                string[] args = new string[] { };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                bootstrapper.AddSetting("Value", 1234);
                bootstrapper.AddPipeline<TestPipeline>();
                bootstrapper.AddPipeline<TestPipeline>("Foo");
                bootstrapper.AddPipeline<TestPipeline>("Bar");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.Engine.Pipelines.Count.ShouldBe(3);
                ((TestPipeline)result.Engine.Pipelines["TestPipeline"]).Value.ShouldBe("TestPipeline1234");
                ((TestPipeline)result.Engine.Pipelines["Foo"]).Value.ShouldBe("Foo1234");
                ((TestPipeline)result.Engine.Pipelines["Bar"]).Value.ShouldBe("Bar1234");
            }

            [Test]
            public async Task AddsNamedPipelineByTypeWithoutGeneric()
            {
                // Given
                string[] args = new string[] { };
                App.Bootstrapper bootstrapper = App.Bootstrapper.Factory.CreateDefault(args);
                bootstrapper.AddSetting("Value", 1234);
                bootstrapper.AddPipeline<TestPipeline>("Foo");
                bootstrapper.AddPipeline<TestPipeline>("Bar");

                // When
                BootstrapperTestResult result = await bootstrapper.RunTestAsync();

                // Then
                result.Engine.Pipelines.Count.ShouldBe(2);
                ((TestPipeline)result.Engine.Pipelines["Foo"]).Value.ShouldBe("Foo1234");
                ((TestPipeline)result.Engine.Pipelines["Bar"]).Value.ShouldBe("Bar1234");
            }
        }

        public class TestPipeline : Pipeline
        {
            public TestPipeline(ISettings settings)
            {
                ProcessModules = new ModuleList
                {
                    new ExecuteConfig(Config.FromContext(ctx => Value = ctx.PipelineName + settings.GetString("Value")))
                };
            }

            public string Value { get; private set; }
        }
    }
}