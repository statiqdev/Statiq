using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Modules
{
    [TestFixture]
    public class ParallelModuleFixture : BaseFixture
    {
        public class ExecuteTests : ParallelModuleFixture
        {
            [Test]
            public async Task OutputsContent()
            {
                // Given
                TestDocument[] inputs = new TestDocument[]
                {
                    new TestDocument
                    {
                        { "Outputs", new string[] { "A", "B" } }
                    },
                    new TestDocument
                    {
                        { "Outputs", new string[] { "C" } }
                    }
                };
                TestModule module = new TestModule();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(inputs, module);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "A", "B", "C" }, true);
            }

            [Test]
            public async Task IgnoresNullResult()
            {
                // Given
                TestDocument[] inputs = new TestDocument[]
                {
                    new TestDocument
                    {
                        { "Outputs", new string[] { "A", "B" } }
                    },
                    new TestDocument()
                };
                TestModule module = new TestModule();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(inputs, module);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "A", "B" }, true);
            }
        }

        private class TestModule : ParallelSyncModule
        {
            protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
            {
                IReadOnlyList<string> contents = input.GetList<string>("Outputs");
                if (contents is null)
                {
                    return null;
                }
                List<IDocument> outputs = new List<IDocument>();
                foreach (string content in contents)
                {
                    outputs.Add(input.Clone(context.GetContentProvider(content)));
                }
                return outputs;
            }
        }
    }
}
