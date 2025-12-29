using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Feeds.Tests
{
    [TestFixture]
    public class GenerateFeedsFixture : BaseFixture
    {
        public class ExecuteTests : GenerateFeedsFixture
        {
            [Test]
            public async Task DoesNotChangeImageDomain()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument document = new TestDocument(
                    new NormalizedPath("/input/fizz/buzz"),
                    new NormalizedPath("fizz/buzz"),
                    new Dictionary<string, object>
                    {
                        { FeedKeys.Image, new Uri("http://foo.com/bar/baz.png") }
                    });
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                results.Select(x => x.Destination.FullPath).ShouldBe(new[] { "feed.rss", "feed.atom" }, true);
                results[0].Content.ShouldContain("http://foo.com/bar/baz.png");
            }

            [Test]
            public async Task OrdersByPublish()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.IndexOf("<title>B</title>").ShouldBeLessThan(result.Content.IndexOf("<title>A</title>"));
            }

            [Test]
            public async Task ShouldSetFeedTitleFromMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[FeedKeys.Title] = "My Feed";
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.ShouldContain("<title>My Feed</title>");
            }

            [Test]
            public async Task ShouldSetDefaultFeedTitleIfNoMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.ShouldContain("<title>Feed</title>");
            }

            [Test]
            public async Task ShouldSetFeedTitleFromFluentMethod()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds().WithFeedTitle("Best Feed");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.ShouldContain("<title>Best Feed</title>");
            }

            [TestCase(null)]
            [TestCase("")]
            public async Task ShouldSetDefaultFeedTitleIfNullOrEmptyFluentTitle(string feedTitle)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds().WithFeedTitle(feedTitle);

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.ShouldContain("<title>Feed</title>");
            }

            [Test]
            public async Task PreservesInputOrder()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument a = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Published, new DateTime(2010, 1, 1) }
                };
                TestDocument b = new TestDocument(new NormalizedPath("b.txt"))
                {
                    { FeedKeys.Title, "B" },
                    { FeedKeys.Published, new DateTime(2010, 2, 1) }
                };
                GenerateFeeds module = new GenerateFeeds().PreserveOrdering();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { a, b }, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.IndexOf("<title>A</title>").ShouldBeLessThan(result.Content.IndexOf("<title>B</title>"));
            }

            [Test]
            public async Task MakesLinksAbsoluteInDescription()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument document = new TestDocument(new NormalizedPath("a.txt"))
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Description, "<p>Foo <a href=\"/fizz/buzz\">Fizzbuzz</a> bar <img src=\"/abc/def.png\"> baz</p>" }
                };
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.ShouldContain(@"<description>&lt;p&gt;Foo &lt;a href=""http://statiq.dev/fizz/buzz""&gt;Fizzbuzz&lt;/a&gt; bar &lt;img src=""http://statiq.dev/abc/def.png""&gt; baz&lt;/p&gt;</description>");
            }

            [Test]
            public async Task MakesLinksAbsoluteInContent()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "statiq.dev";
                TestDocument document = new TestDocument(new NormalizedPath("a.txt"), "<p>Foo <a href=\"/fizz/buzz\">Fizzbuzz</a> bar <img src=\"/abc/def.png\"> baz</p>")
                {
                    { FeedKeys.Title, "A" },
                    { FeedKeys.Description, "Hello" }
                };
                GenerateFeeds module = new GenerateFeeds();

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(document, context, module);

                // Then
                TestDocument result = results.Single(x => x.Destination.FullPath == "feed.rss");
                result.Content.ShouldContain(@"<content:encoded>&lt;p&gt;Foo &lt;a href=""http://statiq.dev/fizz/buzz""&gt;Fizzbuzz&lt;/a&gt; bar &lt;img src=""http://statiq.dev/abc/def.png""&gt; baz&lt;/p&gt;</content:encoded>");
            }
        }
    }
}