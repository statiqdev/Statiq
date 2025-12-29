using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Includes a file from the virtual file system.
    /// </summary>
    /// <remarks>
    /// The raw content of the file will be rendered where the shortcode appears.
    /// If the file does not exist nothing will be rendered.
    /// </remarks>
    /// <example>
    /// <para>Example usage to show the contents of test-include.html in the output.</para>
    /// <para>
    /// <code>
    /// &lt;?# Include "test-include.html" /?&gt;
    /// </code>
    /// </para>
    /// <para>
    /// If the included file contains Markdown syntax, you can even include it before the Markdown engine runs with a slight syntax change:
    /// </para>
    /// <para>
    /// <code>
    /// &lt;?! Include "test-include.md" /?&gt;?
    /// </code>
    /// </para>
    /// <para>You can also include HTTP/HTTPS resources.</para>
    /// <para>
    /// <code>
    /// &lt;?# Include "https://raw.githubusercontent.com/statiqdev/Statiq.Framework/master/README.md" /?&gt;
    /// </code>
    /// </para>
    /// </example>
    /// <parameter>The path to the file to include.</parameter>
    public class IncludeShortcode : Shortcode
    {
        private NormalizedPath _sourcePath = NormalizedPath.Null;

        /// <inheritdoc />
        public override async Task<ShortcodeResult> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            // See if this is a web include
            string included = args.SingleValue();
            if (included.StartsWith(Uri.UriSchemeHttp + "://") || included.StartsWith(Uri.UriSchemeHttps + "://"))
            {
                // This is a URI, so just get the web resource
                HttpResponseMessage response = await context.SendHttpRequestWithRetryAsync(included);
                if (!response.IsSuccessStatusCode)
                {
                    context.LogWarning($"Included web resource {included} returned {response.StatusCode}");
                    return null;
                }

                // Got the resource, copy to a stream and create a document result
                using (Stream contentStream = context.GetContentStream())
                {
                    await response.Content.CopyToAsync(contentStream);
                    return contentStream;
                }
            }

            // Get the included path relative to the document
            NormalizedPath includedPath = new NormalizedPath(included);
            if (_sourcePath.IsNull)
            {
                // Cache the source path for this shortcode instance since it'll be the same for all future shortcodes
                _sourcePath = document.GetPath("IncludeShortcodeSource", document.Source);
            }

            // Try to find the file relative to the current document path
            IFile includedFile = null;
            if (includedPath.IsRelative && !_sourcePath.IsNull)
            {
                includedFile = context.FileSystem.GetFile(_sourcePath.ChangeFileName(includedPath));
            }

            // If that didn't work, try relative to the input folder
            if (includedFile?.Exists != true)
            {
                includedFile = context.FileSystem.GetInputFile(includedPath);
            }

            // Get the included file
            if (!includedFile.Exists)
            {
                context.LogWarning($"Included file {includedPath.FullPath} does not exist");
                return null;
            }

            // Set the currently included shortcode source so nested includes can use it
            return new ShortcodeResult(
                includedFile.GetContentProvider(),
                new MetadataItems
                {
                    { "IncludeShortcodeSource", includedFile.Path.FullPath }
                });
        }
    }
}