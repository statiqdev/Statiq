using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Execution
{
    [TestFixture]
    public class IExecutionContextLinkExtensionsFixture : BaseFixture
    {
        public class GetLinkTests : IExecutionContextLinkExtensionsFixture
        {
            [TestCase("http://foo.com/bar", false, "http://foo.com/bar")]
            [TestCase("http://foo.com/bar", true, "http://foo.com/bar")]
            [TestCase("https://foo.com/bar", false, "https://foo.com/bar")]
            [TestCase("https://foo.com/bar", true, "https://foo.com/bar")]
            [TestCase("foo/bar", false, "/foo/bar")]
            [TestCase("foo/bar", true, "http://domain.com/foo/bar")]
            [TestCase("/foo/bar", false, "/foo/bar")]
            [TestCase("/foo/bar", true, "http://domain.com/foo/bar")]
            [TestCase("//foo/bar", false, "//foo/bar")]
            [TestCase("//foo/bar", true, "http://domain.com//foo/bar")]
            public void UsesAbsoluteLinkIfProvided(string value, bool includeHost, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument
                {
                    { "Path", value }
                };

                // When
                string result = ((IExecutionContext)context).GetLink(document, "Path", includeHost);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("foo/bar.txt", false, "/foo/bar.txt")]
            [TestCase("foo/bar.txt", true, "http://domain.com/foo/bar.txt")]
            public void GetsLinkFromDestination(string destination, bool includeHost, string expected)
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Settings[Keys.Host] = "domain.com";
                TestDocument document = new TestDocument(null, new NormalizedPath(destination));

                // When
                string result = ((IExecutionContext)context).GetLink(document, includeHost);

                // Then
                result.ShouldBe(expected);
            }
        }
    }
}