namespace Statiq.Common
{
    /// <summary>
    /// This is a non-generic base class for <see cref="Document{TDocument}"/> and
    /// is not intended to be used directly. Derive from <see cref="Document{TDocument}"/>
    /// instead.
    /// </summary>
    public abstract class FactoryDocument
    {
        // Prevents direct sub-classing
        internal FactoryDocument()
        {
        }

        internal abstract IDocument Initialize(
            IReadOnlySettings settings,
            NormalizedPath source,
            NormalizedPath destination,
            IMetadata metadata,
            IContentProvider contentProvider);
    }
}
