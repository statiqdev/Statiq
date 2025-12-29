using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    [TestFixture]
    public class AnalyzeCSharpFixture : AnalyzeCSharpBaseFixture
    {
        public class ExecuteTests : AnalyzeCSharpFixture
        {
            [Test]
            public async Task SetsCompilation()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
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
                Compilation compilation = results.First()[CodeAnalysisKeys.Compilation] as Compilation;
                compilation.ShouldNotBeNull();
            }

            [Test]
            public async Task SetsCompilationAssenblyName()
            {
                // Given
                const string code = @"
                    namespace Foo
                    {
                        public class Blue
                        {
                        }
                    }
                ";
                TestDocument document = GetDocument(code);
                TestExecutionContext context = GetContext();
                IModule module = new AnalyzeCSharp().WithCompilationAssemblyName("fizz.buzz");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                Compilation compilation = results.First()[CodeAnalysisKeys.Compilation] as Compilation;
                compilation.AssemblyName.ShouldBe("fizz.buzz");
            }
        }
    }
}
