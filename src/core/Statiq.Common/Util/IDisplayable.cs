namespace Statiq.Common
{
    /// <summary>
    /// Indicates the implementation supports a display string (typically used for logging).
    /// </summary>
    public interface IDisplayable
    {
        /// <summary>
        /// A display string that can be used for tracing.
        /// </summary>
        /// <remarks>
        /// Generally you should use <see cref="IDisplayableExtensions.ToSafeDisplayString(IDisplayable)"/>
        /// to avoid null reference exceptions if the underlying object is null.
        /// </remarks>
        /// <returns>A display string.</returns>
        string ToDisplayString();
    }
}