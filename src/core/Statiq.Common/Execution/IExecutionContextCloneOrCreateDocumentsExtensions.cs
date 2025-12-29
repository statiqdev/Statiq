using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionContextCloneOrCreateDocumentsExtensions
    {
        /// <summary>
        /// Clones or creates documents from several types of values.
        /// </summary>
        /// <remarks>
        /// If the value is <c>null</c> the provided document will be returned.
        /// If the value is a <see cref="IDocument"/>, <see cref="IEnumerable{IDocument}"/>, or
        /// <see cref="IAsyncEnumerable{IDocument}"/>, the document(s) will be returned.
        /// If the value is <c>IEnumerable&lt;KeyValuePair&lt;string, object&gt;&gt;</c>,
        /// the document will be cloned with the resulting metadata.
        /// If the value is a <see cref="IEnumerable{IModule}"/> or <see cref="IModule"/>,
        /// the module(s) will be executed with the input document(s) as their input
        /// and the results will be returned.
        /// If the value is an <see cref="IContentProvider"/>,
        /// <see cref="IContentProviderFactory"/>, or <see cref="Stream"/> it will be used to get new content for the
        /// provided document (or create a new document if the provided document is <c>null</c>).
        /// If config value is anything else, the content of the input document will be
        /// changed to (or a new document created with) the string value.
        /// </remarks>
        /// <param name="executionContext">The execution context.</param>
        /// <param name="document">The document to clone (if appropriate).</param>
        /// <param name="moduleInputs">The inputs to use when executing if <paramref name="value"/> contains modules.</param>
        /// <param name="value">The value to clone or create documents from.</param>
        /// <returns>The result documents.</returns>
        public static async Task<IEnumerable<IDocument>> CloneOrCreateDocumentsAsync(
            this IExecutionContext executionContext,
            IDocument document,
            IEnumerable<IDocument> moduleInputs,
            object value) =>
            value is null
                ? document.Yield()
                : GetDocuments(value)
                    ?? GetDocumentFromMetadata(executionContext, document, value)
                        ?? await ExecuteModulesAsync(executionContext, moduleInputs, value)
                            ?? ChangeContent(executionContext, document, value);

        private static IEnumerable<IDocument> GetDocuments(object value) =>
            value is IDocument document ? document.Yield() : value as IEnumerable<IDocument>;

        private static IEnumerable<IDocument> GetDocumentFromMetadata(IExecutionContext context, IDocument document, object value) =>
            value is IEnumerable<KeyValuePair<string, object>> metadata ? context.CloneOrCreateDocument(document, metadata).Yield() : null;

        private static async Task<IEnumerable<IDocument>> ExecuteModulesAsync(IExecutionContext context, IEnumerable<IDocument> moduleInputs, object value)
        {
            // Check for a single IModule first since some modules also implement IEnumerable<IModule>
            IEnumerable<IModule> modules = value is IModule module ? new[] { module } : value as IEnumerable<IModule>;
            return modules is null ? null : (IEnumerable<IDocument>)await context.ExecuteModulesAsync(modules, moduleInputs);
        }

        private static IEnumerable<IDocument> ChangeContent(IExecutionContext context, IDocument document, object value)
        {
            // Check if this is a known content provider type first
            IContentProvider contentProvider = GetContentProvider(context, document, value, false);
            if (contentProvider is object)
            {
                return context.CloneOrCreateDocument(document, contentProvider).Yield();
            }

            // It wasn't a known content provider type, so treat as an enumeration and convert the string value
            object[] valueObjects = (value as IEnumerable ?? new[] { value }).Cast<object>().ToArray();
            IDocument[] results = new IDocument[valueObjects.Length];
            for (int c = 0; c < valueObjects.Length; c++)
            {
                results[c] = context.CloneOrCreateDocument(document, GetContentProvider(context, document, valueObjects[c], true));
            }
            return results;
        }

        // Preserves the original media type of the input document
        private static IContentProvider GetContentProvider(IExecutionContext context, IDocument document, object value, bool asString)
        {
            switch (value)
            {
                case IContentProvider contentProvider:
                    return contentProvider;
                case IContentProviderFactory factory:
                    return factory.GetContentProvider(document?.ContentProvider?.MediaType);
                case Stream stream:
                    return context.GetContentProvider(stream, document?.ContentProvider?.MediaType);
                case string str:
                    return context.GetContentProvider(str, document?.ContentProvider?.MediaType);
            }
            return asString && value is object ? context.GetContentProvider(value.ToString(), document?.ContentProvider?.MediaType) : null;
        }
    }
}
