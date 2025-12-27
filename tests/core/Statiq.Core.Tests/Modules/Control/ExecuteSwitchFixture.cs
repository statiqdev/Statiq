using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ExecuteSwitchFixture : BaseFixture
    {
        public class ExecuteTests : ExecuteSwitchFixture
        {
            [Test]
            public async Task SwitchResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A") { AdditionalOutputs = 2 };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                CountModule d = new CountModule("D");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetContentStringAsync())).Case("1", b).Case("2", c).Default(d);

                // When
                await ExecuteAsync(a, switchModule);

                // Then
                Assert.That(a.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.ExecuteCount, Is.EqualTo(1));
                Assert.That(c.ExecuteCount, Is.EqualTo(1));
                Assert.That(d.ExecuteCount, Is.EqualTo(1));
            }

            [Test]
            public async Task SwitchNoCasesResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetContentStringAsync())).Default(b);

                // When
                await ExecuteAsync(a, switchModule, c);

                // Then
                Assert.That(a.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.InputCount, Is.EqualTo(3));
                Assert.That(b.OutputCount, Is.EqualTo(3));
                Assert.That(c.InputCount, Is.EqualTo(3));
            }

            [Test]
            public async Task MissingDefaultResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetContentStringAsync())).Case("1", b);

                // When
                await ExecuteAsync(a, switchModule, c);

                // Then
                Assert.That(a.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.InputCount, Is.EqualTo(1));
                Assert.That(b.OutputCount, Is.EqualTo(1));
                Assert.That(c.InputCount, Is.EqualTo(3));
            }

            [Test]
            public async Task ArrayInCaseResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                CountModule c = new CountModule("C");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetContentStringAsync())).Case(new string[] { "1", "2" }, b);

                // When
                await ExecuteAsync(a, switchModule, c);

                // Then
                Assert.That(a.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.ExecuteCount, Is.EqualTo(1));
                Assert.That(b.InputCount, Is.EqualTo(2));
                Assert.That(b.OutputCount, Is.EqualTo(2));
                Assert.That(c.InputCount, Is.EqualTo(3));
            }

            [Test]
            public async Task OmittingCasesAndDefaultResultsInCorrectCounts()
            {
                // Given
                CountModule a = new CountModule("A")
                {
                    AdditionalOutputs = 2,
                    EnsureInputDocument = true
                };
                CountModule b = new CountModule("B");
                ExecuteSwitch switchModule = new ExecuteSwitch(Config.FromDocument(async x => (object)await x.GetContentStringAsync()));

                // When
                await ExecuteAsync(a, switchModule, b);

                // Then
                Assert.That(b.InputCount, Is.EqualTo(3));
            }
        }
    }
}
