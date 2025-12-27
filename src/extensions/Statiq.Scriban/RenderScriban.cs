using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    /// <summary>
    /// Parses, compiles, and renders Scriban and Liquid templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Scriban is a fast, powerful, safe and lightweight text templating language and engine for .NET, with a compatibility mode for parsing Liquid templates.
    /// See <a href="https://github.com/lunet-io/scriban/blob/master/doc/language.md">this guide</a> for an introduction to Scriban.
    /// See <a href="https://shopify.github.io/liquid/">this guide</a> for an introduction to Liquid.
    /// </para>
    /// <para>
    /// This module user <a href="https://github.com/lunet-io/scriban">Scriban</a> to render Scriban and liquid templates.
    /// </para>
    /// </remarks>
    /// <category name="Templates" />
    public class RenderScriban : ParallelModule
    {
        private readonly string _sourceKey;
        private readonly string _destinationKey;

        private Config<object> _model;
        private MemberRenamerDelegate _renamer;
        private ParserOptions _parserOptions;
        private LexerOptions _lexerOptions;

        /// <summary>
        /// Parses Scriban templates in each input document and outputs documents with rendered content.
        /// </summary>
        public RenderScriban()
        {
            _lexerOptions = LexerOptions.Default;
        }

        /// <summary>
        /// Parses Scriban templates in the metadata of each input document and outputs documents with metadata containing the rendered content.
        /// </summary>
        /// /// <param name="sourceKey">The metadata key of the Scriban template to process.</param>
        /// <param name="destinationKey">The metadata key to store the rendered content (if null, it gets placed back in the source metadata key).</param>
        public RenderScriban(string sourceKey, string destinationKey = null)
            : this()
        {
            _sourceKey = sourceKey;
            _destinationKey = destinationKey;
        }

        /// <summary>
        /// Specifies a model to use for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="model">A delegate that returns the model.</param>
        /// <returns>The current module instance.</returns>
        public RenderScriban WithModel(Config<object> model)
        {
            _model = model;
            return this;
        }

        /// <summary>
        /// Specifies the member renamer used for renaming document metadata keys,
        /// properties and methods when used in Scriban templates. The standard Scriban
        /// member renamer will make a camel/pascalcase name changed by `_` and lowercase.
        /// e.g `ThisIsAnExample` becomes `this_is_an_example`.
        /// </summary>
        /// <param name="renamer">A delegate that returns the new name of a member.</param>
        /// <returns>The current module instance.</returns>
        public RenderScriban WithMemberRenamer(MemberRenamerDelegate renamer)
        {
            _renamer = renamer;
            return this;
        }

        /// <summary>
        /// Specifies the options used when parsing Scriban templates.
        /// </summary>
        /// <param name="parserOptions">The parsing options.</param>
        /// <returns>The current module instance.</returns>
        public RenderScriban WithParserOptions(ParserOptions parserOptions)
        {
            _parserOptions = parserOptions;
            return this;
        }

        /// <summary>
        /// Specifies the options passed to the lexer.
        /// </summary>
        /// <param name="lexerOptions">The lexer options.</param>
        /// <returns>The current module instance.</returns>
        public RenderScriban WithLexerOptions(LexerOptions lexerOptions)
        {
            _lexerOptions = lexerOptions;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            context.LogDebug(
                   "Processing Scriban {0} for {1}",
                   _sourceKey.IsNullOrEmpty() ? string.Empty : ("in" + _sourceKey),
                   input.ToSafeDisplayString());

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
                // Don't do anything if the key doesn't exist
                return input.Yield();
            }

            _renamer ??= StandardMemberRenamer.Default;

            Template template = Template.Parse(content, input.Source.FullPath, _parserOptions, _lexerOptions);

            if (template.HasErrors)
            {
                throw new InvalidOperationException(
                    $"Errors while parsing template.\n{string.Join("\n", template.Messages.Select(x => x.ToString()))}");
            }

            foreach (LogMessage message in template.Messages)
            {
                switch (message.Type)
                {
                    case ParserMessageType.Error:
                        context.LogError(message.ToString());
                        break;
                    case ParserMessageType.Warning:
                        context.LogWarning(message.ToString());
                        break;
                    default:
                        context.LogInformation(message.ToString());
                        break;
                }
            }

            IScriptObject scriptObject;

            if (_model is null)
            {
                scriptObject = new StatiqScriptObject(input, _renamer);
            }
            else
            {
                object model = await _model.GetValueAsync(input, context);

                if (model is IDocument documentModel)
                {
                    scriptObject = new StatiqScriptObject(documentModel, _renamer);
                }
                else
                {
                    scriptObject = new global::Scriban.Runtime.ScriptObject();

                    if (model is object)
                    {
                        scriptObject.Import(model, filter: null, renamer: _renamer);
                    }
                }
            }

            // TODO: Expose member filter
            TemplateContext templateContext = new StatiqTemplateContext
            {
                TemplateLoader = new TemplateLoader(context.FileSystem),
                MemberRenamer = _renamer,
                TemplateLoaderLexerOptions = _lexerOptions,
                TemplateLoaderParserOptions = _parserOptions,
            };
            templateContext.PushGlobal(scriptObject);

            string result = await template.RenderAsync(templateContext);

            return _sourceKey.IsNullOrEmpty()
                ? input.Clone(context.GetContentProvider(result, MediaTypes.Html)).Yield()
                : input
                    .Clone(new MetadataItems
                    {
                        { _destinationKey.IsNullOrEmpty() ? _sourceKey : _destinationKey, result }
                    })
                    .Yield();
        }
    }
}