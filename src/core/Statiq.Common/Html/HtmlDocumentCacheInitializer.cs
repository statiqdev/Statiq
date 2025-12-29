namespace Statiq.Common
{
    // Clears the cache after each execution
    public class HtmlDocumentCacheInitializer : IEngineInitializer
    {
        public void Initialize(IEngine engine) =>
            engine.Events.Subscribe<AfterEngineExecution>(_ => IDocumentHtmlExtensions.ClearHtmlDocumentCache());
    }
}