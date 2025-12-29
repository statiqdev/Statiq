using System;
using System.IO;
using Markdig;
using Markdig.Helpers;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using NetFabric.Hyperlinq;
using Statiq.Common;

namespace Statiq.Markdown
{
    public static class MarkdownHelper
    {
        /// <summary>
        /// The default Markdown configuration.
        /// </summary>
        public const string DefaultConfiguration = "common";

        public static MarkdownDocument RenderMarkdown(
            IExecutionState executionState,
            IDocument document,
            string content,
            TextWriter writer = null,
            bool prependLinkRoot = false,
            bool passThroughRawFence = true,
            bool escapeAtInRawFence = false,
            string configuration = DefaultConfiguration,
            OrderedList<IMarkdownExtension> extensions = null)
        {
            document.ThrowIfNull(nameof(document));
            content.ThrowIfNull(nameof(content));
            configuration.ThrowIfNullOrEmpty(nameof(configuration));

            try
            {
                // Create the pipeline
                MarkdownPipelineBuilder pipelineBuilder = new MarkdownPipelineBuilder();
                pipelineBuilder.Configure(configuration);
                if (extensions is object)
                {
                    pipelineBuilder.Extensions.AddRange(extensions);
                }
                MarkdownPipeline pipeline = pipelineBuilder.Build();

                // Render the content
                HtmlRenderer htmlRenderer = new HtmlRenderer(writer ?? NullTextWriter.Instance);
                pipeline.Setup(htmlRenderer);

                htmlRenderer.LinkRewriter = (link) =>
                {
                    if (string.IsNullOrEmpty(link))
                    {
                        return link;
                    }

                    if (!RelativeUrl.IsRelative(link))
                    {
                        return executionState.LinkGenerator.TryGetAbsoluteHttpUri(link, out string absoluteUri) ? absoluteUri : link;
                    }

                    // TODO: Remove when RenderMarkdown.PrependLinkRoot goes away.
                    if (prependLinkRoot)
                    {
                        if (!link.StartsWith("/"))
                        {
                            link = "/" + link;
                        }

                        link = "~" + link;
                    }

                    RelativeUrl relativeUrl = new RelativeUrl(link, document.GetString(Keys.LinkRoot));

                    return relativeUrl.ToString();
                };

                MarkdownDocument markdownDocument = MarkdownParser.Parse(content, pipeline);

                // Pass through raw code fences
                if (passThroughRawFence)
                {
                    foreach (FencedCodeBlock codeBlock in markdownDocument.Descendants<FencedCodeBlock>())
                    {
                        if (codeBlock.Info == "raw")
                        {
                            ContainerBlock parent = codeBlock.Parent;
                            int childIndex = parent.IndexOf(codeBlock);
                            parent.Remove(codeBlock);
                            HtmlBlock rawBlock = new HtmlBlock(null)
                            {
                                Lines = codeBlock.Lines
                            };
                            if (escapeAtInRawFence)
                            {
                                string lines = rawBlock.Lines.ToString();
                                if (lines.Contains('@'))
                                {
                                    rawBlock.Lines.Clear();
                                    rawBlock.Lines = new StringLineGroup(lines.Replace("@", "\\@"));
                                }
                            }
                            parent.Insert(childIndex, rawBlock);
                        }
                    }
                }

                htmlRenderer.Render(markdownDocument);
                writer.Flush();
                return markdownDocument;
            }
            catch (Exception ex)
            {
                document.LogWarning($"Exception while rendering Markdown: {ex.Message}");
            }
            return null;
        }
    }
}