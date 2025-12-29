using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Handlebars
{
    /// <summary>
    /// Parses, compiles, and renders Handlebars templates.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Handlebars provides the power necessary to let you build semantic templates effectively with no frustration.
    /// See <a href="https://handlebarsjs.com/guide/">this guide</a> for an introduction to Handlebars.
    /// </para>
    /// <para>
    /// This module user <a href="https://github.com/rexm/Handlebars.Net">Handlebars.Net</a> to render Handlebars templates.
    /// Handlebars.Net doesn't use a scripting engine to run a Javascript library - it compiles Handlebars templates directly to IL bytecode.
    /// It also mimics the JS library's API as closely as possible.
    /// </para>
    /// </remarks>
    /// <category name="Templates" />
    public class RenderHandlebars : ParallelModule
    {
        private readonly string _sourceKey;
        private readonly string _destinationKey;
        private readonly IDictionary<string, Config<string>> _partials;
        private readonly IDictionary<string, Config<HandlebarsHelper>> _helpers;
        private readonly IDictionary<string, Config<HandlebarsBlockHelper>> _blockHelpers;

        private Config<object> _model;
        private Func<IExecutionContext, IDocument, IHandlebars, Task> _configure;

        /// <summary>
        /// Parses Handlebars templates in each input document and outputs documents with rendered content.
        /// </summary>
        public RenderHandlebars()
        {
            _partials = new Dictionary<string, Config<string>>(StringComparer.Ordinal);
            _helpers = new Dictionary<string, Config<HandlebarsHelper>>(StringComparer.Ordinal);
            _blockHelpers = new Dictionary<string, Config<HandlebarsBlockHelper>>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Parses Handlebars templates in the metadata of each input document and outputs documents with metadata containing the rendered content.
        /// </summary>
        /// /// <param name="sourceKey">The metadata key of the Handlebars template to process.</param>
        /// <param name="destinationKey">The metadata key to store the rendered content (if null, it gets placed back in the source metadata key).</param>
        public RenderHandlebars(string sourceKey, string destinationKey = null)
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
        public RenderHandlebars WithModel(Config<object> model)
        {
            _model = model;
            return this;
        }

        /// <summary>
        /// Specifies a partial template to be registered for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="name">The name of the partial template.</param>
        /// <param name="partial">A delegate that returns the partial template.</param>
        /// <returns>The current module instance.</returns>
        public RenderHandlebars WithPartial(string name, Config<string> partial)
        {
            _partials[name] = partial;
            return this;
        }

        /// <summary>
        /// Specifies a helper to be registered for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="name">The name of the helper.</param>
        /// <param name="helper">A delegate that returns the helper.</param>
        /// <returns>The current module instance.</returns>
        public RenderHandlebars WithHelper(string name, Config<HandlebarsHelper> helper)
        {
            _helpers[name] = helper;
            return this;
        }

        /// <summary>
        /// Specifies a block helper to be registered for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="name">The name of the block helper.</param>
        /// <param name="blockHelper">A delegate that returns the block helper.</param>
        /// <returns>The current module instance.</returns>
        public RenderHandlebars WithBlockHelper(string name, Config<HandlebarsBlockHelper> blockHelper)
        {
            _blockHelpers[name] = blockHelper;
            return this;
        }

        /// <summary>
        /// Specifies an extension point to configure the handlebars environment for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="configure">A delegate for configuring the handlebars environment.</param>
        /// <returns>The current module instance.</returns>
        public RenderHandlebars Configure(
            Action<IExecutionContext, IDocument, IHandlebars> configure)
        {
            Configure((context, document, configuration) =>
            {
                configure(context, document, configuration);
                return Task.CompletedTask;
            });

            return this;
        }

        /// <summary>
        /// Specifies an extension point to configure the handlebars environment for each page based on the current input
        /// document and context.
        /// </summary>
        /// <param name="configure">A delegate for configuring the handlebars environment.</param>
        /// <returns>The current module instance.</returns>
        public RenderHandlebars Configure(
            Func<IExecutionContext, IDocument, IHandlebars, Task> configure)
        {
            _configure = configure;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            context.LogDebug(
                   "Processing Handlebars {0} for {1}",
                   string.IsNullOrEmpty(_sourceKey) ? string.Empty : ("in" + _sourceKey),
                   input.ToSafeDisplayString());

            string content;
            if (string.IsNullOrEmpty(_sourceKey))
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

            IHandlebars handlebars = HandlebarsDotNet.Handlebars.Create();

            // Configure
            if (_configure is object)
            {
                await _configure(context, input, handlebars);
            }

            // Register partials
            foreach ((string name, Config<string> partial) in _partials)
            {
                handlebars.RegisterTemplate(name, await partial.GetValueAsync(input, context));
            }

            // Register helpers
            foreach ((string name, Config<HandlebarsHelper> helper) in _helpers)
            {
                handlebars.RegisterHelper(name, await helper.GetValueAsync(input, context));
            }

            // Register block helpers
            foreach ((string name, Config<HandlebarsBlockHelper> blockHelper) in _blockHelpers)
            {
                handlebars.RegisterHelper(name, await blockHelper.GetValueAsync(input, context));
            }

            string result = handlebars.Compile(content)(_model is null
                ? input.AsDynamic()
                : await _model.GetValueAsync(input, context));

            return string.IsNullOrEmpty(_sourceKey)
                ? input.Clone(context.GetContentProvider(result, MediaTypes.Html)).Yield()
                : input
                    .Clone(new MetadataItems
                    {
                        { string.IsNullOrEmpty(_destinationKey) ? _sourceKey : _destinationKey, result }
                    })
                    .Yield();
        }
    }
}