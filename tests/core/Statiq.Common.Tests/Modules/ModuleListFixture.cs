using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Modules
{
    [TestFixture]
    public class ModuleListFixture : BaseFixture
    {
        public class AddTests : ModuleListFixture
        {
            [Test]
            public void AddModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add(count);

                // Then
                list.ShouldBe(new[] { count });
            }
        }

        public class ContainsTests : ModuleListFixture
        {
            [Test]
            public void ContainsModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");

                // When
                list.Add(count);

                // Then
                Assert.That(list.Contains(count), Is.True);
            }

            [Test]
            public void DoesNotContainModule()
            {
                // Given
                ModuleList list = new ModuleList();
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");

                // When
                list.Add(count);

                // Then
                Assert.That(list.Contains(count2), Is.False);
            }
        }

        public class RemoveTests : ModuleListFixture
        {
            [Test]
            public void RemovesModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count, count2);

                // When
                bool result = list.Remove(count);

                // Then
                Assert.That(result, Is.True);
                Assert.That(list, Is.EqualTo(new[] { count2 }));
            }

            [Test]
            public void ReturnsFalseWhenNotRemoved()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count2);

                // When
                bool result = list.Remove(count);

                // Then
                Assert.That(result, Is.False);
                Assert.That(list, Is.EqualTo(new[] { count2 }));
            }
        }

        public class IndexOfTests : ModuleListFixture
        {
            [Test]
            public void ReturnsIndex()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count, count2);

                // When
                int index = list.IndexOf(count2);

                // Then
                Assert.That(index, Is.EqualTo(1));
            }

            [Test]
            public void ReturnsNegativeIndexWhenNotFound()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count);

                // When
                int index = list.IndexOf(count2);

                // Then
                Assert.That(index, Is.LessThan(0));
            }
        }

        public class InsertTests : ModuleListFixture
        {
            [Test]
            public void InsertsModule()
            {
                // Given
                CountModule count = new CountModule("A");
                CountModule count2 = new CountModule("B");
                ModuleList list = new ModuleList(count2);

                // When
                list.Insert(0, count);

                // Then
                Assert.That(list, Is.EqualTo(new[] { count, count2 }));
            }
        }
        public class InsertAfterFirstTests : ModuleListFixture
        {
            [Test]
            public void InsertAfterFirst()
            {
                // Given
                ModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertAfterFirst<RedModule>(new CountModule("foo"));

                // Then
                Assert.That(collection[0].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[1].GetType(), Is.EqualTo(typeof(CountModule)));
                Assert.That(collection[2].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[3].GetType(), Is.EqualTo(typeof(GreenModule)));
            }
        }

        public class InsertBeforeFirstTests : ModuleListFixture
        {
            [Test]
            public void InsertBeforeFirst()
            {
                // Given
                ModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertBeforeFirst<RedModule>(new CountModule("foo"));

                // Then
                Assert.That(collection[0].GetType(), Is.EqualTo(typeof(CountModule)));
                Assert.That(collection[1].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[2].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[3].GetType(), Is.EqualTo(typeof(GreenModule)));
            }
        }

        public class InsertAfterLastTests : ModuleListFixture
        {
            [Test]
            public void InsertAfterLast()
            {
                // Given
                ModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertAfterLast<RedModule>(new CountModule("foo"));

                // Then
                Assert.That(collection[0].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[1].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[2].GetType(), Is.EqualTo(typeof(CountModule)));
                Assert.That(collection[3].GetType(), Is.EqualTo(typeof(GreenModule)));
            }
        }

        public class InsertBeforeLastTests : ModuleListFixture
        {
            [Test]
            public void InsertBeforeLast()
            {
                // Given
                ModuleList collection = new ModuleList(
                    new RedModule(),
                    new RedModule(),
                    new GreenModule());

                // When
                collection.InsertBeforeLast<RedModule>(new CountModule("foo"));

                // Then
                Assert.That(collection[0].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[1].GetType(), Is.EqualTo(typeof(CountModule)));
                Assert.That(collection[2].GetType(), Is.EqualTo(typeof(RedModule)));
                Assert.That(collection[3].GetType(), Is.EqualTo(typeof(GreenModule)));
            }
        }

        public class ReplaceFirstTests : ModuleListFixture
        {
            [Test]
            public void ReplaceFirst()
            {
                // Given
                ModuleList collection = new ModuleList(
                    new RedModule(),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new GreenModule());

                // When
                collection.ReplaceFirst<CountModule>(new CountModule("replacedKey"));

                // Then
                Assert.That(((CountModule)collection[1]).ValueKey, Is.EqualTo("replacedKey"));
                Assert.That(((CountModule)collection[2]).ValueKey, Is.EqualTo("mykey2"));
            }
        }

        public class ReplaceLastTests : ModuleListFixture
        {
            [Test]
            public void ReplaceLast()
            {
                // Given
                ModuleList collection = new ModuleList(
                    new RedModule(),
                    new CountModule("mykey1"),
                    new CountModule("mykey2"),
                    new GreenModule());

                // When
                collection.ReplaceLast<CountModule>(new CountModule("replacedKey"));

                // Then
                Assert.That(((CountModule)collection[1]).ValueKey, Is.EqualTo("mykey1"));
                Assert.That(((CountModule)collection[2]).ValueKey, Is.EqualTo("replacedKey"));
            }
        }

        private class RedModule : IModule
        {
            public Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) => throw new NotImplementedException();
        }

        private class GreenModule : IModule
        {
            public Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context) => throw new NotImplementedException();
        }
    }
}
