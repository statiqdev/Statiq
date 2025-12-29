using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.IO
{
    [TestFixture]
    public class ReadWebFixture : BaseFixture
    {
        public class ExecuteTests : ReadWebFixture
        {
            [Test]
            public async Task SingleHtmlDownloadGetStream()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext(document)
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new System.Net.Http.StringContent("Fizz")
                        };
                        response.Headers.Add("Foo", "Bar");
                        return response;
                    }
                };
                IModule download = new ReadWeb().WithUris("https://statiq.dev/");

                // When
                TestDocument result = await ExecuteAsync(context, download).SingleAsync();

                // Then
                Dictionary<string, string> headers = result[Keys.SourceHeaders] as Dictionary<string, string>;

                Assert.That(headers, Is.Not.Null, "Header cannot be null");
                Assert.That(headers.Count > 0, Is.True, "Headers must contain contents");

                foreach (KeyValuePair<string, string> h in headers)
                {
                    Assert.That(h.Key, Is.Not.Empty, "Header key cannot be empty");
                    Assert.That(h.Value, Is.Not.Empty, "Header value cannot be empty");
                }

                result.Content.ShouldNotBeEmpty();
            }

            [Test]
            public async Task MultipleHtmlDownload()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext(document)
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        HttpResponseMessage response = new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new System.Net.Http.StringContent("Fizz")
                        };
                        response.Headers.Add("Foo", "Bar");
                        return response;
                    }
                };
                IModule download = new ReadWeb().WithUris("https://statiq.dev/", "https://github.com/statiqdev/Statiq.Framework");

                // When
                IReadOnlyList<TestDocument> results = await ExecuteAsync(context, download);

                // Then
                foreach (TestDocument result in results)
                {
                    Dictionary<string, string> headers = result[Keys.SourceHeaders] as Dictionary<string, string>;

                    Assert.That(headers, Is.Not.Null, "Header cannot be null");
                    Assert.That(headers.Count > 0, Is.True, "Headers must contain contents");

                    foreach (KeyValuePair<string, string> h in headers)
                    {
                        Assert.That(h.Key, Is.Not.Empty, "Header key cannot be empty");
                        Assert.That(h.Value, Is.Not.Empty, "Header value cannot be empty");
                    }

                    result.Content.ShouldNotBeEmpty();
                }
            }

            [Test]
            public async Task SingleImageDownload()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext(document)
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 0x01, 0x01 })
                        };
                    }
                };
                IModule download = new ReadWeb().WithUris("https://statiq.dev/assets/img/logo.png");

                // When
                IDocument result = await ExecuteAsync(context, download).SingleAsync();

                // Then
                using (Stream stream = result.GetContentStream())
                {
                    stream.ReadByte().ShouldNotBe(-1);
                }
            }

            [Test]
            public async Task SingleImageDownloadWithRequestHeader()
            {
                // Given
                TestDocument document = new TestDocument();
                TestExecutionContext context = new TestExecutionContext(document)
                {
                    HttpResponseFunc = (_, __) =>
                    {
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new ByteArrayContent(new byte[] { 0x01, 0x01 })
                        };
                    }
                };
                WebRequestHeaders header = new WebRequestHeaders();
                header.Accept.Add("image/jpeg");
                IModule download = new ReadWeb().WithUri("https://statiq.dev/assets/img/logo.png", header);

                // When
                IDocument result = await ExecuteAsync(context, download).SingleAsync();

                // Then
                using (Stream stream = result.GetContentStream())
                {
                    stream.ReadByte().ShouldNotBe(-1);
                }
            }
        }
    }
}
