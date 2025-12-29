using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public interface IConcurrentCache
    {
        // Don't want to hold on to them if they'd otherwise be collected, so use a weak reference
        private static readonly List<WeakReference<IConcurrentCache>> ResettableCaches =
            new List<WeakReference<IConcurrentCache>>();

        internal static void AddResettableCache(IConcurrentCache cache) =>
            ResettableCaches.Add(new WeakReference<IConcurrentCache>(cache));

        // Called by the engine prior to execution
        // Not thread safe and caches should not be used during this operation
        internal static void ResetCaches()
        {
            // Reset the caches
            for (int index = 0; index < ResettableCaches.Count; index++)
            {
                WeakReference<IConcurrentCache> cacheReference = ResettableCaches[index];
                if (cacheReference.TryGetTarget(out IConcurrentCache cache))
                {
                    cache.Reset();
                }
                else
                {
                    ResettableCaches[index] = null;
                }
            }

            // Clean up collected caches
            ResettableCaches.RemoveAll(x => x is null);
        }

        /// <summary>
        /// Resets the cache by clearing all entries and disposing any <see cref="IDisposable"/> values.
        /// </summary>
        /// <remarks>
        /// This method is not thread-safe.
        /// </remarks>
        void Reset();
    }
}