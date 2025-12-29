using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Extensibility
{
    [TestFixture]
    public class EvaluateScriptFixture : BaseFixture
    {
        public class ExecuteTests : EvaluateScriptFixture
        {
            [Test]
            public async Task ChangesContentWhenStringResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument("return 1 + 2;");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, context, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("3");
            }

            [Test]
            public async Task CanAccessMetadataAsAnObject()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument("return (int)Get(\"Foo\") + 2;")
                {
                    { "Foo", 5 }
                };
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, context, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("7");
            }

            [Test]
            public async Task CanAccessMetadataWithTypeConversion()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument("return GetInt(\"Foo\") + 2;")
                {
                    { "Foo", "5" }
                };
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, context, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("7");
            }

            [Test]
            public async Task MultipleStatements()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument("int x = 1 + 2; return x;");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, context, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("3");
            }

            [Test]
            public async Task DoesNotRequireReturn()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument("1 + 2");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, context, evaluateScript);

                // Then
                result.Single().Content.ShouldBe("3");
            }

            [Test]
            public async Task ReturnsEmptyContentForExpression()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                TestDocument document = new TestDocument("_ = 1 + 2;");
                EvaluateScript evaluateScript = new EvaluateScript();

                // When
                IReadOnlyList<TestDocument> result = await ExecuteAsync(document, context, evaluateScript);

                // Then
                result.Single().Content.ShouldBeEmpty();
            }
        }
    }
}
