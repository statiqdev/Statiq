using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Evaluates a series of child modules for each input document if a specified condition is met.
    /// </summary>
    /// <remarks>
    /// Any result documents from the child modules will be returned as the result of the
    /// this module. Any input documents that don't match a predicate will be returned as
    /// outputs without modification.
    /// </remarks>
    /// <category name="Control" />
    public class ExecuteIf : Module, IList<IfCondition>
    {
        private readonly List<IfCondition> _conditions = new List<IfCondition>();

        private bool _withoutUnmatchedDocuments;

        /// <summary>
        /// Specifies a predicate and a series of child modules to be evaluated if the predicate returns <c>true</c>.
        /// </summary>
        /// <remarks>
        /// If the config requires a document, the predicate will be evaluated against each input document individually.
        /// Otherwise the predicate will be evaluated against the context.
        /// </remarks>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        public ExecuteIf(Config<bool> predicate, params IModule[] modules)
        {
            _conditions.Add(new IfCondition(predicate.ThrowIfNull(nameof(predicate)), modules));
        }

        /// <summary>
        /// Specifies a predicate and a series of child modules to be evaluated if the predicate returns <c>true</c>.
        /// </summary>
        /// <remarks>
        /// If the config requires a document, the predicate will be evaluated against each input document individually.
        /// Otherwise the predicate will be evaluated against the context.
        /// </remarks>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        public ExecuteIf(Config<bool> predicate, IEnumerable<IModule> modules)
        {
            _conditions.Add(new IfCondition(predicate.ThrowIfNull(nameof(predicate)), modules));
        }

        /// <summary>
        /// Specifies child modules to be evaluated if a given metadata key is present in the input document.
        /// </summary>
        /// <remarks>
        /// Only input documents will be processed and the modules will not be run if there are no input documents.
        /// </remarks>
        /// <param name="key">A metadata key that must be present in the input document.</param>
        /// <param name="modules">The modules to execute on documents that contain the specified metadata key.</param>
        public ExecuteIf(string key, params IModule[] modules)
            : this(Config.FromDocument(doc => doc.ContainsKey(key)), modules)
        {
        }

        /// <summary>
        /// Specifies child modules to be evaluated if a given metadata key is present in the input document.
        /// </summary>
        /// <remarks>
        /// Only input documents will be processed and the modules will not be run if there are no input documents.
        /// </remarks>
        /// <param name="key">A metadata key that must be present in the input document.</param>
        /// <param name="modules">The modules to execute on documents that contain the specified metadata key.</param>
        public ExecuteIf(string key, IEnumerable<IModule> modules)
            : this(Config.FromDocument(doc => doc.ContainsKey(key)), modules)
        {
        }

        /// <summary>
        /// Specifies child modules to be evaluated if a given metadata key is present in the input document.
        /// </summary>
        /// <remarks>
        /// Only input documents will be processed and the modules will not be run if there are no input documents.
        /// </remarks>
        /// <param name="keys">Metadata keys that must be present in the input document.</param>
        /// <param name="modules">The modules to execute on documents that contain the specified metadata key.</param>
        public ExecuteIf(IEnumerable<string> keys, params IModule[] modules)
            : this(Config.FromDocument(doc => doc.ContainsKeys(keys)), modules)
        {
        }

        /// <summary>
        /// Specifies child modules to be evaluated if a given metadata key is present in the input document.
        /// </summary>
        /// <remarks>
        /// Only input documents will be processed and the modules will not be run if there are no input documents.
        /// </remarks>
        /// <param name="keys">Metadata keys that must be present in the input document.</param>
        /// <param name="modules">The modules to execute on documents that contain the specified metadata key.</param>
        public ExecuteIf(IEnumerable<string> keys, IEnumerable<IModule> modules)
            : this(Config.FromDocument(doc => doc.ContainsKeys(keys)), modules)
        {
        }

        /// <summary>
        /// Adds a module to the initial condition. This method is mainly to support collection initialization of the module.
        /// </summary>
        /// <param name="module">The module to add.</param>
        public void Add(IModule module) => _conditions[0].Add(module);

        /// <summary>
        /// Specifies an alternate condition to be tested on documents that did not satisfy
        /// previous conditions. You can chain together as many <c>ElseIf</c> calls as needed.
        /// </summary>
        /// <remarks>
        /// If the config requires a document, the predicate will be evaluated against each input document individually.
        /// Otherwise the predicate will be evaluated against the context.
        /// </remarks>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        /// <returns>The current module instance.</returns>
        public ExecuteIf ElseIf(Config<bool> predicate, params IModule[] modules)
        {
            _conditions.Add(new IfCondition(predicate.ThrowIfNull(nameof(predicate)), modules));
            return this;
        }

        /// <summary>
        /// Specifies an alternate condition to be tested on documents that did not satisfy
        /// previous conditions. You can chain together as many <c>ElseIf</c> calls as needed.
        /// </summary>
        /// <remarks>
        /// If the config requires a document, the predicate will be evaluated against each input document individually.
        /// Otherwise the predicate will be evaluated against the context.
        /// </remarks>
        /// <param name="predicate">A predicate delegate that should return a <c>bool</c>.</param>
        /// <param name="modules">The modules to execute on documents where the predicate is <c>true</c>.</param>
        /// <returns>The current module instance.</returns>
        public ExecuteIf ElseIf(Config<bool> predicate, IEnumerable<IModule> modules)
        {
            _conditions.Add(new IfCondition(predicate.ThrowIfNull(nameof(predicate)), modules));
            return this;
        }

        /// <summary>
        /// Specifies an alternate condition to be tested on documents that did not satisfy
        /// previous conditions. You can chain together as many <c>ElseIf</c> calls as needed.
        /// </summary>
        /// <remarks>
        /// Only input documents will be processed and the modules will not be run if there are no input documents.
        /// </remarks>
        /// <param name="key">A metadata key that must be present.</param>
        /// <param name="modules">The modules to execute on documents that contain the specified metadata key.</param>
        /// <returns>The current module instance.</returns>
        public ExecuteIf ElseIf(string key, params IModule[] modules) =>
            ElseIf(Config.FromDocument(doc => doc.ContainsKey(key)), modules);

        /// <summary>
        /// Specifies an alternate condition to be tested on documents that did not satisfy
        /// previous conditions. You can chain together as many <c>ElseIf</c> calls as needed.
        /// </summary>
        /// <remarks>
        /// Only input documents will be processed and the modules will not be run if there are no input documents.
        /// </remarks>
        /// <param name="key">A metadata key that must be present.</param>
        /// <param name="modules">The modules to execute on documents that contain the specified metadata key.</param>
        /// <returns>The current module instance.</returns>
        public ExecuteIf ElseIf(string key, IEnumerable<IModule> modules) =>
            ElseIf(Config.FromDocument(doc => doc.ContainsKey(key)), modules);

        /// <summary>
        /// This should be at the end of your fluent method chain and will evaluate the
        /// specified child modules on all documents that did not satisfy previous predicates.
        /// The predicate will be evaluated against every input document individually.
        /// </summary>
        /// <param name="modules">The modules to execute on documents where no previous predicate was <c>true</c>.</param>
        /// <returns>The current module instance.</returns>
        public IModule Else(params IModule[] modules)
        {
            _conditions.Add(new IfCondition(modules));
            return this;
        }

        /// <summary>
        /// This should be at the end of your fluent method chain and will evaluate the
        /// specified child modules on all documents that did not satisfy previous predicates.
        /// The predicate will be evaluated against every input document individually.
        /// </summary>
        /// <param name="modules">The modules to execute on documents where no previous predicate was <c>true</c>.</param>
        /// <returns>The current module instance.</returns>
        public IModule Else(IEnumerable<IModule> modules)
        {
            _conditions.Add(new IfCondition(modules));
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
        public ExecuteIf WithoutUnmatchedDocuments(bool withoutUnmatchedDocuments = true)
        {
            _withoutUnmatchedDocuments = withoutUnmatchedDocuments;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            List<IDocument> results = new List<IDocument>();
            IReadOnlyList<IDocument> documents = context.Inputs;
            bool hasExecuted = false;
            foreach (IfCondition condition in _conditions)
            {
                // Split the documents into ones that satisfy the predicate and ones that don't
                List<IDocument> matched = new List<IDocument>();
                List<IDocument> unmatched = new List<IDocument>();
                bool predicateResult = false;
                if (condition.Predicate is null)
                {
                    // This is the final else without a predicate
                    matched.AddRange(documents);
                }
                else if (condition.Predicate.RequiresDocument)
                {
                    foreach (IDocument document in documents)
                    {
                        if (await condition.Predicate.GetValueAsync(document, context))
                        {
                            predicateResult = true;
                            matched.Add(document);
                        }
                        else
                        {
                            unmatched.Add(document);
                        }
                    }
                }
                else if (!hasExecuted)
                {
                    if (await condition.Predicate.GetValueAsync(null, context))
                    {
                        predicateResult = true;
                        matched.AddRange(documents);
                    }
                    else
                    {
                        unmatched.AddRange(documents);
                    }
                }

                // Run the modules on the documents that satisfy the predicate
                if (matched.Count > 0
                    || (condition.Predicate is null && !hasExecuted)
                    || (condition.Predicate is object && !condition.Predicate.RequiresDocument && predicateResult))
                {
                    hasExecuted = true; // Need to track if one of the pre-else conditions was executed
                    results.AddRange(await context.ExecuteModulesAsync(condition, matched));
                }

                // Continue with the documents that don't satisfy the predicate
                documents = unmatched;
            }

            // Add back any documents that never matched a predicate
            if (!_withoutUnmatchedDocuments)
            {
                results.AddRange(documents);
            }

            return results;
        }

        /// <inheritdoc />
        public int Count => _conditions.Count;

        /// <inheritdoc />
        public bool IsReadOnly => ((IList<IfCondition>)_conditions).IsReadOnly;

        /// <inheritdoc />
        public IfCondition this[int index]
        {
            get => _conditions[index];
            set => _conditions[index] = value;
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <inheritdoc />
        public IEnumerator<IfCondition> GetEnumerator() => _conditions.GetEnumerator();

        /// <inheritdoc />
        public int IndexOf(IfCondition item) => _conditions.IndexOf(item);

        /// <inheritdoc />
        public void Insert(int index, IfCondition item) => _conditions.Insert(index, item);

        /// <inheritdoc />
        public void RemoveAt(int index) => _conditions.RemoveAt(index);

        /// <inheritdoc />
        public void Add(IfCondition item) => _conditions.Add(item);

        /// <inheritdoc />
        public void Clear() => _conditions.Clear();

        /// <inheritdoc />
        public bool Contains(IfCondition item) => _conditions.Contains(item);

        /// <inheritdoc />
        public void CopyTo(IfCondition[] array, int arrayIndex) => _conditions.CopyTo(array, arrayIndex);

        /// <inheritdoc />
        public bool Remove(IfCondition item) => _conditions.Remove(item);
    }
}