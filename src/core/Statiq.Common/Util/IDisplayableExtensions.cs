namespace Statiq.Common
{
    public static class IDisplayableExtensions
    {
        /// <summary>
        /// A safe display string that can be used for logging and returns "null" when the
        /// underlying object is null.
        /// </summary>
        /// <param name="displayable">The <see cref="IDisplayable"/> object.</param>
        /// <returns>A display string.</returns>
        public static string ToSafeDisplayString(this IDisplayable displayable) => displayable?.ToDisplayString() ?? "null";
    }
}