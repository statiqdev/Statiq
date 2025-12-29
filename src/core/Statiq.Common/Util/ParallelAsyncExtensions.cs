using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// General extensions for LINQ-like parallel async operations on arbitrary objects.
    /// </summary>
    public static class ParallelAsyncExtensions
    {
        public static async Task<IEnumerable<TSource>> ParallelWhereAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<bool>> asyncPredicate,
            CancellationToken cancellationToken = default) =>
                (await Task.WhenAll(items.Select(x => Task.Run(async () => (x, await asyncPredicate(x)), cancellationToken))))
                    .Where(x => x.Item2)
                    .Select(x => x.Item1);

        /// <summary>
        /// Invokes an async selector in parallel.
        /// </summary>
        /// <remarks>
        /// This method will preserve ordering.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="asyncSelector"/>.</typeparam>
        /// <param name="items">The items to select from.</param>
        /// <param name="asyncSelector">The selector to apply on each element of <paramref name="items"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The result of invoking the <paramref name="asyncSelector"/> on each element of <paramref name="items"/>.</returns>
        public static async Task<IEnumerable<TResult>> ParallelSelectAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<TResult>> asyncSelector,
            CancellationToken cancellationToken = default) =>
                await Task.WhenAll(items.Select(x => Task.Run(() => asyncSelector(x), cancellationToken)));

        /// <summary>
        /// Invokes an async selector that returns multiple results in parallel.
        /// </summary>
        /// <remarks>
        /// This method will preserve ordering.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements of <paramref name="items"/>.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by <paramref name="asyncSelector"/>.</typeparam>
        /// <param name="items">The items to select from.</param>
        /// <param name="asyncSelector">The selector to apply on each element of <paramref name="items"/>.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
        /// <returns>The result of invoking the <paramref name="asyncSelector"/> on each element of <paramref name="items"/>.</returns>
        public static async Task<IEnumerable<TResult>> ParallelSelectManyAsync<TSource, TResult>(
            this IEnumerable<TSource> items,
            Func<TSource, Task<IEnumerable<TResult>>> asyncSelector,
            CancellationToken cancellationToken = default) =>
                (await Task.WhenAll(items.Select(x => Task.Run(() => asyncSelector(x), cancellationToken)))).SelectMany(x => x);

        public static Task ParallelForEachAsync<TSource>(
            this IEnumerable<TSource> items,
            Func<TSource, Task> asyncFunc,
            CancellationToken cancellationToken = default) =>
                Task.WhenAll(items.Select(x => Task.Run(() => asyncFunc(x), cancellationToken)));
    }
}
