using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpNamespacesFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpNamespacesFixture
        {
            [Test]
            public async Task GetsTopLevelNamespaces()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Foo", "Bar" }, true);
            }

            [Test]
            public async Task DoesNotIncludeEmptyNamespace()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Bar
                    {
                        public class Red
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Bar", "Red" }, true);
            }

            [Test]
            public async Task DoesNotIncludeEmptyNestedNamespaces()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        namespace Fizz
                        {
                        }
                    }

                    namespace Bar
                    {
                        namespace Buzz
                        {
                            public class Red
                            {
                            }
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Bar", "Buzz", "Red" }, true);
            }

            [Test]
            public async Task TopLevelNamespaceContainsDirectlyNestedNamespaces()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Foo.Baz
                    {
                    }

                    namespace Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Foo", "Baz", "Bar" }, true);
                results
                    .Single(x => x["Name"].Equals(string.Empty))
                    .Get<IEnumerable<IDocument>>("MemberNamespaces")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Foo", "Bar" }, true);
            }

            [Test]
            public async Task NestedNamespaceContainsDirectlyNestedNamespaces()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Foo.Baz
                    {
                    }

                    namespace Foo.Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Foo", "Baz", "Bar" }, true);
                results
                    .Single(x => x["Name"].Equals("Foo"))
                    .Get<IEnumerable<IDocument>>("MemberNamespaces")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Baz", "Bar" }, true);
            }

            [Test]
            public async Task FullNameDoesNotContainFullHierarchy()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Foo.Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["FullName"]).ShouldBe(new[] { string.Empty, "Foo", "Bar" }, true);
            }

            [Test]
            public async Task QualifiedNameContainsFullHierarchy()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Foo.Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["QualifiedName"]).ShouldBe(new[] { string.Empty, "Foo", "Foo.Bar" }, true);
            }

            [Test]
            public async Task DisplayNameContainsFullHierarchy()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Foo.Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["DisplayName"]).ShouldBe(new[] { "global", "Foo", "Foo.Bar" }, true);
            }

            [Test]
            public async Task NamespaceKindIsNamespace()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Foo.Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Kind"]).ShouldBe(new[] { "Namespace", "Namespace", "Namespace" }, true);
            }

            [Test]
            public async Task NestedNamespacesReferenceParents()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                    }

                    namespace Foo.Bar
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results
                    .Single(x => x["Name"].Equals("Bar"))
                    .Get<IDocument>("ContainingNamespace")["Name"]
                    .ShouldBe("Foo");
                results
                    .Single(x => x["Name"].Equals("Foo"))
                    .Get<IDocument>("ContainingNamespace")["Name"]
                    .ShouldBe(string.Empty);
            }

            [Test]
            public async Task NamespacesContainTypes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        class Blue
                        {
                        }

                        class Green
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results
                    .Single(x => x["Name"].Equals("Foo"))
                    .Get<IEnumerable<IDocument>>("MemberTypes")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Red" }, true);
                results
                    .Single(x => x["Name"].Equals("Bar"))
                    .Get<IEnumerable<IDocument>>("MemberTypes")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Blue", "Green" }, true);
            }

            [Test]
            public async Task NamespacesDoNotContainNestedTypes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Blue
                        {
                            class Green
                            {
                            }
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results
                    .Single(x => x["Name"].Equals("Foo"))
                    .Get<IEnumerable<IDocument>>("MemberTypes")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Blue" }, true);
            }

            [Test]
            public async Task DestinationPathIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        namespace Bar
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().IncludeEmptyNamespaces();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results
                    .Where(x => x["Kind"].Equals("Namespace"))
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(new[] { "global/index.html", "Foo/index.html", "Foo.Bar/index.html" }, true);
            }
        }
    }
}