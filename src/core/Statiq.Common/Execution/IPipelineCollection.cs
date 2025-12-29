using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// A collection of pipelines.
    /// </summary>
    public interface IPipelineCollection : IReadOnlyPipelineCollection, IDictionary<string, IPipeline>
    {
        // Adds a new pipeline and returns it for editing
        IPipeline Add(string name);

        IEnumerable<KeyValuePair<string, IPipeline>> AsEnumerable() => this;

        // Avoids ambiguities between multiple interfaces

        new int Count { get; }

        new IEnumerator<KeyValuePair<string, IPipeline>> GetEnumerator();

        new bool ContainsKey(string key);

        new IPipeline this[string key] { get; }

        new ICollection<string> Keys { get; }

        new ICollection<IPipeline> Values { get; }

        new bool TryGetValue(string key, out IPipeline value);
    }
}
