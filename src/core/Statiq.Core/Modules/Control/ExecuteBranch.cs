using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Executes "branches" of modules with the input documents and concatenates their outputs.
    /// </summary>
    /// <category name="Control" />
    public class ExecuteBranch : Module, IList<ModuleList>
    {
        private readonly List<ModuleList> _branches = new List<ModuleList>();

        public ExecuteBranch(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        public ExecuteBranch(IEnumerable<IModule> modules)
        {
            Branch(modules);
        }

        public ExecuteBranch Branch(params IModule[] modules) => Branch((IEnumerable<IModule>)modules);

        public ExecuteBranch Branch(IEnumerable<IModule> modules)
        {
            _branches.Add(new ModuleList(modules.ThrowIfNull(nameof(modules))));
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            foreach (IEnumerable<IModule> modules in _branches)
            {
                results.AddRange(await context.ExecuteModulesAsync(modules, context.Inputs));
            }
            return results;
        }

        /// <summary>
        /// Adds a module to the initial condition. This method is mainly to support collection initialization of the module.
        /// </summary>
        /// <param name="module">The module to add.</param>
        public void Add(IModule module) => _branches[^1].Add(module);

        public int Count => _branches.Count;

        public bool IsReadOnly => false;

        public ModuleList this[int index]
        {
            get => _branches[index];
            set => _branches[index] = value;
        }

        public int IndexOf(ModuleList item) => _branches.IndexOf(item);

        public void Insert(int index, ModuleList item) => _branches.Insert(index, item);

        public void RemoveAt(int index) => _branches.RemoveAt(index);

        public void Add(ModuleList item) => _branches.Add(item);

        public void Clear() => _branches.Clear();

        public bool Contains(ModuleList item) => _branches.Contains(item);

        public void CopyTo(ModuleList[] array, int arrayIndex) => _branches.CopyTo(array, arrayIndex);

        public bool Remove(ModuleList item) => _branches.Remove(item);

        public IEnumerator<ModuleList> GetEnumerator() => ((IEnumerable<ModuleList>)_branches).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}