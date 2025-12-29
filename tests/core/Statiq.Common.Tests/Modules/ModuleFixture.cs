using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Modules
{
    [TestFixture]
    public class ModuleFixture : BaseFixture
    {
        public class ExecuteTests : ModuleFixture
        {
            [Test]
            public async Task CallsOverrides()
            {
                // Given
                TestDocument a = new TestDocument("A");
                TestDocument b = new TestDocument("B");
                IExecutionContext context = new TestExecutionContext(a, b);
                TestModule module = new TestModule(null, null, false);

                // When
                IEnumerable<IDocument> outputs = await module.ExecuteAsync(context);

                // Then
                module.BeforeExecutionCount.ShouldBe(1);
                module.BeforeExecutionAsyncCount.ShouldBe(1);
                module.AfterExecutionCount.ShouldBe(1);
                module.AfterExecutionAsyncCount.ShouldBe(1);
                module.FinallyCount.ShouldBe(1);
                module.FinallyAsyncCount.ShouldBe(1);
                module.ExecuteContextAsyncCount.ShouldBe(1);
                module.ExecuteInputAsyncCount.ShouldBe(2);
                outputs.ShouldBe(new[] { a, b });
            }

            [Test]
            public async Task CallsOverridesOnException()
            {
                // Given
                TestDocument a = new TestDocument("A");
                TestDocument b = new TestDocument("B");
                IExecutionContext context = new TestExecutionContext(a, b);
                TestModule module = new TestModule(null, null, true);

                // When
                await Should.ThrowAsync(module.ExecuteAsync(context), typeof(Exception));

                // Then
                module.BeforeExecutionCount.ShouldBe(1);
                module.BeforeExecutionAsyncCount.ShouldBe(1);
                module.AfterExecutionCount.ShouldBe(0);
                module.AfterExecutionAsyncCount.ShouldBe(0);
                module.FinallyCount.ShouldBe(1);
                module.FinallyAsyncCount.ShouldBe(1);
            }

            [Test]
            public async Task DoesNotCallExecuteInputOnContextOverride()
            {
                // Given
                TestDocument a = new TestDocument("A");
                TestDocument b = new TestDocument("B");
                IExecutionContext context = new TestExecutionContext(a, b);
                TestDocument x = new TestDocument("X");
                TestDocument y = new TestDocument("Y");
                TestModule module = new TestModule(new[] { x, y }, null, false);

                // When
                IEnumerable<IDocument> outputs = await module.ExecuteAsync(context);

                // Then
                module.BeforeExecutionCount.ShouldBe(1);
                module.BeforeExecutionAsyncCount.ShouldBe(1);
                module.AfterExecutionCount.ShouldBe(1);
                module.AfterExecutionAsyncCount.ShouldBe(1);
                module.FinallyCount.ShouldBe(1);
                module.FinallyAsyncCount.ShouldBe(1);
                module.ExecuteContextAsyncCount.ShouldBe(1);
                module.ExecuteInputAsyncCount.ShouldBe(0);
                outputs.ShouldBe(new[] { x, y });
            }

            [Test]
            public async Task AfterExecutionReplacesOutputs()
            {
                // Given
                TestDocument a = new TestDocument("A");
                TestDocument b = new TestDocument("B");
                IExecutionContext context = new TestExecutionContext(a, b);
                TestDocument x = new TestDocument("X");
                TestDocument y = new TestDocument("Y");
                TestModule module = new TestModule(null, new[] { x, y }, false);

                // When
                IEnumerable<IDocument> outputs = await module.ExecuteAsync(context);

                // Then
                module.BeforeExecutionCount.ShouldBe(1);
                module.BeforeExecutionAsyncCount.ShouldBe(1);
                module.AfterExecutionCount.ShouldBe(1);
                module.AfterExecutionAsyncCount.ShouldBe(1);
                module.FinallyCount.ShouldBe(1);
                module.FinallyAsyncCount.ShouldBe(1);
                module.ExecuteContextAsyncCount.ShouldBe(1);
                module.ExecuteInputAsyncCount.ShouldBe(2);
                outputs.ShouldBe(new[] { x, y });
            }
        }

        private class TestModule : Module
        {
            private readonly IEnumerable<IDocument> _outputs;
            private readonly IEnumerable<IDocument> _afterExecutionOutputs;
            private readonly bool _throwExecution;

            public int BeforeExecutionCount { get; set; }
            public int BeforeExecutionAsyncCount { get; set; }
            public int AfterExecutionCount { get; set; }
            public int AfterExecutionAsyncCount { get; set; }
            public int FinallyCount { get; set; }
            public int FinallyAsyncCount { get; set; }
            public int ExecuteContextAsyncCount { get; set; }
            public int ExecuteInputAsyncCount { get; set; }

            public TestModule(IEnumerable<IDocument> outputs, IEnumerable<IDocument> afterExecutionOutputs, bool throwExecution)
            {
                _outputs = outputs;
                _afterExecutionOutputs = afterExecutionOutputs;
                _throwExecution = throwExecution;
            }

            protected override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
            {
                ExecuteContextAsyncCount++;
                if (_throwExecution)
                {
                    throw new Exception();
                }
                if (_outputs is object)
                {
                    return Task.FromResult(_outputs);
                }
                return base.ExecuteContextAsync(context);
            }

            protected override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
            {
                ExecuteInputAsyncCount++;
                return input.YieldAsync();
            }

            protected override void BeforeExecution(IExecutionContext context)
            {
                BeforeExecutionCount++;
            }

            protected override Task BeforeExecutionAsync(IExecutionContext context)
            {
                BeforeExecutionAsyncCount++;
                return Task.CompletedTask;
            }

            protected override void AfterExecution(IExecutionContext context, ExecutionOutputs outputs)
            {
                AfterExecutionCount++;
                if (_afterExecutionOutputs is object)
                {
                    outputs.Outputs = _afterExecutionOutputs;
                }
            }

            protected override Task AfterExecutionAsync(IExecutionContext context, ExecutionOutputs outputs)
            {
                AfterExecutionAsyncCount++;
                return Task.CompletedTask;
            }

            protected override void Finally(IExecutionContext context)
            {
                FinallyCount++;
            }

            protected override Task FinallyAsync(IExecutionContext context)
            {
                FinallyAsyncCount++;
                return Task.CompletedTask;
            }
        }
    }
}
