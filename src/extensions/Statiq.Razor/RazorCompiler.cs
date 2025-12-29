using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ConcurrentCollections;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Hosting;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// Holds references to Razor objects based on the compilation parameters. This ensures the compilation cache and other
    /// service objects are persisted from one generation to the next, given the same compilation parameters.
    /// </summary>
    /// <remarks>
    /// This holds on to a service scope, so it needs to be disposed when we're done.
    /// </remarks>
    internal class RazorCompiler : CachingCompiler, IDisposable
    {
        private const string ViewStartFileName = "_ViewStart.cshtml";

        private readonly RazorProjectEngine _projectEngine;

        private readonly IServiceScope _compilerServiceScope;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
        /// Creates a Razor compiler using an existing set of services, which must already have Razor services registered using
        /// <see cref="IServiceCollectionExtensions.AddRazor(IServiceCollection, IReadOnlyFileSystem)"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider to use.</param>
        public RazorCompiler(IServiceProvider serviceProvider)
        {
            serviceProvider.ThrowIfNull(nameof(serviceProvider));

            // Create a service provider scope to get a distinct project engine
            IServiceScopeFactory serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            _compilerServiceScope = serviceScopeFactory.CreateScope();
            _projectEngine = _compilerServiceScope.ServiceProvider.GetRequiredService<RazorProjectEngine>();

            // Create an inner service scope factory so that each rendering can use a separate scope
            _serviceScopeFactory = _compilerServiceScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        public void Dispose()
        {
            _compilerServiceScope.Dispose();
        }

        // We need to initialize lazily since restoring from the cache won't have the actual namespaces, only a cache code
        public void EnsurePhases(CompilationParameters parameters, string[] namespaces) => EnsurePhases(_projectEngine, namespaces, parameters);

        public async Task RenderPageAsync(RenderRequest request)
        {
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;
                IRazorPage page = await GetPageFromStreamAsync(serviceProvider, request);
                IView view = GetViewFromStream(serviceProvider, request, page);

                using (StreamWriter writer = request.Output.GetWriter())
                {
                    Microsoft.AspNetCore.Mvc.Rendering.ViewContext viewContext = GetViewContext(serviceProvider, request, view, writer);
                    await viewContext.View.RenderAsync(viewContext);
                }
            }
        }

        private ViewContext GetViewContext(IServiceProvider serviceProvider, RenderRequest request, IView view, TextWriter output)
        {
            HttpContext httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider
            };

            ActionContext actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            ViewDataDictionary viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), actionContext.ModelState)
            {
                Model = request.Model
            };

            if (request.ViewData != null)
            {
                foreach (KeyValuePair<string, object> pair in request.ViewData)
                {
                    viewData.Add(pair);
                }
            }

            ITempDataDictionary tempData = new TempDataDictionary(actionContext.HttpContext, serviceProvider.GetRequiredService<ITempDataProvider>());

            return new ViewContext(
                actionContext,
                view,
                viewData,
                tempData,
                output,
                new HtmlHelperOptions(),
                request.Document,
                request.Context,
                serviceProvider);
        }

        /// <summary>
        /// Gets the view for an input document (which is different than the view for a layout, partial, or
        /// other indirect view because it's not necessarily on disk or in the file system).
        /// </summary>
        private IView GetViewFromStream(IServiceProvider serviceProvider, RenderRequest request, IRazorPage page)
        {
            StatiqRazorProjectFileSystem projectFileSystem = serviceProvider.GetRequiredService<StatiqRazorProjectFileSystem>();

            IEnumerable<string> viewStartLocations = request.ViewStartLocation is object
                ? new[] { request.ViewStartLocation }
                : projectFileSystem.FindHierarchicalItems(request.RelativePath, ViewStartFileName).Select(x => x.FilePath);

            List<IRazorPage> viewStartPages = viewStartLocations
                .Select(serviceProvider.GetRequiredService<IRazorPageFactoryProvider>().CreateFactory)
                .Where(x => x.Success)
                .Select(x => x.RazorPageFactory())
                .Reverse()
                .ToList();

            if (request.LayoutLocation is object)
            {
                page.Layout = request.LayoutLocation;
            }

            IRazorViewEngine viewEngine = serviceProvider.GetRequiredService<IRazorViewEngine>();
            IRazorPageActivator pageActivator = serviceProvider.GetRequiredService<IRazorPageActivator>();
            HtmlEncoder htmlEncoder = serviceProvider.GetRequiredService<HtmlEncoder>();
            DiagnosticListener diagnosticListener = serviceProvider.GetRequiredService<DiagnosticListener>();

            return new RazorView(viewEngine, pageActivator, viewStartPages, page, htmlEncoder, diagnosticListener);
        }

        /// <summary>
        /// Gets the Razor page for an input document stream. This is roughly modeled on
        /// DefaultRazorPageFactory and CompilerCache. Note that we don't actually bother
        /// with caching the page if it's from a live stream.
        /// </summary>
        private async Task<IRazorPage> GetPageFromStreamAsync(IServiceProvider serviceProvider, RenderRequest request)
        {
            string relativePath = request.RelativePath;

            if (relativePath.StartsWith("~/", StringComparison.Ordinal))
            {
                // For tilde slash paths, drop the leading ~ to make it work with the underlying IFileProvider.
                relativePath = relativePath.Substring(1);
            }

            // Get the file info by combining the stream content with info found at the document's original location (if any)
            StatiqRazorProjectFileSystem projectFileSystem = serviceProvider.GetRequiredService<StatiqRazorProjectFileSystem>();
            RazorProjectItem projectItem = projectFileSystem.GetItem(relativePath, request.Document);

            // Compute a hash for the content since pipelines could have changed it from the underlying file
            int contentCacheCode = await request.Document.ContentProvider.GetCacheCodeAsync();

            CompilationResult compilationResult = CompilePage(request, contentCacheCode, projectItem);
            return compilationResult.GetPage(request.RelativePath);
        }

        /// <summary>
        /// Checks the cache for a matching compilation and then compiles and loads the dynamic assembly if a miss.
        /// </summary>
        private CompilationResult CompilePage(RenderRequest request, int contentCacheCode, RazorProjectItem projectItem)
        {
            CompilerCacheKey compilerCacheKey = CompilerCacheKey.Get(request, contentCacheCode);
            return GetOrAddCachedCompilation(compilerCacheKey, _ => GetCompilation(projectItem));
        }

        /// <summary>
        /// Gets the assembly and loads it for a compilation cache miss.
        /// </summary>
        private CompilationResult GetCompilation(RazorProjectItem projectItem)
        {
            IExecutionContext.Current.LogDebug($"Compiling " + projectItem.FilePath);
            using (IServiceScope scope = _serviceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;

                // See RazorViewCompiler.CompileAndEmit()
                RazorCodeDocument codeDocument = _projectEngine.Process(projectItem);
                RazorCSharpDocument cSharpDocument = codeDocument.GetCSharpDocument();
                if (cSharpDocument.Diagnostics.Count > 0)
                {
                    throw StatiqViewCompiler.CreateCompilationFailedExceptionFromRazor(codeDocument, cSharpDocument.Diagnostics);
                }

                // Use the RazorViewCompiler to finish compiling the view for consistency with layouts
                StatiqViewCompiler viewCompiler = (StatiqViewCompiler)serviceProvider.GetRequiredService<IViewCompilerProvider>();
                return viewCompiler.CompileAndEmit(codeDocument, cSharpDocument.GeneratedCode);
            }
        }
    }
}