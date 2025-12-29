using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Xmp.Tests
{
    [TestFixture]
    public class ReadXmpFixture : BaseFixture
    {
        public class ExecuteTests : ReadXmpFixture
        {
            [Test]
            public async Task ReadMetadata()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"));
                ReadXmp directoryMetadata = new ReadXmp()
                    .WithMetadata("xmpRights:UsageTerms", "Copyright");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(documents, directoryMetadata);

                // Then
                results.Single()["Copyright"]
                    .ShouldBe("This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.");
            }

            [Test]
            public async Task SkipMissingMandatory()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "RomantiqueInitials.ttf"));
                ReadXmp directoryMetadata = new ReadXmp(skipElementOnMissingMandatoryData: true)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);
                TestExecutionContext context = new TestExecutionContext(documents);
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.Error;

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, directoryMetadata);

                // Then
                results.Count.ShouldBe(1);
            }

            [Test]
            public async Task DontSkipMissingMandatory()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"),
                    Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "RomantiqueInitials.ttf"));
                ReadXmp directoryMetadata = new ReadXmp(skipElementOnMissingMandatoryData: false)
                    .WithMetadata("xmpRights:UsageTerms", "Copyright", true);
                TestExecutionContext context = new TestExecutionContext(documents);
                context.TestLoggerProvider.ThrowLogLevel = LogLevel.Error;

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, directoryMetadata);

                // Then
                results.Count.ShouldBe(2);
            }

            [Test]
            public async Task UsingNonDefaultNamespace()
            {
                // Given
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo("en");
                IReadOnlyList<TestDocument> documents = GetDocumentsFromSources(Path.Combine(TestContext.CurrentContext.TestDirectory, "Samples", "Flamme.png"));
                ReadXmp directoryMetadata = new ReadXmp()
                    .WithNamespace("http://ns.adobe.com/xap/1.0/rights/", "bla")
                    .WithMetadata("bla:UsageTerms", "Copyright");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(documents, directoryMetadata);

                // Then
                results.Single()["Copyright"]
                    .ShouldBe("This work is licensed under a <a rel=\"license\" href=\"http://creativecommons.org/licenses/by-sa/4.0/\">Creative Commons Attribution-ShareAlike 4.0 International License</a>.");
            }

            private IReadOnlyList<TestDocument> GetDocumentsFromSources(params string[] pathArray) =>
                pathArray.Select(x => new TestDocument(x, (NormalizedPath)null, File.OpenRead(x))).ToList();
        }
    }
}