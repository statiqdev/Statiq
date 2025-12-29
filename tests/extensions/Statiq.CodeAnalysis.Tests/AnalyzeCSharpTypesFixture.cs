using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpTypesFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpTypesFixture
        {
            [Test]
            public async Task ReturnsAllTypes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                        }

                        class Green
                        {
                            class Red
                            {
                            }
                        }

                        internal struct Yellow
                        {
                        }

                        enum Orange
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
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Foo", "Blue", "Green", "Red", "Yellow", "Orange" }, true);
            }

            [Test]
            public async Task ReturnsExtensionMethods()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                        }

                        public static class Green
                        {
                            public static void Ext(this Blue blue)
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
                    .Single(x => x["Name"].Equals("Blue"))
                    .Get<IEnumerable<IDocument>>("ExtensionMethods")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Ext" });
            }

            [Test]
            public async Task ReturnsExtensionMethodsForBaseClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Red
                        {
                        }

                        public class Blue : Red
                        {
                        }

                        public static class Green
                        {
                            public static void Ext(this Red red)
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
                    .Single(x => x["Name"].Equals("Blue"))
                    .Get<IEnumerable<IDocument>>("ExtensionMethods")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Ext" });
            }

            [Test]
            public async Task MemberTypesReturnsNestedTypes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            public class Blue
                            {
                            }

                            private struct Red
                            {
                            }

                            enum Yellow
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
                    .Single(x => x["Name"].Equals("Green"))
                    .Get<IEnumerable<IDocument>>("MemberTypes")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Blue", "Red", "Yellow" }, true);
            }

            [Test]
            public async Task FullNameContainsContainingType()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
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
                results
                    .Select(x => x["FullName"])
                    .ShouldBe(new[] { string.Empty, "Foo", "Green", "Green.Blue", "Red", "Yellow", "Bar" }, true);
            }

            [Test]
            public async Task DisplayNameContainsContainingType()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
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
                results
                    .Select(x => x["DisplayName"])
                    .ShouldBe(new[] { "global", "Foo", "Green", "Green.Blue", "Red", "Yellow", "Foo.Bar" }, true);
            }

            [Test]
            public async Task QualifiedNameContainsNamespaceAndContainingType()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
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
                results
                    .Select(x => x["QualifiedName"])
                    .ShouldBe(new[] { string.Empty, "Foo", "Foo.Green", "Foo.Green.Blue", "Foo.Red", "Foo.Bar.Yellow", "Foo.Bar" }, true);
            }

            [Test]
            public async Task ContainingNamespaceIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
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
                results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("ContainingNamespace")["Name"].ShouldBe("Foo");
                results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("ContainingNamespace")["Name"].ShouldBe("Foo");
                results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("ContainingNamespace")["Name"].ShouldBe("Foo");
                results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("ContainingNamespace")["Name"].ShouldBe("Bar");
            }

            [Test]
            public async Task ContainingTypeIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Green
                        {
                            class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
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
                results.Single(x => x["Name"].Equals("Green")).Get<IDocument>("ContainingType").ShouldBeNull();
                results.Single(x => x["Name"].Equals("Blue")).Get<IDocument>("ContainingType")["Name"].ShouldBe("Green");
                results.Single(x => x["Name"].Equals("Red")).Get<IDocument>("ContainingType").ShouldBeNull();
                results.Single(x => x["Name"].Equals("Yellow")).Get<IDocument>("ContainingType").ShouldBeNull();
            }

            [Test]
            public async Task KindIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
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
                results.Single(x => x["Name"].Equals("Green"))["Kind"].ShouldBe("NamedType");
                results.Single(x => x["Name"].Equals("Blue"))["Kind"].ShouldBe("NamedType");
                results.Single(x => x["Name"].Equals("Red"))["Kind"].ShouldBe("NamedType");
                results.Single(x => x["Name"].Equals("Yellow"))["Kind"].ShouldBe("NamedType");
            }

            [Test]
            public async Task SpecificKindIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Green
                        {
                            private class Blue
                            {
                            }
                        }

                        struct Red
                        {
                        }
                    }

                    namespace Foo.Bar
                    {
                        enum Yellow
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
                results.Single(x => x["Name"].Equals("Green"))["SpecificKind"].ShouldBe("Class");
                results.Single(x => x["Name"].Equals("Blue"))["SpecificKind"].ShouldBe("Class");
                results.Single(x => x["Name"].Equals("Red"))["SpecificKind"].ShouldBe("Struct");
                results.Single(x => x["Name"].Equals("Yellow"))["SpecificKind"].ShouldBe("Enum");
            }

            [Test]
            public async Task BaseTypeIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Red
                        {
                        }

                        public class Green : Red
                        {
                        }

                        struct Blue
                        {
                        }

                        interface Yellow
                        {
                        }

                        interface Purple : Yellow
                        {
                        }

                        enum Orange
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
                results.Single(x => x["Name"].Equals("Red")).GetDocumentList("BaseTypes").First()["Name"].ShouldBe("Object");
                results.Single(x => x["Name"].Equals("Green")).GetDocumentList("BaseTypes").First()["Name"].ShouldBe("Red");
                results.Single(x => x["Name"].Equals("Blue")).GetDocumentList("BaseTypes").First()["Name"].ShouldBe("ValueType");
                results.Single(x => x["Name"].Equals("Yellow")).GetDocumentList("BaseTypes").ShouldBeEmpty();
                results.Single(x => x["Name"].Equals("Purple")).GetDocumentList("BaseTypes").ShouldBeEmpty();
                results.Single(x => x["Name"].Equals("Orange")).GetDocumentList("BaseTypes").First()["Name"].ShouldBe("Enum");
            }

            [Test]
            public async Task MembersReturnsAllMembersExceptConstructors()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public Blue()
                            {
                            }

                            void Green()
                            {
                            }

                            int Red { get; }

                            string _yellow;

                            event ChangedEventHandler Changed;
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                GetResult(results, "Blue")
                    .Get<IReadOnlyList<IDocument>>("Members")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Green", "Red", "_yellow", "Changed", "ToString", "Equals", "Equals", "ReferenceEquals", "GetHashCode", "GetType", "Finalize", "MemberwiseClone" }, true);
            }

            [Test]
            public async Task ConstructorsIsPopulated()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public Blue()
                            {
                            }

                            protected Blue(int x)
                            {
                            }

                            void Green()
                            {
                            }

                            int Red { get; }

                            string _yellow;

                            event ChangedEventHandler Changed;
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                GetResult(results, "Blue").Get<IReadOnlyList<IDocument>>("Constructors").Count.ShouldBe(2);
            }

            [Test]
            public async Task DestinationPathIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red
                        {
                        }

                        enum Green
                        {
                        }

                        namespace Bar
                        {
                            struct Blue
                            {
                            }
                        }
                    }

                    class Yellow
                    {
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results
                    .Where(x => x["Kind"].Equals("NamedType"))
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(new[] { "Foo/Green/index.html", "Foo.Bar/Blue/index.html", "global/Yellow/index.html", "Foo/Red/index.html" }, true);
            }

            [Test]
            public async Task GetDocumentForExternalBaseType()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red
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
                GetResult(results, "Red").GetDocumentList("BaseTypes").First()["Name"].ShouldBe("Object");
            }

            [Test]
            public async Task GetDocumentsForExternalInterfaces()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red : IBlue, IFoo
                        {
                        }

                        interface IBlue
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
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Foo", "Red", "IBlue" }, true);
                GetResult(results, "Red")
                    .Get<IEnumerable<IDocument>>("AllInterfaces")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "IBlue", "IFoo" }, true);
            }

            [Test]
            public async Task GetDerivedTypes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red : IBlue, IFoo
                        {
                        }

                        class Blue : Red, IBlue
                        {
                        }

                        class Green : Blue
                        {
                        }

                        class Yellow : Blue
                        {
                        }

                        interface IBlue
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
                GetResult(results, "Red")
                    .Get<IEnumerable<IDocument>>("DerivedTypes")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Blue" });
                GetResult(results, "Blue")
                    .Get<IEnumerable<IDocument>>("DerivedTypes")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Green", "Yellow" }, true);
                GetResult(results, "Green").Get<IEnumerable<IDocument>>("DerivedTypes").ShouldBeEmpty();
                GetResult(results, "Yellow").Get<IEnumerable<IDocument>>("DerivedTypes").ShouldBeEmpty();
                GetResult(results, "IBlue").Get<IEnumerable<IDocument>>("DerivedTypes").ShouldBeEmpty();
            }

            [Test]
            public async Task GetTypeParams()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red<T>
                        {
                        }

                        interface IBlue<TKey, TValue>
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
                GetResult(results, "Red")
                    .Get<IEnumerable<IDocument>>("TypeParameters")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "T" }, true);
                GetResult(results, "IBlue")
                    .Get<IEnumerable<IDocument>>("TypeParameters")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "TKey", "TValue" }, true);
            }

            [Test]
            public async Task TypeParamReferencesClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red<T>
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
                GetResult(results, "Red")
                    .Get<IEnumerable<IDocument>>("TypeParameters")
                    .First()
                    .Get<IDocument>("DeclaringType")["Name"]
                    .ShouldBe("Red");
            }

            [Test]
            public async Task TypesExcludedByPredicate()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                        }

                        class Green
                        {
                            class Red
                            {
                            }
                        }

                        internal struct Yellow
                        {
                        }

                        enum Orange
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().WhereSymbol(x => x is INamedTypeSymbol && ((INamedTypeSymbol)x).TypeKind == TypeKind.Class);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { "Blue", "Green", "Red" }, true);
            }

            [Test]
            public async Task RestrictedToNamedTypes()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public void Foo()
                            {
                            }
                        }

                        public interface Red
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().WithNamedTypes();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { "Blue", "Red" }, true);
            }

            [Test]
            public async Task RestrictedToNamedTypesWithPredicate()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            public void Foo()
                            {
                            }
                        }

                        public interface Red
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().WithNamedTypes(x => x.TypeKind == TypeKind.Class);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { "Blue" }, true);
            }

            [Test]
            public async Task NestedTypesGetDifferentSymbolIds()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            class Red
                            {
                            }
                        }

                        class Green
                        {
                            class Red
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
                results
                    .Select(x => x.GetString(CodeAnalysisKeys.SymbolId))
                    .ShouldBe(new[] { string.Empty, "Foo", "Blue", "Green", "Green.Red", "Blue.Red" }, true);
            }
        }
    }
}