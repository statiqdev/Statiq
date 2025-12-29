using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpMethodsFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpMethodsFixture
        {
            [Test]
            public async Task ClassMembersContainsMethods()
            {
                try
                {
                    // Given
                    const string code = @"
                        namespace Foo
                        {
                            public class Blue
                            {
                                void Green()
                                {
                                }

                                void Red()
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
                    GetResult(results, "Blue")
                        .Get<IReadOnlyList<IDocument>>("Members")
                        .Select(x => x["Name"])
                        .ShouldBe(new[] { "Green", "Red", "ToString", "Equals", "Equals", "ReferenceEquals", "GetHashCode", "GetType", "Finalize", "MemberwiseClone" }, true);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine("Loader exceptions: " + Environment.NewLine + string.Join(Environment.NewLine, ex.LoaderExceptions.Select(x => x.Message)));
                    throw;
                }
            }

            [Test]
            public async Task ClassOperatorsContainsOperators()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
	                        public static Blue operator +(Blue a1, Blue a2)
	                        {
		                        return null;
	                        }
                            public static explicit operator string(Blue b)
	                        {
	                            return null;
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
                GetResult(results, "Blue")
                    .Get<IReadOnlyList<IDocument>>("Operators")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "op_Addition", "op_Explicit" }, true);
            }

            [Test]
            public async Task ContainingTypeIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            void Green()
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
                GetMember(results, "Blue", "Green").Get<IDocument>("ContainingType")["Name"].ShouldBe("Blue");
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
                            void X()
                            {
                            }
                        }
                    }

                    class Yellow
                    {
                        void Y<T>()
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
                    .Where(x => x["Kind"].Equals("Method"))
                    .Select(x => x.Destination.FullPath)
                    .ShouldBe(new[] { "global/Yellow/66F23CDD.html", "Foo/Red/A94FD382.html" }, true);
            }

            [Test]
            public async Task DisplayNameIsCorrect()
            {
                // Given
                const string code = @"
                    class Yellow
                    {
                        public void X()
                        {
                        }

                        void Y<T>(T a, int b)
                        {
                        }

                        void Z(bool a)
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
                GetMember(results, "Yellow", "X")["DisplayName"].ShouldBe("X()");
                GetMember(results, "Yellow", "Y")["DisplayName"].ShouldBe("Y<T>(T, int)");
                GetMember(results, "Yellow", "Z")["DisplayName"].ShouldBe("Z(bool)");
            }

            [Test]
            public async Task ReturnTypeIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            int Green()
                            {
                                return 0;
                            }

                            Red GetRed()
                            {
                                return new Red();
                            }

                            TFoo Bar<TFoo>()
                            {
                                return default(TFoo);
                            }
                        }

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
                GetMember(results, "Blue", "Green").Get<IDocument>("ReturnType")["DisplayName"].ShouldBe("int");
                GetMember(results, "Blue", "GetRed").Get<IDocument>("ReturnType")["DisplayName"].ShouldBe("Red");
                GetMember(results, "Blue", "Bar").Get<IDocument>("ReturnType")["DisplayName"].ShouldBe("TFoo");
            }

            [Test]
            public async Task ReturnTypeParamReferencesClass()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        class Red<T>
                        {
                            public T Blue()
                            {
                                return default(T);
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
                GetMember(results, "Red", "Blue").Get<IDocument>("ReturnType").Get<IDocument>("DeclaringType")["Name"].ShouldBe("Red");
            }

            [Test]
            public async Task ClassMemberExcludedByPredicate()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                            void Green()
                            {
                            }

                            void Red()
                            {
                            }
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().WhereSymbol(x => x.Name != "Green");

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x["Name"]).ShouldBe(new[] { string.Empty, "Foo", "Blue", "Red" }, true);
                GetResult(results, "Blue")
                    .Get<IReadOnlyList<IDocument>>("Members")
                    .Where(x => x.GetBool("IsResult"))
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Red" });
                GetResult(results, "Blue")
                    .Get<IReadOnlyList<IDocument>>("Members")
                    .Select(x => x["Name"])
                    .ShouldBe(new[] { "Red", "Green", "ToString", "Equals", "Equals", "ReferenceEquals", "GetHashCode", "GetType", "Finalize", "MemberwiseClone" }, true);
            }

            [Test]
            public async Task ParameterType()
            {
                // Given
                const string code = @"
                    class Yellow
                    {
                        public void X(int z)
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
                ((IDocument)GetParameter(results, "Yellow", "X", "z")["Type"])["Name"].ShouldBe("Int32");
            }

            [Test]
            public async Task ParameterParamsType()
            {
                // Given
                const string code = @"
                    class Yellow
                    {
                        public void X(params int[] z)
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
                IDocument x = GetParameter(results, "Yellow", "X", "z");
                IDocument y = (IDocument)x["Type"];
                object z = y["Name"];
                ((IDocument)GetParameter(results, "Yellow", "X", "z")["Type"])["Name"].ShouldBe("int[]");
            }

            [Test]
            public async Task ImplementsIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public interface IRed
                        {
                            void Green();
                        }
                        
                        public class Blue : IRed
                        {
                            public void Green()
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
                IReadOnlyList<IDocument> implements = GetMember(results, "Blue", "Green").Get<IReadOnlyList<IDocument>>("Implements");
                IDocument red = implements.ShouldHaveSingleItem();
                red.GetString("Name").ShouldBe("Green");
                red.GetDocument("ContainingType").GetString("Name").ShouldBe("IRed");
            }

            [Test]
            public async Task MultipleImplementsIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public interface IRed
                        {
                            void Green();
                        }
                        
                        public interface IGreen
                        {
                            void Green();
                        }
                        
                        public class Blue : IRed, IGreen
                        {
                            public void Green()
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
                IReadOnlyList<IDocument> implements = GetMember(results, "Blue", "Green").Get<IReadOnlyList<IDocument>>("Implements");
                implements
                    .Select(x => x.GetDocument("ContainingType").GetString("Name"))
                    .ShouldBe(new[] { "IRed", "IGreen" }, true);
            }

            [Test]
            public async Task NestedImplementsIsCorrect()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public interface IRed
                        {
                            void Green();
                        }
                        
                        public interface IGreen : IRed
                        {
                        }
                        
                        public class Blue : IGreen
                        {
                            public void Green()
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
                IReadOnlyList<IDocument> implements = GetMember(results, "Blue", "Green").Get<IReadOnlyList<IDocument>>("Implements");
                IDocument red = implements.ShouldHaveSingleItem();
                red.GetString("Name").ShouldBe("Green");
                red.GetDocument("ContainingType").GetString("Name").ShouldBe("IRed");
            }
        }
    }
}