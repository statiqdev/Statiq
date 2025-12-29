using System;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Execution
{
    [TestFixture]
    public class PipelineCollectionFixture : BaseFixture
    {
        public class AddTests : PipelineCollectionFixture
        {
            [Test]
            public void ThrowsForDuplicateName()
            {
                // Given
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton<IFileCleaner>(new TestFileCleaner());
                Engine engine = new Engine(services);
                PipelineCollection pipelines = new PipelineCollection(engine);
                pipelines.Add("Foo");

                // When, Then
                Assert.Throws<ArgumentException>(() => pipelines.Add("Foo"));
            }

            [Test]
            public void ThrowsForDuplicateNameWithDifferentCase()
            {
                // Given
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton<IFileCleaner>(new TestFileCleaner());
                Engine engine = new Engine(services);
                PipelineCollection pipelines = new PipelineCollection(engine);
                pipelines.Add("Foo");

                // When, Then
                Assert.Throws<ArgumentException>(() => pipelines.Add("foo"));
            }
        }

        public class ContainsKeyTests : PipelineCollectionFixture
        {
            [Test]
            public void ReturnsTrueForDifferentCase()
            {
                // Given
                ServiceCollection services = new ServiceCollection();
                services.AddSingleton<IFileCleaner>(new TestFileCleaner());
                Engine engine = new Engine(services);
                PipelineCollection pipelines = new PipelineCollection(engine);
                pipelines.Add("Test");

                // When
                bool contains = pipelines.ContainsKey("test");

                // Then
                Assert.That(contains, Is.True);
            }
        }
    }
}