using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Markdown.Tests
{
    [TestFixture]
    public class RenderMarkdownFixture : BaseFixture
    {
        public class ExecuteTests : RenderMarkdownFixture
        {
            [Test]
            public async Task RendersMarkdown()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CanUseExternalExtensionDirectly()
            {
                TestDocument document = new TestDocument();
                TestMarkdownExtension extension = new TestMarkdownExtension();
                RenderMarkdown markdown = new RenderMarkdown().UseExtension(extension);

                // When
                await ExecuteAsync(document, markdown);

                // Then
                extension.ReceivedSetup.ShouldBeTrue();
            }

            [Test]
            public async Task CanUseExternalExtension()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image"" alt=""Alt text"" /></p>
";
                TestDocument document = new TestDocument(input);
                Type[] o = { typeof(TestMarkdownExtension) };
                IEnumerable<Type> cast = o as IEnumerable<Type>;
                RenderMarkdown markdown = new RenderMarkdown().UseExtensions(cast);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task CanUseMultipleExternalExtensions()
            {
                const string input = "![Alt text](/path/to/img.jpg)";
                const string output = @"<p><img src=""/path/to/img.jpg"" class=""ui spaced image second"" alt=""Alt text"" /></p>
";
                TestDocument document = new TestDocument(input);
                Type[] o =
                {
                    typeof(TestMarkdownExtension),
                    typeof(AlternateTestMarkdownExtension)
                };
                IEnumerable<Type> cast = o;
                RenderMarkdown markdown = new RenderMarkdown().UseExtensions(cast);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotRenderSpecialAttributesByDefault()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"">link</a>{#id .class}</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesRenderSpecialAttributesIfExtensionsActive()
            {
                // Given
                const string input = "[link](url){#id .class}";
                const string output = @"<p><a href=""url"" id=""id"" class=""class"">link</a></p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().UseExtensions();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNotRenderDefinitionListWithoutExtensions()
            {
                // Given
                const string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                const string output = @"<p>Apple
:   Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesRenderDefinitionListWithSpecificConfiguration()
            {
                // Given
                const string input = @"Apple
:   Pomaceous fruit of plants of the genus Malus in 
    the family Rosaceae.";
                const string output = @"<dl>
<dt>Apple</dt>
<dd>Pomaceous fruit of plants of the genus Malus in
the family Rosaceae.</dd>
</dl>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().UseConfiguration("definitionlists");

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldEscapeAtByDefault()
            {
                // Given
                const string input = "Looking @Good, @Man!";
                const string output = @"<p>Looking &#64;Good, &#64;Man!</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenFalse()
            {
                // Given
                const string input = "Looking @Good, @Man!";
                const string output = @"<p>Looking @Good, @Man!</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldEscapeAtWhenOverriddenByMetadata()
            {
                // Given
                const string input = "Looking @Good, @Man!";
                const string output = @"<p>Looking &#64;Good, &#64;Man!</p>
";
                TestDocument document = new TestDocument(input)
                {
                    { MarkdownKeys.EscapeAtInMarkdown, true }
                };
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenOverriddenByMetadata()
            {
                // Given
                const string input = "Looking @Good, @Man!";
                const string output = @"<p>Looking @Good, @Man!</p>
";
                TestDocument document = new TestDocument(input)
                {
                    { MarkdownKeys.EscapeAtInMarkdown, false }
                };
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenEscapedBySlash()
            {
                // Given
                const string input = @"Looking @Good, \@Man!";
                const string output = @"<p>Looking &#64;Good, @Man!</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtDoubleSlash()
            {
                // Given
                // The first slash is treated as a Markdown escape of the second slash,
                // so a single slash is output, the EscapeAtParser doesn't see it and consume,
                // and the EscapeAtWriter still sees the single output slash
                const string input = @"Looking @Good, \\@Man!";
                const string output = @"<p>Looking &#64;Good, @Man!</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtEscapedDoubleSlash()
            {
                // Given
                const string input = @"Looking @Good, \\\@Man!";
                const string output = @"<p>Looking &#64;Good, \@Man!</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldEscapeAtInHtmlElement()
            {
                // Given
                const string input = "<div>Looking @Good, @Man!</div>";
                const string output = @"<div>Looking &#64;Good, &#64;Man!</div>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenFalseInHtmlElement()
            {
                // Given
                const string input = "<div>Looking @Good, @Man!</div>";
                const string output = @"<div>Looking @Good, @Man!</div>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenEscapedBySlashInHtmlElement()
            {
                // Given
                const string input = "<div>Looking @Good, \\@Man!</div>";
                const string output = @"<div>Looking &#64;Good, @Man!</div>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldEscapeAtInCode()
            {
                // Given
                const string input = "`Looking @Good, @Man!`";
                const string output = @"<p><code>Looking &#64;Good, &#64;Man!</code></p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenFalseInCode()
            {
                // Given
                const string input = "`Looking @Good, @Man!`";
                const string output = @"<p><code>Looking @Good, @Man!</code></p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenEscapedBySlashInCode()
            {
                // Given
                const string input = "`Looking @Good, \\@Man!`";
                const string output = @"<p><code>Looking &#64;Good, @Man!</code></p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldEscapeAtInCodeFence()
            {
                // Given
                const string input = @"```
Looking @Good, @Man!
```";
                const string output = @"<pre><code>Looking &#64;Good, &#64;Man!
</code></pre>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenFalseInCodeFence()
            {
                // Given
                const string input = @"```
Looking @Good, @Man!
```";
                const string output = @"<pre><code>Looking @Good, @Man!
</code></pre>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtWhenEscapedBySlashInCodeFence()
            {
                // Given
                const string input = @"```
Looking @Good, \@Man!
```";
                const string output = @"<pre><code>Looking &#64;Good, @Man!
</code></pre>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            // Very special exception since mailto seems to be one place where escaping is universally bad
            [Test]
            public async Task ShouldNotEscapeAtInMailToLink()
            {
                // Given
                const string input = "<div>Looking <a href=\"mailto:foo@bar.com\">Good</a>, @Man!</div>";
                const string output = @"<div>Looking <a href=""mailto:foo@bar.com"">Good</a>, &#64;Man!</div>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldPassThroughSlash()
            {
                // Given
                // Need to use double-slashes in cases where Markdown would treat it as an escape
                const string input = @"\Loo\king \\!Good, \\$Man!\\";
                const string output = @"<p>\Loo\king \!Good, \$Man!\</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldOutputProcessingInstructionsVerbatim()
            {
                // Given
                const string input = @"<?^ Raw ?>
<?*
@section Name { }
?>
<?^/ Raw ?>";
                const string output = @"<?^ Raw ?>
<?*
@section Name { }
?>
<?^/ Raw ?>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldPassThroughRawFenceByDefault()
            {
                // Given
                const string input = @"Fizz
```raw
Hello! @foo <b>bar</b>

    After return and spaces
    
```";
                const string output = @"<p>Fizz</p>
Hello! @foo <b>bar</b>

    After return and spaces
    
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotEscapeAtInRawFence()
            {
                // Given
                const string input = @"Fizz
```raw
Hello! @foo <b>bar</b>

    After return and spaces
    
```";
                const string output = @"<p>Fizz</p>
Hello! @foo <b>bar</b>

    After return and spaces
    
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotPassThroughRawFence()
            {
                // Given
                const string input = @"Fizz
```raw
Hello! @foo <b>bar</b>

    After return and spaces
    
```";
                const string output = @"<p>Fizz</p>
<pre><code class=""language-raw"">Hello! &#64;foo &lt;b&gt;bar&lt;/b&gt;

    After return and spaces
    
</code></pre>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown().PassThroughRawFence(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldNotPassThroughRawFenceOrEscapeAt()
            {
                // Given
                const string input = @"Fizz
```raw
Hello! @foo <b>bar</b>

    After return and spaces
    
```";
                const string output = @"<p>Fizz</p>
<pre><code class=""language-raw"">Hello! @foo &lt;b&gt;bar&lt;/b&gt;

    After return and spaces
    
</code></pre>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown()
                    .PassThroughRawFence(false)
                    .EscapeAt(false);

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersMarkdownFromMetadata()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                RenderMarkdown markdown = new RenderMarkdown("meta");

                // When
                IDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.GetString("meta").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersMarkdownFromMetadataToNewKey()
            {
                // Given
                const string input = @"Line 1
*Line 2*
# Line 3";
                const string output = @"<p>Line 1
<em>Line 2</em></p>
<h1>Line 3</h1>
";
                TestDocument document = new TestDocument(new MetadataItems
                {
                    { "meta", input }
                });
                RenderMarkdown markdown = new RenderMarkdown("meta", "meta2");

                // When
                IDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.GetString("meta2").ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task DoesNothingIfMetadataKeyDoesNotExist()
            {
                // Given
                TestDocument document = new TestDocument();
                RenderMarkdown markdown = new RenderMarkdown("meta");

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.ShouldBe(document);
            }

            [Test]
            [Obsolete]
            public async Task UsePrependLinkRootSetting()
            {
                // Given
                const string input = "This is a [link](/link.html)";
                string output = @"<p>This is a <a href=""/virtual-dir/link.html"">link</a></p>" + Environment.NewLine;
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input)
                {
                    { Keys.LinkRoot, "/virtual-dir" }
                };
                RenderMarkdown markdown = new RenderMarkdown().PrependLinkRoot(true);

                // When
                TestDocument result = await ExecuteAsync(document, context, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task UsingTildeAddsLinkRoot()
            {
                // Given
                const string input = "[link](~/foo)";
                string expected = "<p><a href=\"/virtual-dir/foo\">link</a></p>" + Environment.NewLine;
                TestExecutionContext context = new TestExecutionContext();
                TestDocument document = new TestDocument(input)
                {
                    { Keys.LinkRoot, "/virtual-dir" }
                };
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, context, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(expected, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHttpLinks()
            {
                // Given
                const string input = @"Visit my [web](http://www.statiq.dev) site.";
                const string output = @"<p>Visit my <a href=""http://www.statiq.dev/"">web</a> site.</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersHttpsLinks()
            {
                // Given
                const string input = @"Visit my [web](https://www.statiq.dev) site.";
                const string output = @"<p>Visit my <a href=""https://www.statiq.dev/"">web</a> site.</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task RendersNonHttpLinks()
            {
                // Given
                const string input = @"Email me at [my](foo:bar) address.";
                const string output = @"<p>Email me at <a href=""foo:bar"">my</a> address.</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldAddExtensionsFromMetadata()
            {
                // Given
                const string input = @"# Hi!

A simple | table
-- | --
with multiple | lines

End";
                const string output = @"<h1>Hi!</h1>
<table class=""table"">
<thead>
<tr>
<th>A simple</th>
<th>table</th>
</tr>
</thead>
<tbody>
<tr>
<td>with multiple</td>
<td>lines</td>
</tr>
</tbody>
</table>
<p>End</p>
";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            MarkdownKeys.MarkdownExtensions,
                            new[] { "PipeTableExtension", "BootstrapExtension" }
                        }
                    },
                    input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            [Test]
            public async Task ShouldAddExtensionsFromMetadataWithSimpleNames()
            {
                // Given
                const string input = @"# Hi!

A simple | table
-- | --
with multiple | lines

End";
                const string output = @"<h1>Hi!</h1>
<table class=""table"">
<thead>
<tr>
<th>A simple</th>
<th>table</th>
</tr>
</thead>
<tbody>
<tr>
<td>with multiple</td>
<td>lines</td>
</tr>
</tbody>
</table>
<p>End</p>
";
                TestDocument document = new TestDocument(
                    new MetadataItems
                    {
                        {
                            MarkdownKeys.MarkdownExtensions,
                            new[] { "PipeTableExtension", "BootstrapExtension" }
                        }
                    },
                    input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }

            // Older versions of Markdig had a problem related to this pattern, see
            // https://github.com/statiqdev/Statiq.Framework/issues/267
            [Test]
            public async Task ShouldCorrectlyRenderAltTextWithFollowingText()
            {
                // Given
                const string input = @"![alt text][fastcar2]

[fastcar2]: img/car.jfif ""VROOM""

any arbitrary text";
                const string output = @"<p><img src=""img/car.jfif"" alt=""alt text"" title=""VROOM"" /></p>
<p>any arbitrary text</p>
";
                TestDocument document = new TestDocument(input);
                RenderMarkdown markdown = new RenderMarkdown();

                // When
                TestDocument result = await ExecuteAsync(document, markdown).SingleAsync();

                // Then
                result.Content.ShouldBe(output, StringCompareShould.IgnoreLineEndings);
            }
        }
    }
}