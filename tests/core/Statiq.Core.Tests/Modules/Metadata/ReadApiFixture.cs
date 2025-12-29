using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    /// <summary>
    /// Test fixture for <see cref="ReadApi{TClient}"/>.
    /// </summary>
    [TestFixture]
    public class ReadApiFixture : BaseFixture
    {
        /// <summary>
        /// Tests for execute method.
        /// </summary>
        public class ExecuteTests : ReadApiFixture
        {
            /// <summary>
            /// Sets the metadata from the requests with client instance.
            /// </summary>
            [TestCase(1)]
            [TestCase(-1)]
            public async Task SetsMetadataWithClientInstance(int requestLimit)
            {
                // Given
                HttpClient httpClient = new HttpClient();
                TestDocument document = new TestDocument();
                IModule client = new ReadApi<HttpClient>(httpClient, "client_name")
                    .WithClientInitialization((doc, ctx, c) =>
                    {
                        c.DefaultRequestHeaders.Add("Authorization", "Bearer ...");
                    })
                    .WithRequestLimit(requestLimit)
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, client).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }

            /// <summary>
            /// Sets the metadata from the requests with throttling.
            /// </summary>
            [NonParallelizable]
            [TestCase(1, 1000, 2000, -1)]
            [TestCase(2, 1000, 1000, 2000)]
            [TestCase(3, 1000, -1, 1000)]
            [TestCase(1, 0, 200, -1)]
            [TestCase(2, 0, 100, 200)]
            [TestCase(3, 0, -1, 100)]
            public async Task SetsMetadataWithThrottling(int requestLimit, int requestDelay, int minimumDuration, int maximumDuration)
            {
                // Given
                HttpClient httpClient = new HttpClient();
                TestDocument document = new TestDocument();
                IModule client = new ReadApi<HttpClient>(httpClient)
                    .WithRequestLimit(requestLimit)
                    .WithRequestDelay(requestDelay)
                    .WithRequest("Foo", async (ctx, yt) =>
                    {
                        DateTime now = DateTime.Now;
                        ctx.LogDebug("Foo: DateTime now is {0}", now.ToString("HH:mm:ss.fff"));
                        await Task.Delay(150);
                        return now;
                    })
                    .WithRequest("Buz", async (doc, ctx, yt) =>
                    {
                        DateTime now = DateTime.Now;
                        ctx.LogDebug("Buz: DateTime now is {0}", now.ToString("HH:mm:ss.fff"));
                        await Task.Delay(150);
                        return now;
                    })
                    .WithRequest("Bar", async (doc, ctx, yt) =>
                    {
                        DateTime now = DateTime.Now;
                        ctx.LogDebug("Bar: DateTime now is {0}", now.ToString("HH:mm:ss.fff"));
                        await Task.Delay(150);
                        return now;
                    });

                // When
                TestDocument result = await ExecuteAsync(document, client).SingleAsync();

                // Then
                DateTime fooDateTime = result["Foo"].ShouldBeOfType<DateTime>();
                DateTime barDateTime = result["Bar"].ShouldBeOfType<DateTime>();
                if (minimumDuration != -1)
                {
                    Math.Abs(fooDateTime.Subtract(barDateTime).TotalMilliseconds).ShouldBeGreaterThanOrEqualTo(minimumDuration);
                }
                else if (maximumDuration != -1)
                {
                    Math.Abs(fooDateTime.Subtract(barDateTime).TotalMilliseconds).ShouldBeLessThanOrEqualTo(maximumDuration);
                }
            }

            /// <summary>
            /// Sets the metadata from the requests with client factory.
            /// </summary>
            [TestCase(1)]
            [TestCase(-1)]
            public async Task SetsMetadataWithClientFactory(int requestLimit)
            {
                // Given
                HttpClient httpClient = new HttpClient();
                TestDocument document = new TestDocument();
                IModule client = new ReadApi<HttpClient>(Config.FromDocument(_ => new HttpClient()), "client_name")
                    .WithClientInitialization((doc, ctx, c) =>
                    {
                        c.DefaultRequestHeaders.Add("Authorization", "Bearer ...");
                    })
                    .WithRequestLimit(requestLimit)
                    .WithRequest("Foo", (ctx, yt) => 1)
                    .WithRequest("Bar", (doc, ctx, yt) => "baz");

                // When
                TestDocument result = await ExecuteAsync(document, client).SingleAsync();

                // Then
                result["Foo"].ShouldBe(1);
                result["Bar"].ShouldBe("baz");
            }
        }
    }
}
