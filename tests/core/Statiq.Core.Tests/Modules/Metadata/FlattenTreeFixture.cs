using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class FlattenTreeFixture : BaseFixture
    {
        public class ExecuteTests : FlattenTreeFixture
        {
            [Test]
            public async Task FlattensDefaultKey()
            {
                // Given
                TestDocument input = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new TestDocument[]
                        {
                            new TestDocument("A"),
                            new TestDocument("B")
                        }
                    },
                    {
                        "Foo",
                        new TestDocument[]
                        {
                            new TestDocument("C"),
                            new TestDocument("D")
                        }
                    }
                };
                FlattenTree module = new FlattenTree();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "X", "A", "B" }, true);
            }

            [Test]
            public async Task FlattensAlternateKey()
            {
                // Given
                TestDocument input = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new TestDocument[]
                        {
                            new TestDocument("A"),
                            new TestDocument("B")
                        }
                    },
                    {
                        "Foo",
                        new TestDocument[]
                        {
                            new TestDocument("C"),
                            new TestDocument("D")
                        }
                    }
                };
                FlattenTree module = new FlattenTree("Foo");

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "X", "C", "D" }, true);
            }

            [Test]
            public async Task FlattensAllKeys()
            {
                // Given
                TestDocument input = new TestDocument("X")
                {
                    {
                        Keys.Children,
                        new TestDocument[]
                        {
                            new TestDocument("A"),
                            new TestDocument("B")
                        }
                    },
                    {
                        "Foo",
                        new TestDocument[]
                        {
                            new TestDocument("C"),
                            new TestDocument("D")
                        }
                    }
                };
                FlattenTree module = new FlattenTree(null);

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(input, module);

                // Then
                results.Select(x => x.Content).ShouldBe(new[] { "X", "A", "B", "C", "D" }, true);
            }
        }
    }
}
