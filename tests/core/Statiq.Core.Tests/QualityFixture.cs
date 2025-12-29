using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Shouldly;

namespace Statiq.Core.Tests
{
    [TestFixture]
    public class QualityFixture
    {
        [Test]
        public void FlatNamespace()
        {
            // Given, When
            string[] namespaces = typeof(ExecutionContext).Assembly.GetTypes()
                .Where(x => x.IsPublic) // Eliminate compiler-generated attributes like Microsoft.CodeAnalysis.EmbeddedAttribute
                .Select(x => x.Namespace)
                .Distinct()
                .Where(x => x is object)
                .ToArray();

            // Then
            namespaces.ShouldBe(new[] { "Statiq.Core" });
        }
    }
}