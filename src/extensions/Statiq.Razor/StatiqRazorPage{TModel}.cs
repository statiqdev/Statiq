using System;
using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Statiq.Common;

namespace Statiq.Razor
{
    public abstract class StatiqRazorPage<TModel> : RazorPage<TModel>
    {
        private IHtmlHelper<TModel> _htmlHelper;

        public IDocument Document => ViewData[ViewDataKeys.StatiqDocument] as IDocument;
        public IMetadata Metadata => Document;

        public IExecutionContext ExecutionContext => ViewData[ViewDataKeys.StatiqExecutionContext] as IExecutionContext;
        public new IExecutionContext Context => ExecutionContext;
        public ImmutableArray<IDocument> Inputs => ExecutionContext.Inputs;
        public IPipelineOutputs Outputs => ExecutionContext.Outputs;
        public FilteredDocumentList<IDocument> OutputPages => ExecutionContext.OutputPages;

        public HttpContext HttpContext => base.Context;

        /// <summary>
        /// The <see cref="IHtmlHelper"/> isn't normally available in the page, so get one
        /// and contextualize it from the service provider.
        /// </summary>
        /// <returns>The <see cref="IHtmlHelper"/>.</returns>
        protected IHtmlHelper<TModel> GetHtmlHelper()
        {
            if (_htmlHelper is null)
            {
                IServiceProvider serviceProvider = ViewData[ViewDataKeys.StatiqServiceProvider] as IServiceProvider;
                _htmlHelper = serviceProvider.GetRequiredService<IHtmlHelper<TModel>>();
                (_htmlHelper as IViewContextAware)?.Contextualize(ViewContext);
            }
            return _htmlHelper;
        }

        /// <summary>
        /// Renders the content of the specified section if it's defined.
        /// If the section is not defined, renders the provided content.
        /// </summary>
        /// <param name="name">The name of the section to render.</param>
        /// <param name="defaultContents">The default content to render if the section is not defined.</param>
        /// <returns>The HTML content.</returns>
        public IHtmlContent RenderSection(string name, Func<dynamic, HelperResult> defaultContents)
        {
            if (IsSectionDefined(name))
            {
                return RenderSection(name);
            }
            return defaultContents(null);
        }

        /// <summary>
        /// Renders the content of the specified section if it's defined.
        /// If the section is not defined, renders the provided partial.
        /// </summary>
        /// <param name="sectionName">The name of the section to render.</param>
        /// <param name="partialName">The name of the partial to render.</param>
        /// <returns>The HTML content.</returns>
        public IHtmlContent RenderSectionOrPartial(string sectionName, string partialName) =>
            IsSectionDefined(sectionName) ? RenderSection(sectionName) : GetHtmlHelper().Partial(partialName);
    }
}