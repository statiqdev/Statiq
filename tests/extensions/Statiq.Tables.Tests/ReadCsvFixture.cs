using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Tables.Tests
{
    [TestFixture]
    public class ReadCsvFixture : BaseFixture
    {
        public class ExecuteTests : ReadCsvFixture
        {
            [Test]
            public async Task ReadsTable()
            {
                // Given
                TestDocument document = new TestDocument(GetTestFileStream("test.csv"));
                ReadCsv module = new ReadCsv();

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                IReadOnlyList<IReadOnlyList<string>> table = result.Get<IReadOnlyList<IReadOnlyList<string>>>(TablesKeys.Table);
                table.ShouldNotBeNull();
                table[0][2].ShouldBe("B");
                table[2][0].ShouldBe("2");
            }

            [Test]
            public async Task AlternateKey()
            {
                // Given
                TestDocument document = new TestDocument(GetTestFileStream("test.csv"));
                ReadCsv module = new ReadCsv().WithKey("Foo");

                // When
                TestDocument result = await ExecuteAsync(document, module).SingleAsync();

                // Then
                IReadOnlyList<IReadOnlyList<string>> table = result.Get<IReadOnlyList<IReadOnlyList<string>>>("Foo");
                table.ShouldNotBeNull();
                table[0][2].ShouldBe("B");
                table[2][0].ShouldBe("2");
            }
        }
    }
}