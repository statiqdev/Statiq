using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Statiq.Common
{
    /// <summary>
    /// A collection of modules.
    /// </summary>
    public class ModuleList : IList<IModule>
    {
        private readonly List<IModule> _modules = new List<IModule>();

        /// <summary>
        /// Creates a new empty module list.
        /// </summary>
        public ModuleList()
        {
        }

        /// <summary>
        /// Creates a new module list with an initial set of modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The initial modules in the list.</param>
        public ModuleList(params IModule[] modules)
            : this((IEnumerable<IModule>)modules)
        {
        }

        /// <summary>
        /// Creates a new module list with an initial set of modules.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The initial modules in the list.</param>
        public ModuleList(IEnumerable<IModule> modules)
        {
            AddRange(modules);
        }

        /// <summary>
        /// Adds modules to the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The modules to add.</param>
        public void Add(params IModule[] modules) => AddRange(modules);

        /// <summary>
        /// Adds modules to the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="modules">The modules to add.</param>
        public void AddRange(IEnumerable<IModule> modules)
        {
            if (modules is object)
            {
                foreach (IModule module in modules.Where(x => x is object))
                {
                    _modules.Add(module);
                }
            }
        }

        /// <inheritdoc />
        void ICollection<IModule>.Add(IModule item)
        {
            item.ThrowIfNull(nameof(item));
            _modules.Add(item);
        }

        /// <summary>
        /// Inserts modules into the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="index">The index at which to insert the modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public void Insert(int index, params IModule[] modules) =>
            InsertRange(index, (IEnumerable<IModule>)modules);

        /// <summary>
        /// Inserts modules into the list.
        /// Any <c>null</c> items in the sequence of modules will be discarded.
        /// </summary>
        /// <param name="index">The index at which to insert the modules.</param>
        /// <param name="modules">The modules to insert.</param>
        public void InsertRange(int index, IEnumerable<IModule> modules)
        {
            IModule[] moduleArray = modules.Where(x => x is object).ToArray();
            for (int i = index; i < index + moduleArray.Length; i++)
            {
                _modules.Insert(i, moduleArray[i - index]);
            }
        }

        /// <inheritdoc />
        public void Insert(int index, IModule item)
        {
            item.ThrowIfNull(nameof(item));
            _modules.Insert(index, item);
        }

        /// <inheritdoc />
        public bool Remove(IModule item)
        {
            int index = IndexOf(item);
            if (index >= 0)
            {
                _modules.RemoveAt(index);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public void RemoveAt(int index) => _modules.RemoveAt(index);

        /// <inheritdoc />
        public IModule this[int index]
        {
            get => _modules[index];
            set => _modules[index] = value.ThrowIfNull(nameof(value));
        }

        /// <inheritdoc />
        public int Count => _modules.Count;

        /// <inheritdoc />
        public void Clear() => _modules.Clear();

        /// <inheritdoc />
        public bool Contains(IModule item) => _modules.Any(x => x.Equals(item));

        /// <inheritdoc />
        public void CopyTo(IModule[] array, int arrayIndex) => _modules.CopyTo(array);

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf(IModule item) => _modules.FindIndex(x => x.Equals(item));

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<IModule> GetEnumerator() => _modules.GetEnumerator();

        public static implicit operator ModuleList(IModule[] modules) => new ModuleList(modules);

        /// <summary>
        /// Appends modules.
        /// </summary>
        /// <param name="modules">The modules to append.</param>
        /// <returns>The current instance.</returns>
        public ModuleList Append(params IModule[] modules)
        {
            Add(modules);
            return this;
        }

        /// <summary>
        /// Prepends modules.
        /// </summary>
        /// <param name="modules">The modules to prepend.</param>
        /// <returns>The current instance.</returns>
        public ModuleList Prepend(params IModule[] modules)
        {
            Insert(0, modules);
            return this;
        }

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertBeforeFirst<TModule>(params IModule[] modules)
            where TModule : class, IModule
            => InsertBeforeFirst<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertBeforeFirst<TModule>(Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            Insert(IndexOfFirstOrThrow(filter), modules);
            return this;
        }

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertAfterFirst<TModule>(params IModule[] modules)
            where TModule : class, IModule
            => InsertAfterFirst<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertAfterFirst<TModule>(Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            Insert(IndexOfFirstOrThrow(filter) + 1, modules);
            return this;
        }

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertBeforeLast<TModule>(params IModule[] modules)
            where TModule : class, IModule
            => InsertBeforeLast<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules before the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertBeforeLast<TModule>(Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            Insert(IndexOfLastOrThrow(filter), modules);
            return this;
        }

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the specified modules.</typeparam>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertAfterLast<TModule>(params IModule[] modules)
            where TModule : class, IModule
            => InsertAfterLast<TModule>(_ => true, modules);

        /// <summary>
        /// Inserts modules after the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module at which to insert the modules.</typeparam>
        /// <param name="filter">A predicate determining at which module to insert the specified modules.</param>
        /// <param name="modules">The modules to insert.</param>
        /// <returns>The current instance.</returns>
        public ModuleList InsertAfterLast<TModule>(Predicate<TModule> filter, params IModule[] modules)
            where TModule : class, IModule
        {
            Insert(IndexOfLastOrThrow(filter) + 1, modules);
            return this;
        }

        /// <summary>
        /// Replaces the first module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="module">The replacement module.</param>
        /// <returns>The current instance.</returns>
        public ModuleList ReplaceFirst<TModule>(IModule module)
            where TModule : class, IModule
            => ReplaceFirst<TModule>(_ => true, module);

        /// <summary>
        /// Replaces the first module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <returns>The current instance.</returns>
        public ModuleList ReplaceFirst<TModule>(Predicate<TModule> filter, IModule module)
            where TModule : class, IModule
            => Replace(IndexOfFirstOrThrow(filter), module);

        /// <summary>
        /// Replaces the last module in the list of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="module">The replacement module.</param>
        /// <returns>The current instance.</returns>
        public ModuleList ReplaceLast<TModule>(IModule module)
            where TModule : class, IModule
            => ReplaceLast<TModule>(_ => true, module);

        /// <summary>
        /// Replaces the last module in the list of the specified type that satisfies a predicate.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to replace.</typeparam>
        /// <param name="filter">A predicate determining which module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <returns>The current instance.</returns>
        public ModuleList ReplaceLast<TModule>(Predicate<TModule> filter, IModule module)
            where TModule : class, IModule
            => Replace(IndexOfLastOrThrow(filter), module);

        /// <summary>
        /// Replaces a module at the specified index.
        /// </summary>
        /// <param name="index">The index of the module to replace.</param>
        /// <param name="module">The replacement module.</param>
        /// <returns>The current instance.</returns>
        public ModuleList Replace(int index, IModule module)
        {
            RemoveAt(index);
            Insert(index, module);
            return this;
        }

        /// <summary>
        /// Modifies an inner module list with the specified index.
        /// </summary>
        /// <param name="index">The index of the inner module to modify.</param>
        /// <param name="action">The action to apply to the inner module.</param>
        /// <returns>The current instance.</returns>
        public ModuleList Modify(int index, Action<IModule> action)
        {
            action.ThrowIfNull(nameof(action));
            action(this[index]);
            return this;
        }

        /// <summary>
        /// Gets the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <returns>The first module of the specified type or null if a module of the specified type could not be found.</returns>
        public TModule GetFirst<TModule>()
            where TModule : class, IModule
            => GetFirst<TModule>(_ => true);

        /// <summary>
        /// Gets the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The first module of the specified type or null if a module of the specified type could not be found.</returns>
        public TModule GetFirst<TModule>(Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = IndexOfFirst(filter);
            return index == -1 ? default : (TModule)this[index];
        }

        /// <summary>
        /// Gets the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <returns>The last module of the specified type or null if a module of the specified type could not be found.</returns>
        public TModule GetLast<TModule>()
            where TModule : class, IModule
            => GetLast<TModule>(_ => true);

        /// <summary>
        /// Gets the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The last module of the specified type or null if a module of the specified type could not be found.</returns>
        public TModule GetLast<TModule>(Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = IndexOfLast(filter);
            return index == -1 ? default : (TModule)this[index];
        }

        /// <summary>
        /// Gets the index of the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <returns>The index of the first module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public int IndexOfFirst<TModule>()
            where TModule : class, IModule
            => IndexOfFirst<TModule>(_ => true);

        /// <summary>
        /// Gets the index of the first module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The index of the first module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public int IndexOfFirst<TModule>(Predicate<TModule> filter)
            where TModule : class, IModule
        {
            for (int index = 0; index < Count; index++)
            {
                IModule module = this[index];

                if (!(module is TModule expectedModule))
                {
                    continue;
                }

                if (filter(expectedModule))
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <returns>The index of the last module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public int IndexOfLast<TModule>()
            where TModule : class, IModule
            => IndexOfLast<TModule>(_ => true);

        /// <summary>
        /// Gets the index of the last module of the specified type.
        /// </summary>
        /// <typeparam name="TModule">The type of the module to find.</typeparam>
        /// <param name="filter">A predicate determining which module to find.</param>
        /// <returns>The index of the last module of the specified type or -1 if a module of the specified type could not be found.</returns>
        public int IndexOfLast<TModule>(Predicate<TModule> filter)
            where TModule : class, IModule
        {
            for (int index = Count - 1; index >= 0; index--)
            {
                IModule module = this[index];

                if (!(module is TModule expectedModule))
                {
                    continue;
                }

                if (filter(expectedModule))
                {
                    return index;
                }
            }

            return -1;
        }

        private int IndexOfFirstOrThrow<TModule>(Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = IndexOfFirst(filter);
            if (index == -1)
            {
                throw new InvalidOperationException($"Could not find module of type {typeof(TModule).FullName}");
            }
            return index;
        }

        private int IndexOfLastOrThrow<TModule>(Predicate<TModule> filter)
            where TModule : class, IModule
        {
            int index = IndexOfLast(filter);
            if (index == -1)
            {
                throw new InvalidOperationException($"Could not find module of type {typeof(TModule).FullName}");
            }
            return index;
        }
    }
}
