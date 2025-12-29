using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Gets documents from an arbitrary config value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This module is very useful for customizing pipeline execution without having to write an entire module.
    /// Returning modules from the config value is also useful for customizing existing modules based on the
    /// current set of documents.
    /// </para>
    /// <para>
    /// If the value is <c>null</c> this module will output the original input document(s).
    /// If the value is a <see cref="IDocument"/>, <see cref="IEnumerable{IDocument}"/>, or
    /// <see cref="IAsyncEnumerable{IDocument}"/>, the document(s) will be the output(s) of this module.
    /// If the value is a <see cref="IEnumerable{IModule}"/> or <see cref="IModule"/>,
    /// the module(s) will be executed with the input document(s) as their input
    /// and the results will be the output(s) of this module.
    /// If the value is an <see cref="IContentProvider"/>,
    /// <see cref="IContentProviderFactory"/>, or <see cref="Stream"/> it will be used to get new content for the
    /// provided document (or create a new document if the provided document is <c>null</c>).
    /// If config value is anything else, the content of the input document will be
    /// changed to (or a new document created with) the string value.
    /// </para>
    /// <para>
    /// If the provided config value does not require a document (for example, it's created with the
    /// <see cref="Config.FromSetting(string, object)"/> factory method) then it will be invoked once for all input documents.
    /// If the provided config value does require a document (for example, it's created with the
    /// <see cref="Config.FromDocument(string, object)"/> factory method) then if will be invoked once for each input document.
    /// </para>
    /// </remarks>
    /// <category name="Extensibility" />
    public class ExecuteConfig : ParallelConfigModule<object>
    {
        public ExecuteConfig(Config<object> config)
            : base(config, false)
        {
        }

        protected override Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, object value)
        {
            IEnumerable<IDocument> inputs = context.Inputs;
            if (input is object)
            {
                inputs = input.Yield();
            }

            // This behavior is important because action-based config values always return null, so by returning
            // the input(s) when the value is null it ensures actions will execute without affecting the input documents
            if (value is null)
            {
                return Task.FromResult(inputs);
            }

            return context.CloneOrCreateDocumentsAsync(input, inputs, value);
        }
    }
}