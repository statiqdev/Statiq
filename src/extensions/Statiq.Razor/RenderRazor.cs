using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// Parses, compiles, and renders Razor templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Razor is the template language used by ASP.NET MVC. This module can parse and compile Razor
    /// templates and then render them to HTML. While a bit
    /// outdated, <a href="http://haacked.com/archive/2011/01/06/razor-syntax-quick-reference.aspx/">this guide</a>
    /// is a good quick reference for the Razor language syntax. This module uses the Razor engine from ASP.NET Core.
    /// </para>
    /// <para>
    /// Whenever possible, the same conventions as the Razor engine in ASP.NET MVC were used. It's
    /// important to keep in mind however, that this is <em>not</em> ASP.NET MVC. Many features you may
    /// be used to will not work (like most of the <c>HtmlHelper</c> extensions) and others just don't
    /// make sense (like the concept of <em>actions</em> and <em>controllers</em>). Also, while property names and
    /// classes in the two engines have similar names(such as <c>HtmlHelper</c>) they are not the same,
    /// and code intended to extend the capabilities of Razor in ASP.NET MVC probably won't work.
    /// That said, a lot of functionality does function the same as it does in ASP.NET MVC.
    /// </para>
    /// <para>
    /// This module requires Razor services to be registered before use with
    /// <see cref="IServiceCollectionExtensions.AddRazor(IServiceCollection, IReadOnlyFileSystem)"/>.
    /// This is done automatically when using Statiq.App and the Bootstrapper.
    /// </para>
    /// </remarks>
    /// Used to determine if the source file name contains the ignore prefix.
    /// <category name="Templates" />
    public class RenderRazor : Module
    {
        // Not a valid file name on either Windows or Linux
        internal const string ViewStartPlaceholder = "\0";

        private readonly Type _basePageType;
        private Config<NormalizedPath> _viewStartPath;
        private Config<NormalizedPath> _layoutPath;
        private Config<object> _model;
        private IDictionary<string, Config<object>> _viewData = null;
        private string _ignorePrefix = "_";

        /// <summary>
        /// Parses Razor templates in each input document and outputs documents with rendered HTML content.
        /// If <c>basePageType</c> is specified, it will be used as the base type for Razor pages. The new base
        /// type must derive from <c>StatiqRazorPage&lt;TModel&gt;</c>.
        /// </summary>
        /// <param name="basePageType">Type of the base Razor page class, or <c>null</c> for the default base class.</param>
        public RenderRazor(Type basePageType = null)
        {
            if (basePageType is object && !IsSubclassOfRawGeneric(typeof(StatiqRazorPage<>), basePageType))
            {
                throw new ArgumentException($"The Razor base page type must derive from {nameof(StatiqRazorPage<object>)}.");
            }
            _basePageType = basePageType;
        }

        // From http://stackoverflow.com/a/457708/807064
        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck)
        {
            while (toCheck is object && toCheck != typeof(object))
            {
                Type current = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == current)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module. This
        /// lets you specify a different ViewStart file for each document. For example, you could return a
        /// ViewStart based on document location or document metadata. Returning <c>null</c> from the
        /// function reverts back to the default ViewStart search behavior for that document.
        /// </summary>
        /// <param name="path">A delegate that should return the ViewStart path as a <see cref="NormalizedPath"/>,
        /// or <c>null</c> for the default ViewStart search behavior.</param>
        /// <returns>The current module instance.</returns>
        public RenderRazor WithViewStart(Config<NormalizedPath> path)
        {
            _viewStartPath = path;
            return this;
        }

        /// <summary>
        /// Specifies an alternate ViewStart file to use for all Razor pages processed by this module. This
        /// lets you specify a different ViewStart file for each document. For example, you could return a
        /// ViewStart based on document location or document metadata. Returning <c>null</c> from the
        /// function reverts back to the default ViewStart search behavior for that document.
        /// </summary>
        /// <param name="path">A delegate that should return the ViewStart path as a path,
        /// or <c>null</c> for the default ViewStart search behavior.</param>
        /// <returns>The current module instance.</returns>
        public RenderRazor WithViewStart(Config<string> path)
        {
            _viewStartPath = path?.Transform(x => (NormalizedPath)x);
            return this;
        }

        /// <summary>
        /// Specifies a layout file to use for all Razor pages processed by this module. This
        /// lets you specify a different layout file for each document.
        /// </summary>
        /// <param name="path">A delegate that should return the layout path as a <see cref="NormalizedPath"/>.</param>
        /// <returns>The current module instance.</returns>
        public RenderRazor WithLayout(Config<NormalizedPath> path)
        {
            _layoutPath = path;
            return this;
        }

        /// <summary>
        /// Specifies a layout file to use for all Razor pages processed by this module. This
        /// lets you specify a different layout file for each document.
        /// </summary>
        /// <param name="path">A delegate that should return the layout path as a path.</param>
        /// <returns>The current module instance.</returns>
        public RenderRazor WithLayout(Config<string> path)
        {
            _layoutPath = path?.Transform(x => (NormalizedPath)x);
            return this;
        }

        /// <summary>
        /// Specifies a model to use for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="model">A delegate that returns the model.</param>
        /// <returns>The current module instance.</returns>
        public RenderRazor WithModel(Config<object> model)
        {
            _model = model;
            return this;
        }

        /// <summary>
        /// Specifies ViewData to use for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="key">The view data key.</param>
        /// <param name="model">A delegate that returns the model.</param>
        /// <returns>The current module instance.</returns>
        public RenderRazor WithViewData(string key, Config<object> model)
        {
            if (_viewData == null)
            {
                _viewData = new Dictionary<string, Config<object>>();
            }

            _viewData[key] = model;

            return this;
        }

        /// <summary>
        /// Specifies a file prefix to ignore. If a document has a metadata value for <c>SourceFileName</c> and
        /// that metadata value starts with the specified prefix, that document will not be processed or
        /// output by the module. By default, the Razor module ignores all documents prefixed with
        /// an underscore (_). Specifying <c>null</c> will result in no documents being ignored.
        /// </summary>
        /// <param name="prefix">The file prefix to ignore.</param>
        /// <returns>The current module instance.</returns>
        public RenderRazor IgnorePrefix(string prefix)
        {
            _ignorePrefix = prefix;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Get the Razor service from the service collection if it's in there
            RazorService razorService = context.GetRequiredService<RazorService>();

            // Eliminate input documents that we shouldn't process
            ImmutableArray<IDocument> validInputs = context.Inputs
                .Where(x => _ignorePrefix is null || x.Source.IsNull || !x.Source.FileName.FullPath.StartsWith(_ignorePrefix))
                .ToImmutableArray();

            if (validInputs.Length < context.Inputs.Length)
            {
                context.LogInformation($"Ignoring {context.Inputs.Length - validInputs.Length} inputs due to source file name prefix");
            }

            // Compile and evaluate the pages in parallel
            return await validInputs.ParallelSelectAsync(RenderDocumentAsync);

            async Task<IDocument> RenderDocumentAsync(IDocument input)
            {
                context.LogDebug("Processing Razor for {0}", input.ToSafeDisplayString());

                using (Stream contentStream = context.GetContentStream())
                {
                    NormalizedPath layoutPath = _layoutPath is null ? NormalizedPath.Null : await _layoutPath.GetValueAsync(input, context);
                    string layoutLocation = layoutPath.IsNull ? null : layoutPath.FullPath;

                    // We need to set a non-null ViewStart location if an explicit layout is provided but an explicit ViewStart is not (using a null char which is not a valid file name on Windows or Linux)
                    // otherwise the Razor engine will default to looking up the tree for a _ViewStart.cshtml that will take precedence over the explicit layout
                    // Note that this means in this special case, anything else the ViewStart file is doing will be ignored - this therefore favors correctness of the layout over ViewStart logic
                    NormalizedPath viewStartLocationPath = _viewStartPath is null ? null : await _viewStartPath.GetValueAsync(input, context);
                    string viewStartLocation = viewStartLocationPath.IsNull
                        ? (layoutLocation is object ? ViewStartPlaceholder + input.Source.FullPath + ViewStartPlaceholder : null)
                        : viewStartLocationPath.FullPath;

                    // Get the model
                    object model = _model is null ? input : ((await _model.GetValueAsync(input, context)) ?? input);

                    RenderRequest request = new RenderRequest
                    {
                        Output = contentStream,
                        BaseType = _basePageType, // null indicates the default base page type
                        Context = context,
                        Document = input,
                        LayoutLocation = layoutLocation,
                        ViewStartLocation = viewStartLocation,
                        RelativePath = GetRelativePath(input, context),
                        Model = model,
                        ViewData = await GetViewDataAsync(_viewData, input, context),
                    };

                    // Try to render the page
                    try
                    {
                        await razorService.RenderAsync(request);
                    }
                    catch (Exception ex)
                    {
                        throw input.LogAndWrapException(ex);
                    }

                    return input.Clone(context.GetContentProvider(contentStream, MediaTypes.Html));
                }
            }
        }

        private async Task<IEnumerable<KeyValuePair<string, object>>> GetViewDataAsync(IDictionary<string, Config<object>> viewData, IDocument input, IExecutionContext context)
        {
            if (viewData == null || !viewData.Any())
            {
                return null;
            }

            List<KeyValuePair<string, object>> requestViewData = new List<KeyValuePair<string, object>>();
            foreach (KeyValuePair<string, Config<object>> pair in viewData)
            {
                try
                {
                    object value = await pair.Value.GetValueAsync(input, context);
                    requestViewData.Add(new KeyValuePair<string, object>(pair.Key, value));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Failed to get ViewData value '{pair.Key}'", ex);
                }
            }

            return requestViewData;
        }

        private string GetRelativePath(IDocument document, IExecutionContext context)
        {
            // Use the pre-calculated relative file path if available
            NormalizedPath relativePath = document.Source.IsNull ? NormalizedPath.Null : document.Source.GetRelativeInputPath();
            return relativePath.IsNull ? GetRelativePath(document.Source, context) : $"/{relativePath.FullPath}";
        }

        private string GetRelativePath(NormalizedPath path, IExecutionContext context)
        {
            // Calculate a relative path from the input path(s) (or root) to the provided path
            if (!path.IsNull)
            {
                NormalizedPath inputPath = context.FileSystem.GetContainingInputPath(path);
                if (inputPath.IsNull)
                {
                    inputPath = NormalizedPath.AbsoluteRoot;
                }
                if (path.IsRelative)
                {
                    // If the path is relative, combine it with the input path to make it absolute
                    path = inputPath.Combine(path);
                }
                return $"/{inputPath.GetRelativePath(path).FullPath}";
            }

            // If there's no path, give this document a placeholder name
            return $"/{Path.GetRandomFileName()}.cshtml";
        }
    }
}