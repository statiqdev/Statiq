using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Statiq.Common
{
    public static class IExecutionStateExtensions
    {
        /// <summary>
        /// Sends a GET request with exponential back-off.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="uri">The request URI.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(this IExecutionState executionState, string uri) =>
            await executionState.SendHttpRequestWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, uri));

        /// <summary>
        /// Sends a GET request with exponential back-off.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="uri">The request URI.</param>
        /// <returns>The response.</returns>
        public static async Task<HttpResponseMessage> SendHttpRequestWithRetryAsync(this IExecutionState executionState, Uri uri) =>
            await executionState.SendHttpRequestWithRetryAsync(() => new HttpRequestMessage(HttpMethod.Get, uri));

        /// <summary>
        /// Gets the current date/time using the <c>CurrentDateTime</c> metadata setting if it's set,
        /// otherwise using the value of <see cref="IExecutionState.ExecutionDateTime" /> (which is
        /// <see cref="DateTime.Now" /> at the time execution started).
        /// </summary>
        /// <remarks>
        /// This method should always be used instead of <see cref="DateTime.Now" /> or <see cref="DateTime.Today" />
        /// whenever possible, by both code and themes.
        /// </remarks>
        public static DateTime GetCurrentDateTime(this IExecutionState executionState) =>
            executionState?.Settings.ContainsKey(Keys.CurrentDateTime) == true
                && executionState.Settings.TryGetValue(Keys.CurrentDateTime, out DateTime dateTime)
                ? dateTime
                : executionState?.ExecutionDateTime ?? DateTime.Now;
    }
}