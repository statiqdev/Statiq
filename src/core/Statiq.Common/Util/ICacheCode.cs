using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// An object that provides a deterministic hash code suitable for caching.
    /// </summary>
    public interface ICacheCode
    {
        /// <summary>
        /// Gets a deterministic hash appropriate for caching.
        /// </summary>
        /// <returns>A deterministic hash appropriate for caching.</returns>
        Task<int> GetCacheCodeAsync();
    }
}
