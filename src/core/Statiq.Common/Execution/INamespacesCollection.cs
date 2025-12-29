using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A collection of namespace strings used to inform modules of which namespaces
    /// should be available during dynamic code generation and/or execution.
    /// </summary>
    public interface INamespacesCollection : IReadOnlyCollection<string>, ICacheCode
    {
        /// <summary>
        /// Adds a namespace to the collection.
        /// </summary>
        /// <param name="ns">The namespace to add (will throw for null, empty, or white space values).</param>
        /// <returns><c>true</c> if the namespace was already in the collection, otherwise <c>false</c>.</returns>
        bool Add(string ns);

        /// <summary>
        /// Adds a range of namespaces to the collection.
        /// </summary>
        /// <param name="namespaces">The namespaces to add (will throw for null, empty, or white space values).</param>
        void AddRange(IEnumerable<string> namespaces);
    }
}