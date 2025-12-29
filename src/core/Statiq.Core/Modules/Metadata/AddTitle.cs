using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Sets a title metadata key for documents based on their file path or source.
    /// </summary>
    /// <remarks>
    /// This will split the title at special characters, capitalize first letters, remove extensions, etc.
    /// </remarks>
    /// <metadata cref="Keys.Title" usage="Output" />
    /// <category name="Metadata" />
    public class AddTitle : ParallelModule
    {
        private readonly Config<string> _title = Config.FromDocument(doc => doc.GetTitle());
        private string _key = Keys.Title;
        private bool _keepExisting = true;

        /// <summary>
        /// This will use the existing title metadata key if one exists,
        /// otherwise it will set a title based on the document source.
        /// </summary>
        public AddTitle()
        {
        }

        /// <summary>
        /// This sets the title of all input documents to a value from the delegate.
        /// </summary>
        /// <param name="title">A delegate that must return a string.</param>
        public AddTitle(Config<string> title)
        {
            _title = title.ThrowIfNull(nameof(title));
        }

        /// <summary>
        /// Specifies the key to set for the title. By default this module sets
        /// a value for the key Title.
        /// </summary>
        /// <param name="key">The metadata key to set.</param>
        /// <returns>The current module instance.</returns>
        public AddTitle WithKey(string key)
        {
            _key = key.ThrowIfNullOrEmpty(nameof(key));
            return this;
        }

        /// <summary>
        /// Indicates that an existing value in the title key should be kept. The
        /// default value is <c>true</c>. Setting to <c>false</c> will always
        /// set the title metadata to the result of this module, even if the
        /// result is null or empty.
        /// </summary>
        /// <param name="keepExisting">Whether to keep the existing title metadata value.</param>
        /// <returns>The current module instance.</returns>
        public AddTitle KeepExisting(bool keepExisting = true)
        {
            _keepExisting = keepExisting;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            // Check if there's already a title set
            if (_keepExisting && input.ContainsKey(_key))
            {
                return input.Yield();
            }

            // Calculate the new title
            string title = await _title.GetValueAsync(input, context);
            return title is null
                ? input.Yield()
                : input.Clone(new MetadataItems { { _key, title } }).Yield();
        }
    }
}