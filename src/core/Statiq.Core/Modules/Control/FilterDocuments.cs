using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Filters the current sequence of documents using a predicate.
    /// </summary>
    /// <remarks>
    /// This module filters documents using "or" logic. If you want to also apply
    /// "and" conditions, place additional <see cref="FilterDocuments"/> modules
    /// after this one.
    /// </remarks>
    /// <category name="Control" />
    public class FilterDocuments : Module
    {
        private readonly List<Config<bool>> _predicates = new List<Config<bool>>();

        /// <summary>
        /// Creates a module to filter documents but applies no default filtering.
        /// </summary>
        public FilterDocuments()
        {
        }

        /// <summary>
        /// Specifies the predicate to use for filtering documents.
        /// Only input documents for which the predicate returns <c>true</c> will be output.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        public FilterDocuments(Config<bool> predicate)
        {
            _predicates.Add(predicate.ThrowIfNull(nameof(predicate)));
        }

        /// <summary>
        /// Specifies a metadata key that must be present.
        /// Only input documents for which the key exists will be output.
        /// </summary>
        /// <param name="key">A metadata key that must be present.</param>
        public FilterDocuments(string key)
            : this(Config.FromDocument(doc => doc.ContainsKey(key)))
        {
        }

        /// <summary>
        /// Applies an additional predicate to the filtering operation as an "or" condition.
        /// </summary>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <returns>The current module instance.</returns>
        public FilterDocuments Or(Config<bool> predicate)
        {
            _predicates.Add(predicate.ThrowIfNull(nameof(predicate)));
            return this;
        }

        /// <summary>
        /// Checks for the presence of an additional key in the filtering operation as an "or" condition.
        /// </summary>
        /// <param name="key">A metadata key that must be present.</param>
        /// <returns>The current module instance.</returns>
        public FilterDocuments Or(string key) => Or(Config.FromDocument(doc => doc.ContainsKey(key)));

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            await context.Inputs.FilterAsync(_predicates, context).ToListAsync(context.CancellationToken);
    }
}