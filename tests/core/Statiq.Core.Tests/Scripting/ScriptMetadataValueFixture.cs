using System.Text;
using System.Threading;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Scripting
{
    // ScriptMetadataValue is in Statiq.Common but the tests need to be in Statiq.Core.Tests since
    // functionality relies on a valid instance of ScriptHelper
    [TestFixture]
    public class ScriptMetadataValueFixture : BaseFixture
    {
        public class GetTests : ScriptMetadataValueFixture
        {
            [TestCase("=> $\"ABC {1+2} XYZ\"")]
            [TestCase("=> return $\"ABC {1+2} XYZ\";")]
            [TestCase("=> { int x = 1 + 2; return $\"ABC {x} XYZ\"; }")]
            [TestCase("=> int x = 1 + 2; return $\"ABC {x} XYZ\";")]
            [TestCase("  => $\"ABC {1+2} XYZ\"")]
            public void EvaluatesCachedScriptMetadata(string value)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", value, context, out ScriptMetadataValue scriptMetadataValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", scriptMetadataValue }
                };

                // When
                string result = document.GetString("Foo");

                // Then
                result.ShouldBe("ABC 3 XYZ");
            }

            [TestCase("-> $\"ABC {1+2} XYZ\"")]
            [TestCase("-> return $\"ABC {1+2} XYZ\";")]
            [TestCase("-> { int x = 1 + 2; return $\"ABC {x} XYZ\"; }")]
            [TestCase("-> int x = 1 + 2; return $\"ABC {x} XYZ\";")]
            [TestCase("  -> $\"ABC {1+2} XYZ\"")]
            public void EvaluatesUncachedScriptMetadata(string value)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", value, context, out ScriptMetadataValue scriptMetadataValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", scriptMetadataValue }
                };

                // When
                string result = document.GetString("Foo");

                // Then
                result.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesIntScriptResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> 1 + 2", context, out ScriptMetadataValue scriptMetadataValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", scriptMetadataValue }
                };

                // When
                object result = document.Get("Foo");

                // Then
                result.ShouldBe(3);
            }

            [Test]
            public void WithoutSettings()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> GetString(\"A\")", context, out ScriptMetadataValue fooScriptMetadataValue);
                ScriptMetadataValue.TryGetScriptMetadataValue("Bar", "=> WithoutSettings().GetString(\"A\")", context, out ScriptMetadataValue barScriptMetadataValue);
                Settings settings = new Settings
                {
                    { "A", "a" }
                };
                TestDocument document = new TestDocument(settings, null, null, null)
                {
                    { "Foo", fooScriptMetadataValue },
                    { "Bar", barScriptMetadataValue }
                };

                // When
                object fooResult = document.Get("Foo");
                object barResult = document.Get("Bar");

                // Then
                fooResult.ShouldBe("a");
                barResult.ShouldBeNull();
            }

            [Test]
            public void ExcludesAllScriptEvaluation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Bar", "=> 1 + 2", context, out ScriptMetadataValue fooValue);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> 3 + 4", context, out ScriptMetadataValue barValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", fooValue },
                    { "Bar", barValue },
                    { Keys.ExcludeFromEvaluation, true }
                };

                // When
                object fooResult = document.Get("Foo");
                object barResult = document.Get("bar");

                // Then
                fooResult.ShouldBe("=> 1 + 2");
                barResult.ShouldBe("=> 3 + 4");
            }

            [Test]
            public void DoesNotExcludeAllScriptEvaluation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Bar", "=> 1 + 2", context, out ScriptMetadataValue fooValue);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> 3 + 4", context, out ScriptMetadataValue barValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", fooValue },
                    { "Bar", barValue },
                    { Keys.ExcludeFromEvaluation, false }
                };

                // When
                object fooResult = document.Get("Foo");
                object barResult = document.Get("bar");

                // Then
                fooResult.ShouldBe(3);
                barResult.ShouldBe(7);
            }

            [Test]
            public void ExcludesSingleValueFromScriptEvaluation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> 1 + 2", context, out ScriptMetadataValue fooValue);
                ScriptMetadataValue.TryGetScriptMetadataValue("Bar", "=> 3 + 4", context, out ScriptMetadataValue barValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", fooValue },
                    { "Bar", barValue },
                    { Keys.ExcludeFromEvaluation, new string[] { "foo" } }
                };

                // When
                object fooResult = document.Get("Foo");
                object barResult = document.Get("bar");

                // Then
                fooResult.ShouldBe("=> 1 + 2");
                barResult.ShouldBe(7);
            }

            [Test]
            public void ExcludesAtomicValueFromScriptEvaluation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> 1 + 2", context, out ScriptMetadataValue fooValue);
                ScriptMetadataValue.TryGetScriptMetadataValue("Bar", "=> 3 + 4", context, out ScriptMetadataValue barValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", fooValue },
                    { "Bar", barValue },
                    { Keys.ExcludeFromEvaluation, "foo" }
                };

                // When
                object fooResult = document.Get("Foo");
                object barResult = document.Get("bar");

                // Then
                fooResult.ShouldBe("=> 1 + 2");
                barResult.ShouldBe(7);
            }

            [Test]
            public void ExcludesMultipleValuesFromScriptEvaluation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> 1 + 2", context, out ScriptMetadataValue fooValue);
                ScriptMetadataValue.TryGetScriptMetadataValue("Bar", "=> 3 + 4", context, out ScriptMetadataValue barValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", fooValue },
                    { "Bar", barValue },
                    { Keys.ExcludeFromEvaluation, new string[] { "foo", "Bar" } }
                };

                // When
                object fooResult = document.Get("Foo");
                object barResult = document.Get("bar");

                // Then
                fooResult.ShouldBe("=> 1 + 2");
                barResult.ShouldBe("=> 3 + 4");
            }

            [Test]
            public void DoesNotExcludeForInvalidExclusionValue()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Bar", "=> 1 + 2", context, out ScriptMetadataValue fooValue);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> 3 + 4", context, out ScriptMetadataValue barValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", fooValue },
                    { "Bar", barValue },
                    { Keys.ExcludeFromEvaluation, 5 }
                };

                // When
                object fooResult = document.Get("Foo");
                object barResult = document.Get("bar");

                // Then
                fooResult.ShouldBe(3);
                barResult.ShouldBe(7);
            }

            [Test]
            public void ShouldCacheScriptResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "=> DateTime.Now.ToString()", context, out ScriptMetadataValue scriptMetadataValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", scriptMetadataValue }
                };

                // When
                string result1 = document.GetString("Foo");
                Thread.Sleep(100);
                string result2 = document.GetString("Foo");

                // Then
                result1.ShouldBe(result2);
            }

            [Test]
            public void ShouldNotCacheScriptResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                ScriptMetadataValue.TryGetScriptMetadataValue("Foo", "-> DateTime.Now.Ticks.ToString()", context, out ScriptMetadataValue scriptMetadataValue);
                TestDocument document = new TestDocument
                {
                    { "Foo", scriptMetadataValue }
                };

                // When
                string result1 = document.GetString("Foo");
                Thread.Sleep(100);
                string result2 = document.GetString("Foo");

                // Then
                result1.ShouldNotBe(result2);
            }
        }

        public class TryGetMetadataValueTests : ScriptMetadataValueFixture
        {
            [Test]
            public void ReturnsFalseIfNull()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetScriptMetadataValue(string.Empty, null, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForEmptyString()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetScriptMetadataValue(string.Empty, string.Empty, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForNonString()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetScriptMetadataValue(string.Empty, 1234, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            // To test values that can convert to a string
            [Test]
            public void ReturnsFalseForStringBuilder()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetScriptMetadataValue(string.Empty, new StringBuilder("=> $\"ABC {1+2} XYZ\""), context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [Test]
            public void ReturnsFalseForNormalString()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetScriptMetadataValue(string.Empty, "1234", context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeFalse();
                metadataValue.ShouldBeNull();
            }

            [TestCase("=> $\"ABC {1+2} XYZ\"")]
            [TestCase("=> return $\"ABC {1+2} XYZ\";")]
            [TestCase("=> { int x = 1 + 2; return $\"ABC {x} XYZ\"; }")]
            [TestCase("=> int x = 1 + 2; return $\"ABC {x} XYZ\";")]
            [TestCase("  => $\"ABC {1+2} XYZ\"")]
            public void ReturnsTrueForValidScript(string value)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();

                // When
                bool result = ScriptMetadataValue.TryGetScriptMetadataValue(string.Empty, value, context, out ScriptMetadataValue metadataValue);

                // Then
                result.ShouldBeTrue();
                metadataValue.ShouldNotBeNull();
            }
        }
    }
}