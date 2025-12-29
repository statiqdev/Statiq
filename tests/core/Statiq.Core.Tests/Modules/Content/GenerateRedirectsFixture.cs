using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class GenerateRedirectsFixture : BaseFixture
    {
        [Test]
        public async Task SingleRedirect()
        {
            // Given
            TestDocument redirected = new TestDocument(
                new NormalizedPath("bar.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } }
                });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            results.Select(x => x.Destination.FullPath).ShouldBe(new[] { "foo.html" });
        }

        [Test]
        public async Task NoRedirectWithoutDestination()
        {
            // Given
            TestDocument redirected = new TestDocument(
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } }
                });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            results.ShouldBeEmpty();
        }

        [Test]
        public async Task SetsRedirectToMetadata()
        {
            // Given
            TestDocument redirected = new TestDocument(
                new NormalizedPath("a/b/bar.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } }
                });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            results.ShouldHaveSingleItem().GetString(Keys.RedirectTo).ShouldBe("a/b/bar.html");
        }

        [Test]
        public async Task SetsHtmlMediaType()
        {
            // Given
            TestDocument redirected = new TestDocument(
                new NormalizedPath("bar.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } }
                });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            results.Single().ContentProvider.MediaType.ShouldBe(MediaTypes.Html);
        }

        [TestCase("foo/bar", "foo/bar.html")]
        [TestCase("foo/bar.html", "foo/bar.html")]
        [TestCase("foo/bar.htm", "foo/bar.htm")]
        [TestCase("foo/bar.baz", "foo/bar.baz.html")]
        public async Task AddsExtension(string input, string expected)
        {
            // Given
            TestDocument redirected = new TestDocument(
                new NormalizedPath("bar.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath(input) } }
                });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected, notRedirected }, redirect);

            // Then
            results.Select(x => x.Destination.FullPath).ShouldBe(new[] { expected });
        }

        [Test]
        public async Task WarnsForAbsoluteRedirectFromPath()
        {
            // Given
            TestDocument redirected = new TestDocument(
                new NormalizedPath("bar.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("/foo/bar") } }
                });
            TestDocument notRedirected = new TestDocument();
            GenerateRedirects redirect = new GenerateRedirects();
            TestExecutionContext context = new TestExecutionContext(redirected, notRedirected);
            context.TestLoggerProvider.ThrowLogLevel = LogLevel.None;

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(context, redirect);

            // Then
            context.LogMessages.ShouldContain(x => x.LogLevel == LogLevel.Warning && x.FormattedMessage.StartsWith("The redirect path must be relative"));
            results.ShouldBeEmpty();
        }

        [Test]
        public async Task MultipleRedirects()
        {
            // Given
            TestDocument redirected1 = new TestDocument(
                new NormalizedPath("bar.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } }
                });
            TestDocument redirected2 = new TestDocument(
                new NormalizedPath("baz.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("bar/baz.html") } }
                });
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            results.Select(x => x.Destination.FullPath).ShouldBe(new[] { "foo.html", "bar/baz.html" });
        }

        [Test]
        public async Task WithAdditionalOutput()
        {
            // Given
            TestDocument redirected1 = new TestDocument(
                new NormalizedPath("foo2.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } }
                });
            TestDocument redirected2 = new TestDocument(
                new NormalizedPath("fizz.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("bar/baz.html") } }
                });
            GenerateRedirects redirect = new GenerateRedirects()
                .WithAdditionalOutput(new NormalizedPath("a/b"), x => string.Join("|", x.Select(y => $"{y.Key} {y.Value}")));

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            results.Select(x => x.Destination.FullPath).ShouldBe(new[] { "foo.html", "bar/baz.html", "a/b" });
            results.Single(x => x.Destination.FullPath == "a/b").Content.ShouldContain("foo.html /foo2.html");
        }

        [Test]
        public async Task WithAdditionalOutputWithoutMetaRefresh()
        {
            // Given
            TestDocument redirected1 = new TestDocument(
                new NormalizedPath("foo2.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } }
                });
            TestDocument redirected2 = new TestDocument(
                new NormalizedPath("fizz.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("bar/baz.html") } }
                });
            GenerateRedirects redirect = new GenerateRedirects()
                .WithAdditionalOutput(new NormalizedPath("a/b"), x => string.Join("|", x.Select(y => $"{y.Key} {y.Value}")))
                .WithMetaRefreshPages(false);

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            results.Select(x => x.Destination.FullPath).ShouldBe(new[] { "a/b" });
        }

        [Test]
        public async Task ShouldNotGenerateAdditionalOutputIfNoRedirects()
        {
            // Given
            TestDocument redirected1 = new TestDocument(new NormalizedPath("foo2.html"));
            TestDocument redirected2 = new TestDocument(new NormalizedPath("fizz.html"));
            GenerateRedirects redirect = new GenerateRedirects()
                .WithAdditionalOutput(new NormalizedPath("a/b"), x => "foobar");

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            results.Select(x => x.Destination.FullPath).ShouldBeEmpty();
        }

        [Test]
        public async Task ShouldGenerateAdditionalOutputIfNoRedirects()
        {
            // Given
            TestDocument redirected1 = new TestDocument(new NormalizedPath("foo2.html"));
            TestDocument redirected2 = new TestDocument(new NormalizedPath("fizz.html"));
            GenerateRedirects redirect = new GenerateRedirects()
                .WithAdditionalOutput(new NormalizedPath("a/b"), x => "foobar")
                .AlwaysCreateAdditionalOutput();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected1, redirected2 }, redirect);

            // Then
            results.Select(x => x.Destination.FullPath).ShouldBe(new[] { "a/b" });
            results.Single(x => x.Destination.FullPath == "a/b").Content.ShouldBe("foobar");
        }

        [Test]
        public async Task DifferentBody()
        {
            // Given
            TestDocument redirected = new TestDocument(
                new NormalizedPath("bar.html"),
                new MetadataItems
                {
                    { Keys.RedirectFrom, new List<NormalizedPath> { new NormalizedPath("foo.html") } },
                    { Keys.RedirectBody, "<p>FOOBAR</p>" }
                });
            GenerateRedirects redirect = new GenerateRedirects();

            // When
            IReadOnlyList<TestDocument> results = await ExecuteAsync(new[] { redirected }, redirect);

            // Then
            results.Single().Content.ShouldContain("<p>FOOBAR</p>");
        }
    }
}