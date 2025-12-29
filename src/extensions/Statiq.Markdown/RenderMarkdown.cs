using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Markdig;
using Markdig.Helpers;
using Markdig.Syntax;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Markdown.EscapeAt;

namespace Statiq.Markdown
{
    /// <summary>
    /// Parses markdown content and renders it to HTML.
    /// </summary>
    /// <remarks>
    /// Parses markdown content in each input document and outputs documents with rendered HTML content.
    /// Note that <c>@</c> (at) symbols will be automatically HTML escaped for better compatibility with downstream
    /// Razor modules. If you want to include a raw <c>@</c> symbol when <c>EscapeAt()</c> is <c>true</c>, use
    /// <c>\@</c>. Use the <c>EscapeAt()</c> fluent method to modify this behavior.
    /// </remarks>
    /// <metadata cref="MarkdownKeys.MarkdownExtensions" usage="Input" />
    /// <category name="Templates" />
    public class RenderMarkdown : ParallelModule
    {
        // Used to cache extensions from the metadata key
        private readonly ConcurrentCache<string, (Type, IMarkdownExtension)> _extensionCache =
            new ConcurrentCache<string, (Type, IMarkdownExtension)>(false, StringComparer.OrdinalIgnoreCase);

        private readonly string _sourceKey;
        private readonly string _destinationKey;
        private readonly OrderedList<IMarkdownExtension> _extensions = new OrderedList<IMarkdownExtension>();
        private string _configuration = MarkdownHelper.DefaultConfiguration;
        private bool _escapeAt = true;
        private bool _prependLinkRoot = false;
        private bool _passThroughRawFence = true;
        private string _markdownDocumentKey = nameof(MarkdownDocument);

        /// <summary>
        /// Processes Markdown in the content of the document.
        /// </summary>
        public RenderMarkdown()
        {
        }

        /// <summary>
        /// Processes Markdown in the metadata of the document.
        /// </summary>
        /// <param name="sourceKey">The metadata key of the Markdown to process.</param>
        /// <param name="destinationKey">
        /// The metadata key to store the rendered HTML (if null, it gets placed back in the source metadata key).
        /// </param>
        public RenderMarkdown(string sourceKey, string destinationKey = null)
        {
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
        }

        /// <summary>
        /// Specifies whether code fences with a language of "raw" (I.e. <c>```raw</c>) should
        /// be passed through verbatim (the default is <c>true</c>).
        /// </summary>
        /// <param name="passThroughRawFence">
        /// If set to <c>true</c>, code fences with the language "raw" are passed through verbatim.
        /// </param>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown PassThroughRawFence(bool passThroughRawFence = true)
        {
            _passThroughRawFence = passThroughRawFence;
            return this;
        }

        /// <summary>
        /// Specifies whether the <c>@</c> symbol should be escaped (the default is <c>true</c>).
        /// This is important if the Markdown documents are going to be passed to the Razor module,
        /// otherwise the Razor processor will interpret the unescaped <c>@</c> symbols as code
        /// directives.
        /// If you want to include a raw <c>@</c> symbol when <c>EscapeAt()</c> is <c>true</c>, use <c>\@</c>.
        /// </summary>
        /// <param name="escapeAt">If set to <c>true</c>, <c>@</c> symbols are HTML escaped.</param>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown EscapeAt(bool escapeAt = true)
        {
            _escapeAt = escapeAt;
            return this;
        }

        /// <summary>
        /// Includes a set of useful advanced extensions, e.g., citations, footers, footnotes, math,
        /// grid-tables, pipe-tables, and tasks, in the Markdown processing pipeline.
        /// </summary>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown UseExtensions()
        {
            _configuration = "advanced";
            return this;
        }

        /// <summary>
        /// Includes a set of extensions defined as a string, e.g., "pipetables", "citations",
        /// "mathematics", or "abbreviations". Separate different extensions with a '+'.
        /// </summary>
        /// <param name="extensions">The extensions string.</param>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown UseConfiguration(string extensions)
        {
            _configuration = extensions;
            return this;
        }

