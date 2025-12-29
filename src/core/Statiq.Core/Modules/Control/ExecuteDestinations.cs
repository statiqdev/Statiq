using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    public class ExecuteDestinations : ConfigModule<IEnumerable<string>>, IList<IModule>
    {
        private readonly ModuleList _modules = new ModuleList();

        private bool _withoutUnmatchedDocuments;

        /// <summary>
        /// Filters input document and executes modules on them by using globbing pattern(s) on the document destination.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        /// <param name="modules">The modules to execute on the filtered documents.</param>
        public ExecuteDestinations(Config<IEnumerable<string>> patterns, params IModule[] modules)
            : base(patterns, false)
        {
            WithModules(modules);
        }

        /// <summary>
        /// Filters input document and executes modules on them by using globbing pattern(s) on the document destination.
        /// </summary>
        /// <param name="pattern">The globbing pattern to apply.</param>
        /// <param name="modules">The modules to execute on the filtered documents.</param>
        public ExecuteDestinations(Config<string> pattern, params IModule[] modules)
            : base(pattern.ThrowIfNull(nameof(pattern)).MakeEnumerable(), false)
        {
            WithModules(modules);
        }

        /// <summary>
        /// Filters input document and executes modules on them by using globbing pattern(s) on the document destination.
        /// </summary>
        /// <remarks>Use collection initialization or <see cref="WithModules(IModule[])"/> to add modules to execute.</remarks>
        /// <param name="patterns">The globbing patterns to apply.</param>
        public ExecuteDestinations(params string[] patterns)
            : base(patterns, false)
        {
        }

        /// <summary>
        /// Filters input document and executes modules on them by using globbing pattern(s) on the document destination.
        /// </summary>
        /// <param name="patterns">The globbing patterns to apply.</param>
        /// <param name="modules">The modules to execute on the filtered documents.</param>
        public ExecuteDestinations(IEnumerable<string> patterns, params IModule[] modules)
            : base(Config.FromValue(patterns), false)
        {
            WithModules(modules);
        }

        /// <summary>
        /// Adds a module to execute. This method is mainly to support collection initialization.
        /// </summary>
        /// <param name="module">The module to add.</param>
        public void Add(IModule module) => _modules.Add(module);

        public ExecuteDestinations WithModules(params IModule[] modules)
        {
            if (modules is object)
            {
                _modules.AddRange(modules);
            }
            return this;
        }

        /// <summary>
        /// The default behavior of this module is to "fall through" any documents that
        /// didn't match one of the conditions and add it to the result set. This method
        /// allows you to change that behavior and prevent unmatched documents from being
        /// added to the result set.
        /// </summary>
        /// <param name="withoutUnmatchedDocuments">Set to <c>true</c> to prevent unmatched documents from being added to the resut set.</param>
        /// <returns>The current module.</returns>
        public ExecuteDestinations WithoutUnmatchedDocuments(bool withoutUnmatchedDocuments = true)
        {
            _withoutUnmatchedDocuments = withoutUnmatchedDocuments;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteConfigAsync(IDocument input, IExecutionContext context, IEnumerable<string> value)
        {
            List<IDocument> results = new List<IDocument>();
            HashSet<IDocument> matched = new HashSet<IDocument>(context.Inputs.FilterDestinations(value));
            if (matched.Count > 0)
            {
                results.AddRange(await context.ExecuteModulesAsync(_modules, matched));
            }

            // Add back any documents that never matched a predicate
            if (!_withoutUnmatchedDocuments)
            {
                results.AddRange(context.Inputs.Where(x => !matched.Contains(x)));
            }

            return results;
        }

        public int Count => _modules.Count;

        public bool IsReadOnly => _modules.IsReadOnly;

        public IModule this[int index] { get => _modules[index]; set => _modules[index] = value; }

        public int IndexOf(IModule item) => _modules.IndexOf(item);

        public void Insert(int index, IModule item) => _modules.Insert(index, item);

        public void RemoveAt(int index) => _modules.RemoveAt(index);

        public void Clear() => _modules.Clear();

        public bool Contains(IModule item) => _modules.Contains(item);

        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array, arrayIndex);

        public bool Remove(IModule item) => _modules.Remove(item);

        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_modules).GetEnumerator();
    }
}
