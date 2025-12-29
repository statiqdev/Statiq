using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;

namespace Statiq.Testing
{
    [Parallelizable(ParallelScope.Children)]
    public abstract class BaseFixture
    {
        public static readonly IReadOnlyList<TestDocument> EmptyDocuments = ImmutableArray<TestDocument>.Empty;

        [SetUp]
        public void BaseSetUp()
        {
            Directory.SetCurrentDirectory(TestContext.CurrentContext.TestDirectory);

            // Make sure we set an AsycLocal IExecutionContext (might get overridden in the actual test)
            _ = new TestExecutionContext();
        }

        public static MemoryStream GetTestFileStream(string fileName) =>
            new MemoryStream(File.ReadAllBytes(Path.Combine(TestContext.CurrentContext.TestDirectory, fileName)));

        public static TestDocument[] GetDocuments(params string[] content) =>
            content.Select(x => new TestDocument(x)).ToArray();

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created and an empty initial document collection will be used.
        /// </summary>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<ImmutableArray<TestDocument>> ExecuteAsync(params IModule[] modules) =>
            ExecuteAsync(new TestExecutionContext(), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// </summary>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static async Task<ImmutableArray<TestDocument>> ExecuteAsync(TestExecutionContext context, params IModule[] modules) =>
            (await context.ExecuteModulesAsync(modules, context.Inputs)).Cast<TestDocument>().ToImmutableDocumentArray();

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="document">The initial input document.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<ImmutableArray<TestDocument>> ExecuteAsync(TestDocument document, params IModule[] modules) =>
            ExecuteAsync(new TestExecutionContext(ImmutableArray.Create(document)), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="documents">The initial input documents.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static Task<ImmutableArray<TestDocument>> ExecuteAsync(IEnumerable<TestDocument> documents, params IModule[] modules) =>
            ExecuteAsync(new TestExecutionContext(documents.ToImmutableDocumentArray()), modules);

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="document">The initial input document.</param>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static async Task<ImmutableArray<TestDocument>> ExecuteAsync(TestDocument document, TestExecutionContext context, params IModule[] modules) =>
            (await context.ExecuteModulesAsync(modules, document?.Yield())).Cast<TestDocument>().ToImmutableDocumentArray();

        /// <summary>
        /// A utility method to execute modules in serial. The resulting documents will be materialized before returning.
        /// A new <see cref="TestExecutionContext"/> will be created.
        /// </summary>
        /// <param name="documents">The initial input documents.</param>
        /// <param name="context">The execution context to use.</param>
        /// <param name="modules">The modules to execute.</param>
        /// <returns>A materialized list of result documents from the last module.</returns>
        public static async Task<ImmutableArray<TestDocument>> ExecuteAsync(IEnumerable<TestDocument> documents, TestExecutionContext context, params IModule[] modules) =>
            (await context.ExecuteModulesAsync(modules, documents)).Cast<TestDocument>().ToImmutableDocumentArray();
    }
}
