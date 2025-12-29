using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Html;

namespace Statiq.Common.Tests
{
    [TestFixture]
    public class QualityFixture
    {
        [Test]
        public void FlatNamespace()
        {
            // Given, When
            string[] namespaces = typeof(IExecutionContext).Assembly.GetTypes()
                .Where(x =>
                    x.IsPublic // Eliminate compiler-generated attributes like Microsoft.CodeAnalysis.EmbeddedAttribute
                    && x.Name != nameof(HtmlKeys)) // Don't count the HtmlKeys file that was kept for backwards compat
                .Select(x => x.Namespace)
                .Distinct()
                .Where(x => x is object)
                .ToArray();

            // Then
            namespaces.ShouldBe(new[] { "Statiq.Common" });
        }
    }
}