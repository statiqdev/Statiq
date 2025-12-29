namespace Statiq.Lunr
{
    /// <summary>
    /// Metadata keys for use by the <see cref="GenerateLunrIndex"/> module.
    /// </summary>
    public static class LunrKeys
    {
        /// <summary>
        /// A metadata key that contains search items for the given document. The value
        /// should be either a <c>IEnumerable&lt;IReadOnlyDictionary&lt;string, object&gt;&gt;</c>
        /// or a <c>IReadOnlyDictionary&lt;string, object&gt;</c>.
        /// </summary>
        /// <remarks>
        /// Make sure to include a reference value for each explicit search item, otherwise the
        /// search item won't be included in the search index.
        /// </remarks>
        public const string SearchItems = nameof(SearchItems);

        /// <summary>
        /// Set to <c>true</c> in document metadata to omit a document from the search.
        /// </summary>
        public const string OmitFromSearch = nameof(OmitFromSearch);
    }
}