        /// <summary>
        /// Includes a custom extension in the markdown processing given by a class implementing
        /// the IMarkdownExtension interface.
        /// </summary>
        /// <typeparam name="TExtension">The type of the extension to use.</typeparam>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown UseExtension<TExtension>()
            where TExtension : class, IMarkdownExtension, new()
        {
            _extensions.AddIfNotAlready<TExtension>();
            return this;
        }

        /// <summary>
        /// Includes a custom extension in the markdown processing given by a object implementing
        /// the IMarkdownExtension interface.
        /// </summary>
        /// <param name="extension">A object that that implement <see cref="IMarkdownExtension"/>.</param>
        /// <typeparam name="TExtension">The type of the extension to use.</typeparam>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown UseExtension<TExtension>(TExtension extension)
            where TExtension : IMarkdownExtension
        {
            if (extension is object)
            {
                _extensions.AddIfNotAlready(extension);
            }

            return this;
        }

        /// <summary>
        /// Includes multiple custom extension in the markdown processing given by classes implementing
        /// the <see cref="IMarkdownExtension"/> interface.
        /// </summary>
        /// <param name="extensions">A sequence of types that implement <see cref="IMarkdownExtension"/>.</param>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown UseExtensions(IEnumerable<Type> extensions)
        {
            if (extensions is null)
            {
                return this;
            }

            foreach (Type type in extensions)
            {
                IMarkdownExtension extension = Activator.CreateInstance(type) as IMarkdownExtension;
                if (extension is object)
                {
                    AddUntypedExtension(_extensions, type, extension);
                }
            }

            return this;
        }

        /// <summary>
        /// Specifies if the <see cref="Keys.LinkRoot"/> setting must be used to rewrite root-relative links when rendering markdown.
        /// By default, root-relative links, which are links starting with a '/' are left untouched.
        /// When setting this value to <c>true</c>, the <see cref="Keys.LinkRoot"/> setting value is added before the link.
        /// </summary>
        /// <param name="prependLinkRoot">If set to <c>true</c>, the <see cref="Keys.LinkRoot"/> setting value is added before any root-relative link (eg. stating with a '/').</param>
        /// <returns>The current module instance.</returns>
        [Obsolete("Use ~/ to prepend the link root instead. e.g. ~/foo/bar", false)]
        public RenderMarkdown PrependLinkRoot(bool prependLinkRoot = false)
        {
            _prependLinkRoot = prependLinkRoot;
            return this;
        }

