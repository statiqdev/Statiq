using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Events
{
    [TestFixture]
    public class EventCollectionFixture : BaseFixture
    {
        public class RaiseTests : EventCollectionFixture
        {
            [Test]
            public async Task SubscribesAsyncEvent()
            {
                // Given
                IEventCollection eventCollection = new EventCollection();
                eventCollection.Subscribe<TestEvent>(async args =>
                {
                    args.Foo = 4;
                    await Task.CompletedTask;
                });
                TestEvent args = new TestEvent { Foo = 1 };

                // When
                await eventCollection.RaiseAsync(args);

                // Then
                args.Foo.ShouldBe(4);
            }

            [Test]
            public async Task SubscribesSyncEvent()
            {
                // Given
                IEventCollection eventCollection = new EventCollection();
                eventCollection.Subscribe<TestEvent>(args =>
                {
                    args.Foo = 4;
                });
                TestEvent args = new TestEvent { Foo = 1 };

                // When
                await eventCollection.RaiseAsync(args);

                // Then
                args.Foo.ShouldBe(4);
            }

            [Test]
            public async Task SubscribesMultipleEvents()
            {
                // Given
                IEventCollection eventCollection = new EventCollection();
                eventCollection.Subscribe<TestEvent>(async args =>
                {
                    args.Foo++;
                    await Task.CompletedTask;
                });
                eventCollection.Subscribe<TestEvent>(async args =>
                {
                    args.Foo++;
                    await Task.CompletedTask;
                });
                eventCollection.Subscribe<TestEvent>(args =>
                {
                    args.Foo++;
                });
                TestEvent args = new TestEvent { Foo = 3 };

                // When
                await eventCollection.RaiseAsync(args);

                // Then
                args.Foo.ShouldBe(6);
            }
        }

        public class TestEvent
        {
            public int Foo { get; set; }
        }
    }
}
