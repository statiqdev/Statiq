namespace Statiq.Common
{
    /// <summary>
    /// Implementations of this interface can return a content provider.
    /// </summary>
    public interface IContentProviderFactory
    {
        /// <summary>
        /// Gets a content provider.
        /// </summary>
        /// <returns>The content provider to use with a document.</returns>
        IContentProvider GetContentProvider();

        /// <summary>
        /// Gets a content provider with the specified media type.
        /// </summary>
        /// <param name="mediaType">The media type of the content provider.</param>
        /// <returns>The content provider to use with a document.</returns>
        IContentProvider GetContentProvider(string mediaType);
    }
}