        /// <summary>
        /// Specifies a metadata key where the <see cref="MarkdownDocument"/> should be saved (by default to <c>MarkdownDocument</c>).
        /// </summary>
        /// <param name="markdownDocumentKey">The metadata key or <c>null</c> to not save a <see cref="MarkdownDocument"/>.</param>
        /// <returns>The current module instance.</returns>
        public RenderMarkdown WithMarkdownDocumentKey(string markdownDocumentKey)
        {
            _markdownDocumentKey = markdownDocumentKey;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            context.LogDebug(
                   "Processing Markdown {0} for {1}",
                   string.IsNullOrEmpty(_sourceKey) ? string.Empty : ("in" + _sourceKey),
                   input.ToSafeDisplayString());

            // Get the content
            string content;
            if (_sourceKey.IsNullOrEmpty())
            {
                content = await input.GetContentStringAsync();
            }
            else if (input.ContainsKey(_sourceKey))
            {
                content = input.GetString(_sourceKey) ?? string.Empty;
            }
            else
            {
                // If the key doesn't exist, we're done
                return input.Yield();
            }

            OrderedList<IMarkdownExtension> extensions = _extensions;

            // Add extensions from the metadata key
            IReadOnlyList<string> extensionNames = input.GetList<string>(MarkdownKeys.MarkdownExtensions);
            if (extensionNames is object && extensionNames.Count > 0)
            {
                extensions = new OrderedList<IMarkdownExtension>(_extensions);
                foreach (string extensionName in extensionNames)
                {
                    (Type, IMarkdownExtension) extension = _extensionCache.GetOrAdd(
                        extensionName,
                        (key, classCatalog) =>
                        {
                            // Try to find it in the class catalog by exact name
                            Type extensionType = classCatalog
                                .FindTypes(key, StringComparison.OrdinalIgnoreCase)
                                .FirstOrDefault(x => typeof(IMarkdownExtension).IsAssignableFrom(x));

                            // Then try to find if by appending "Extension" (if not already)
                            if (extensionType is null && !key.EndsWith("Extension", StringComparison.OrdinalIgnoreCase))
                            {
                                extensionType = classCatalog
                                    .FindTypes(key + "Extension", StringComparison.OrdinalIgnoreCase)
                                    .FirstOrDefault(x => typeof(IMarkdownExtension).IsAssignableFrom(x));
                            }

                            if (extensionType is object)
                            {
                                ConstructorInfo[] constructors = extensionType.GetConstructors();
                                ParameterInfo[][] constructorParameters = constructors
                                    .Select(x => x.GetParameters())
                                    .OrderBy(x => x.Length)
                                    .ToArray();
                                if (constructorParameters.Any(x => x.Length == 0))
                                {
                                    return (extensionType, (IMarkdownExtension)Activator.CreateInstance(extensionType));
                                }

                                // That didn't work so try to create it using default arguments
                                ParameterInfo[] usableParameters = constructorParameters
                                    .FirstOrDefault(x => x.All(p => p.HasDefaultValue));
                                if (usableParameters is object)
                                {
                                    return (
                                        extensionType,
                                        (IMarkdownExtension)Activator.CreateInstance(
                                            extensionType, usableParameters.Select(p => p.DefaultValue).ToArray()));
                                }

                                throw new Exception($"Markdown extension {extensionName} does not have a usable constructor");
                            }

                            throw new Exception($"Could not find Markdown extension {extensionName}");
                        },
                        context.ClassCatalog);
                    AddUntypedExtension(extensions, extension.Item1, extension.Item2);
                }
            }

            // Add the @ escaping extension if escaping @ symbols
            bool escapeAt = input.ContainsKey(MarkdownKeys.EscapeAtInMarkdown)
                ? input.GetBool(MarkdownKeys.EscapeAtInMarkdown)
                : _escapeAt;
            if (escapeAt)
            {
                if (extensions == _extensions)
                {
                    extensions = new OrderedList<IMarkdownExtension>(_extensions.Concat(new EscapeAtExtension()));
                }
                else
                {
                    extensions.Add(new EscapeAtExtension());
                }
            }

            // Render the Markdown
            string result;
            MarkdownDocument markdownDocument;
            using (TextWriter writer = new StringWriter())
            {
                markdownDocument = MarkdownHelper.RenderMarkdown(
                    context,
                    input,
                    content,
                    writer,
                    _prependLinkRoot,
                    _passThroughRawFence,
                    escapeAt,
                    _configuration,
                    extensions);
                if (markdownDocument is null)
                {
                    return input.Yield();
                }
                result = writer.ToString();
            }

            MetadataItems metadataItems = new MetadataItems();
            if (!_markdownDocumentKey.IsNullOrEmpty())
            {
                metadataItems.Add(_markdownDocumentKey, markdownDocument);
            }

            if (_sourceKey.IsNullOrEmpty())
            {
                // No source key so change the content
                return input
                    .Clone(metadataItems, context.GetContentProvider(result, MediaTypes.Html))
                    .Yield();
            }
            else
            {
                // Markdown came from metadata so don't change content
                metadataItems.Add(string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result);
                return input
                    .Clone(metadataItems)
                    .Yield();
            }
        }

        private static void AddUntypedExtension(
            OrderedList<IMarkdownExtension> extensions,
            Type extensionType,
            IMarkdownExtension extension)
        {
            // Need - public void AddIfNotAlready<TElement>(TElement element) where TElement : T;
            // Kind of hackish, but no other way to preserve types.
            MethodInfo addIfNotAlready = typeof(OrderedList<IMarkdownExtension>).GetMethods()
                .Where(x => x.IsGenericMethod && x.Name == nameof(OrderedList<IMarkdownExtension>.AddIfNotAlready) && x.GetParameters().Length == 1)
                .Select(x => x.MakeGenericMethod(extensionType))
                .Single();
            addIfNotAlready.Invoke(extensions, new object[] { extension });
        }
    }
}