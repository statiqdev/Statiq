using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Razor
{
    public static class IHtmlHelperExtensions
    {
        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document) => htmlHelper.DocumentLink(document, false);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document, IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, false, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document, bool includeHost)
        {
            document.ThrowIfNull(nameof(document));
            return htmlHelper.DocumentLink(document, document.GetTitle(), includeHost);
        }

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            bool includeHost,
            IDictionary<object, object> htmlAttributes)
        {
            document.ThrowIfNull(nameof(document));
            return htmlHelper.DocumentLink(document, document.GetTitle(), includeHost, htmlAttributes);
        }

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(this IHtmlHelper htmlHelper, IDocument document, string linkText) =>
            htmlHelper.DocumentLink(document, linkText, false);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText) =>
            htmlHelper.DocumentLink(document, queryAndFragment, linkText, false);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string linkText,
            IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, linkText, false, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText,
            IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, queryAndFragment, linkText, false, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string linkText,
            bool includeHost) =>
            htmlHelper.DocumentLink(document, linkText, includeHost, null);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText,
            bool includeHost) =>
            htmlHelper.DocumentLink(document, queryAndFragment, linkText, includeHost, null);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string linkText,
            bool includeHost,
            IDictionary<object, object> htmlAttributes) =>
            htmlHelper.DocumentLink(document, null, linkText, includeHost, htmlAttributes);

        /// <summary>
        /// Gets an anchor HTML element for the specified document using the document destination.
        /// Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="document">The document to generate an anchor element for.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the document path. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="linkText">The title to use for the anchor, or null to use the document title.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <param name="htmlAttributes">HTML attributes to add to the link.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static IHtmlContent DocumentLink(
            this IHtmlHelper htmlHelper,
            IDocument document,
            string queryAndFragment,
            string linkText,
            bool includeHost,
            IDictionary<object, object> htmlAttributes)
        {
            htmlHelper.ThrowIfNull(nameof(htmlHelper));
            document.ThrowIfNull(nameof(document));

            TagBuilder tagBuilder = new TagBuilder("a");
            tagBuilder.InnerHtml.SetContent(linkText ?? document.GetTitle());
            if (htmlAttributes is object)
            {
                tagBuilder.MergeAttributes(htmlAttributes);
            }
            tagBuilder.MergeAttribute("href", document.GetLink(queryAndFragment, includeHost));
            return tagBuilder;
        }

#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks. Use await or JoinableTaskFactory.Run instead.
        public static void RenderCachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            RenderCachedPartialAsync(htmlHelper, partialViewName).GetAwaiter().GetResult();

        public static void RenderCachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            RenderCachedPartialAsync(htmlHelper, partialViewName, model).GetAwaiter().GetResult();

        public static void RenderCachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey) =>
            RenderCachedPartialAsync(htmlHelper, partialViewName, model, cacheKey).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

        public static async Task RenderCachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            await RenderCachedPartialAsync(htmlHelper, partialViewName, null, null);

        public static async Task RenderCachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            await RenderCachedPartialAsync(htmlHelper, partialViewName, model, model);

        public static async Task RenderCachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey)
        {
            CachedPartialContent content = await GetCachedPartialContentAsync(htmlHelper, partialViewName, model, cacheKey);
            content.WriteTo(htmlHelper.ViewContext.Writer, null); // We know this call doesn't use the HtmlEncoder
        }

#pragma warning disable VSTHRD002 // Synchronously waiting on tasks or awaiters may cause deadlocks. Use await or JoinableTaskFactory.Run instead.
        public static IHtmlContent CachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            CachedPartialAsync(htmlHelper, partialViewName).GetAwaiter().GetResult();

        public static IHtmlContent CachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            CachedPartialAsync(htmlHelper, partialViewName, model).GetAwaiter().GetResult();

        public static IHtmlContent CachedPartial(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey) =>
            CachedPartialAsync(htmlHelper, partialViewName, model, cacheKey).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002

        public static async Task<IHtmlContent> CachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName) =>
            await CachedPartialAsync(htmlHelper, partialViewName, null, null);

        public static async Task<IHtmlContent> CachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model) =>
            await CachedPartialAsync(htmlHelper, partialViewName, model, model);

        public static async Task<IHtmlContent> CachedPartialAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey) =>
            await GetCachedPartialContentAsync(htmlHelper, partialViewName, model, cacheKey);

        private static readonly ConcurrentCache<(string, object), Task<CachedPartialContent>> _cachedPartialContent
            = new ConcurrentCache<(string, object), Task<CachedPartialContent>>(true, true);

        private static async Task<CachedPartialContent> GetCachedPartialContentAsync(
            this IHtmlHelper htmlHelper,
            string partialViewName,
            object model,
            object cacheKey)
        {
            htmlHelper.ThrowIfNull(nameof(htmlHelper));

            IServiceProvider serviceProvider = (IServiceProvider)htmlHelper.ViewContext.ViewData[ViewDataKeys.StatiqServiceProvider];
            ICompositeViewEngine viewEngine = serviceProvider.GetRequiredService<ICompositeViewEngine>();

            // Get the normalized path so that we can match up the partial regardless of where it's called from or the name
            // Copied from HtmlHelper.RenderPartialCoreAsync()
            ViewEngineResult viewEngineResult = viewEngine.GetView(
                htmlHelper.ViewContext.ExecutingFilePath,
                partialViewName,
                isMainPage: false);
            if (!viewEngineResult.Success)
            {
                viewEngineResult = viewEngine.FindView(htmlHelper.ViewContext, partialViewName, isMainPage: false);
            }

            // If we can't find it this way, go ahead and try again normally and that'll throw the error
            if (!viewEngineResult.Success)
            {
                throw new Exception($"Could not find partial to cache with name {partialViewName}");
            }

            // Cache the partial results using the path name by writing to a memory stream
            return await _cachedPartialContent.GetOrAdd(
                (viewEngineResult.View.Path, cacheKey),
                async (key, args) =>
                {
                    StringBuilder builder = new StringBuilder();
                    using (StringWriter writer = new StringWriter(builder))
                    {
                        // Temporarily replace the writer in the view context for rendering the partial
                        TextWriter originalWriter = args.htmlHelper.ViewContext.Writer;
                        args.htmlHelper.ViewContext.Writer = writer;
                        await (args.model is object
                            ? args.htmlHelper.RenderPartialAsync(args.partialViewName, args.model)
                            : args.htmlHelper.RenderPartialAsync(args.partialViewName));
                        args.htmlHelper.ViewContext.Writer = originalWriter;
                    }
                    return new CachedPartialContent(builder);
                },
                (partialViewName, htmlHelper, model));
        }

        private class CachedPartialContent : IHtmlContent
        {
            private readonly StringBuilder _builder;

            // StringBuilder is not thread-safe, but since we're only reading it's okay to use as the buffer
            public CachedPartialContent(StringBuilder builder)
            {
                _builder = builder;
            }

            public void WriteTo(TextWriter writer, HtmlEncoder encoder) => writer.Write(_builder);
        }
    }
}