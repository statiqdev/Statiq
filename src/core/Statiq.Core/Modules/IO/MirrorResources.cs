using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Mirrors external <c>link</c> and <c>script</c> resources locally.
    /// </summary>
    /// <remarks>
    /// <para>
    /// For each input document, looks for <c>link</c> and <c>script</c> elements that link to external
    /// resources and copies them locally, replacing the <c>link</c> and <c>script</c> links to the local path.
    /// To prevent local mirroring for specific <c>link</c> and <c>script</c> elements, add a
    /// <c>data-no-mirror</c> attribute.
    /// </para>
    /// <para>
    /// Note that because this module parses the document
    /// content as standards-compliant HTML and outputs the formatted post-parsed DOM, you should
    /// only place this module after all other template processing has been performed.
    /// </para>
    /// </remarks>
    /// <category name="Input/Output" />
    public class MirrorResources : Module
    {
        /// <summary>
        /// The valid values for a "rel" attribute in a link, anything else should be ignored.
        /// </summary>
        private static readonly HashSet<string> MirroredLinkRelValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "dns-prefetch",
            "preconnect",
            "prefetch",
            "preload",
            "prerender",
            "stylesheet",
            "icon"
        };

        private readonly Func<Uri, NormalizedPath> _pathFunc;

        /// <summary>
        /// Mirrors external resources locally. By default, resources will be copied into the output folder
        /// under the <c>mirror</c> directory.
        /// </summary>
        public MirrorResources()
            : this(x => $"mirror/{x.Host}{x.LocalPath}")
        {
        }

        /// <summary>
        /// Mirrors external resources locally, specifying the output location where they should be copied.
        /// </summary>
        /// <param name="pathFunc">A function that specifies where downloaded external resources should be copied to.</param>
        public MirrorResources(Func<Uri, NormalizedPath> pathFunc)
        {
            _pathFunc = pathFunc.ThrowIfNull(nameof(pathFunc));
        }

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Cache downloaded resources
            Dictionary<string, string> mirrorCache = new Dictionary<string, string>();

            // Iterate the input documents synchronously so we don't download the same resource more than once
            return await context.Inputs
                .ToAsyncEnumerable()
                .SelectAwait(async x => await GetDocumentAsync(x))
                .ToListAsync();

            async Task<Common.IDocument> GetDocumentAsync(Common.IDocument input)
            {
                IHtmlDocument htmlDocument = await input.ParseHtmlAsync();
                if (htmlDocument is object)
                {
                    bool modifiedDocument = false;

                    // Link element
                    foreach (IElement element in htmlDocument.GetElementsByTagName("link")
                        .Where(x => x.HasAttribute("href")
                            && !x.HasAttribute("data-no-mirror")
                            && (!x.HasAttribute("rel") || x.GetAttribute("rel").Split(' ', StringSplitOptions.RemoveEmptyEntries).All(y => MirroredLinkRelValues.Contains(y)))))
                    {
                        string replacement = await DownloadAndReplaceAsync(element.GetAttribute("href"), mirrorCache, input, context);
                        if (replacement is object)
                        {
                            element.Attributes["href"].Value = replacement;
                            element.RemoveAttribute("integrity");
                            element.RemoveAttribute("crossorigin");
                            modifiedDocument = true;
                        }
                    }

                    // Scripts
                    foreach (IHtmlScriptElement element in htmlDocument.Scripts
                        .Where(x => !string.IsNullOrEmpty(x.Source)
                            && !x.HasAttribute("data-no-mirror")))
                    {
                        string replacement = await DownloadAndReplaceAsync(element.Source, mirrorCache, input, context);
                        if (replacement is object)
                        {
                            element.Source = replacement;
                            element.RemoveAttribute("integrity");
                            element.RemoveAttribute("crossorigin");
                            modifiedDocument = true;
                        }
                    }

                    // Return a new document with the replacements if we performed any
                    if (modifiedDocument)
                    {
                        return input.Clone(context.GetContentProvider(htmlDocument));
                    }
                }

                return input;
            }
        }

        private async Task<string> DownloadAndReplaceAsync(string source, Dictionary<string, string> mirrorCache, Common.IDocument input, IExecutionContext context)
        {
            if (mirrorCache.TryGetValue(source, out string cachedValue))
            {
                return cachedValue;
            }

            // Get the destination path and link
            if (source.StartsWith("//"))
            {
                source = $"http:{source}";
            }

            // Make sure it's a valid HTTP or HTTPS URI
            if (!Uri.TryCreate(source, UriKind.Absolute, out Uri uri)
                || uri is null
                || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return null;
            }

            // Verify this isn't from the current host
            // Use both the document and the context just in case host is set or overridden at the document level
            if ((context.ContainsKey(Keys.Host) && uri.Host.Equals(context.GetString(Keys.Host), StringComparison.OrdinalIgnoreCase))
                || (input.ContainsKey(Keys.Host) && uri.Host.Equals(input.GetString(Keys.Host), StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            // Get the path and link to the file in the mirror cache
            NormalizedPath path = _pathFunc(uri);
            if (path.IsNull)
            {
                throw new ExecutionException($"Null resource mirror path for {source}");
            }
            string link = context.GetLink(
                path,
                null,
                context.Settings.GetPath(Keys.LinkRoot),
                context.Settings.GetBool(Keys.LinksUseHttps),
                false,
                false,
                context.Settings.GetBool(Keys.LinkLowercase));

            // Download the resource, but only if we haven't already written it to disk
            // Just assume that if it has been written to disk that it's the same (no need to compare hashes or content for mirrored resources)
            IFile outputFile = context.FileSystem.GetOutputFile(path);
            if (outputFile.Exists)
            {
                // Make sure to mark it as "written" though so it doesn't get cleaned up
                context.FileSystem.WriteTracker.TrackWrite(outputFile.Path, await outputFile.GetCacheCodeAsync(), false);
            }
            else
            {
                // Download the resource
                context.LogDebug($"Downloading resource from {uri} to {path.FullPath}");
                HttpResponseMessage response = await context.SendHttpRequestWithRetryAsync(uri);
                response.EnsureSuccessStatusCode();

                // Copy the result to output
                using (Stream outputStream = outputFile.OpenWrite())
                {
                    long initialPosition = outputStream.Position;
                    await response.Content.CopyToAsync(outputStream);
                    long length = outputStream.Position - initialPosition;
                    outputStream.SetLength(length);
                }
            }

            mirrorCache.Add(source, link);
            return link;
        }
    }
}