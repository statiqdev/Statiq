using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Filters the current sequence of documents by source.
    /// </summary>
    /// <remarks>
    /// This module filters documents using "or" logic. If you want to also apply
    /// "and" conditions, place additional <see cref="FilterSources"/> modules
    /// after this one.
    /// </remarks>
    /// <category name="Control" />
    public class FilterSources : SyncConfigModule<IEnumerable<string>>
    {
        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public FilterSources(Config<IEnumerable<string>> patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="pattern">The globbing pattern to apply.</param>
        public FilterSources(Config<string> pattern)
            : base(pattern.ThrowIfNull(nameof(pattern)).MakeEnumerable(), false)
        {
        }

        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public FilterSources(params string[] patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Filters input document by using globbing pattern(s) on the document source.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public FilterSources(IEnumerable<string> patterns)
            : base(Config.FromValue(patterns), false)
        {
        }

        protected override IEnumerable<IDocument> ExecuteConfig(
            IDocument input,
            IExecutionContext context,
            IEnumerable<string> value) =>
            context.Inputs.FilterSources(value);
    }
}