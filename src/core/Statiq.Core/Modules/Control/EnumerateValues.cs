using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Clones a new document for each item in a given metadata value.
    /// </summary>
    /// <remarks>
    /// Enumerates values returned by a config and clones a document for each one.
    /// The current value for each cloned document is contained in <see cref="Keys.Current"/>.
    /// Generally speaking you shouldn't enumerate documents since they get cloned as
    /// a pipeline progresses. Instead you should enumerate some identifier and operate on that
    /// to find the correct documents when the template gets rendered.
    /// </remarks>
    /// <category name="Control" />
    public class EnumerateValues : ParallelConfigModule<IEnumerable<object>>
    {
        private string _currentKey = Keys.Current;
        private Config<bool> _withInput = Config.FromDocument<bool>(Keys.EnumerateWithInput);

        /// <summary>
        /// Enumerates the values returned by the config and clones a document for each one.
        /// If the config returns null for a given document, the original input document will
        /// be output. If the config returns an empty enumerable for a given document, no result
        /// documents will be output for that input document.
        /// </summary>
        /// <param name="values">A delegate that returns the values to enumerate.</param>
        public EnumerateValues(Config<IEnumerable<object>> values)
            : base(values, false)
        {
        }

        /// <summary>
        /// Enumerates the values stored in the given metadata key.
        /// </summary>
        /// <param name="key">The metadata key that contains the values to enumerate.</param>
        public EnumerateValues(Config<string> key)
            : this(key.Transform((keyValue, doc, _) => doc.GetList<object>(keyValue)))
        {
            key.ThrowIfNull(nameof(key));
        }

        /// <summary>
        /// Enumerates the values stored in the <see cref="Keys.Enumerate"/> metadata key.
        /// </summary>
        public EnumerateValues()
            : this(Keys.Enumerate)
        {
        }

        /// <summary>
        /// Sets the metadata key to use for storing the current enumerated value in cloned documents.
        /// This is <see cref="Keys.Current"/> by default.
        /// </summary>
        /// <param name="currentKey">The metadata key to set for the current value.</param>
        /// <returns>The current module instance.</returns>
        public EnumerateValues WithCurrentKey(string currentKey)
        {
            _currentKey = currentKey.ThrowIfNull(nameof(currentKey));
            return this;
        }

        /// <summary>
        /// If the configuration delegate returns <c>true</c> the original input document will be
        /// included as the first result. By default, <see cref="Keys.EnumerateWithInput"/> is used.
        /// </summary>
        /// <param name="withInput">A configuration delegate that should return <c>true</c> to include the original document.</param>
        /// <returns>The current module instance.</returns>
        public EnumerateValues WithInputDocument(Config<bool> withInput)
        {
            _withInput = withInput.ThrowIfNull(nameof(withInput));
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IEnumerable<object> value)
        {
            // Return the input document(s) for empty values
            if (value is null)
            {
                if (input is null && !context.Inputs.IsEmpty)
                {
                    return context.Inputs;
                }
                return input.Yield();
            }

            // If the config value didn't require a document but we had inputs, clone each one with the enumeration results
            if (input is null && !context.Inputs.IsEmpty)
            {
                return await context.Inputs.ParallelSelectManyAsync(async i =>
                    (await _withInput.GetValueAsync(i, context) ? new[] { i } : new IDocument[] { })
                        .Concat(
                            value.Select(x => i.Clone(new MetadataItems
                            {
                                { _currentKey, x }
                            }))));
            }

            // Otherwise clone the input document from which these values came with each value
            return (input is object && await _withInput.GetValueAsync(input, context) ? new[] { input } : new IDocument[] { })
                .Concat(
                    value.Select(x => context.CloneOrCreateDocument(input, new MetadataItems
                    {
                        { _currentKey, x }
                    })));
        }
    }
}