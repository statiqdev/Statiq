using System.Collections.Generic;

namespace Statiq.Common
{
    public interface IAnalyzerCollection : IReadOnlyDictionary<string, IAnalyzer>
    {
        void Add(string name, IAnalyzer analyzer);
    }
}
