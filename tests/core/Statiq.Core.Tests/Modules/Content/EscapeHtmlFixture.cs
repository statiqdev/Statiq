using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class EscapeHtmlFixture : BaseFixture
    {
        public class ExecuteTests : EscapeHtmlFixture
        {
            [Test]
            public async Task NoReplacementReturnsSameDocument()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                EscapeHtml htmlEscape = new EscapeHtml();

                // When
                TestDocument result = await ExecuteAsync(document, htmlEscape).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            public async Task EscapeWith()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Die Sache mit dem Umlaut</h1>
                            <p>Lerchen-Lärchen-Ähnlichkeiten<br/>
                            fehlen.Dieses abzustreiten<br/>
                            mag im Klang der Worte liegen.<br/>
                            Merke, eine Lerch‘ kann fliegen,<br/>
                            Lärchen nicht, was kaum verwundert,<br/>
                            denn nicht eine unter hundert<br/>
                            ist geflügelt.Auch im Singen<br/>
                            sind die Bäume zu bezwingen.<br/>
                            <br/>
                            Die Bätrachtung sollte reichen,<br/>
                            Rächtschreibfählern auszuweichen.<br/>
                            Leicht gälingt’s, zu unterscheiden,<br/>
                            wär ist wär nun von dän beiden.</p>
                            <p>©Ingo Baumgartner, <u>2013</u><br/>
                            Aus der Sammlung<u>Humor, Satire und Nonsens</u>
                            </a>
                            </p>
                        </body>
                    </html>";
                const string output = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Die Sache mit dem Umlaut</h1>
                            <p>Lerchen-L&auml;rchen-&Auml;hnlichkeiten<br/>
                            fehlen.Dieses abzustreiten<br/>
                            mag im Klang der Worte liegen.<br/>
                            Merke, eine Lerch‘ kann fliegen,<br/>
                            L&auml;rchen nicht, was kaum verwundert,<br/>
                            denn nicht eine unter hundert<br/>
                            ist gefl&uuml;gelt.Auch im Singen<br/>
                            sind die B&auml;ume zu bezwingen.<br/>
                            <br/>
                            Die B&auml;trachtung sollte reichen,<br/>
                            R&auml;chtschreibf&auml;hlern auszuweichen.<br/>
                            Leicht g&auml;lingt’s, zu unterscheiden,<br/>
                            w&auml;r ist w&auml;r nun von d&auml;n beiden.</p>
                            <p>&copy;Ingo Baumgartner, <u>2013</u><br/>
                            Aus der Sammlung<u>Humor, Satire und Nonsens</u>
                            </a>
                            </p>
                        </body>
                    </html>";
                TestDocument document = new TestDocument(input);
                EscapeHtml htmlEscape = new EscapeHtml().WithEscapedChar('ä', 'ö', 'ü', 'Ä', 'Ö', 'Ü', 'ß', '©');

                // When
                TestDocument result = await ExecuteAsync(document, htmlEscape).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
            }

            [Test]
            public async Task EscapeNonStandard()
            {
                // Given
                const string input = @"<html>
                        <head>
                            <title>Foobar</title>
                        </head>
                        <body>
                            <h1>Title</h1>
                            <p>This is some Foobar text</p>
                        </body>
                    </html>";
                const string output = @"&lt;html&gt;
                        &lt;head&gt;
                            &lt;title&gt;Foobar&lt;&#47;title&gt;
                        &lt;&#47;head&gt;
                        &lt;body&gt;
                            &lt;h1&gt;Title&lt;&#47;h1&gt;
                            &lt;p&gt;This is some Foobar text&lt;&#47;p&gt;
                        &lt;&#47;body&gt;
                    &lt;&#47;html&gt;";
                TestDocument document = new TestDocument(input);
                EscapeHtml htmlEscape = new EscapeHtml().EscapeAllNonstandard().WithDefaultStandard();

                // When
                TestDocument result = await ExecuteAsync(document, htmlEscape).SingleAsync();

                // Then
                result.Content.ShouldBe(output);
            }
        }
    }
}