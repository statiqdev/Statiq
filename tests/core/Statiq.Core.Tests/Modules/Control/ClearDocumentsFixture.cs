using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ClearDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : ClearDocumentsFixture
        {
            [Test]
            public async Task ClearsDocuments()
            {
                // Given
                TestDocument a = new TestDocument();
                TestDocument b = new TestDocument();
                ClearDocuments clearDocuments = new ClearDocuments();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, clearDocuments);

                // Then
                results.ShouldBeEmpty();
            }
        }
    }
}
