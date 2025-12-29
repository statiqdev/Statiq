namespace Statiq.Common
{
    /// <summary>
    /// Implement this interface to define an initializer that will get automatically
    /// instantiated and run by the engine at startup.
    /// </summary>
    /// <remarks>
    /// In order for the engine to find the initializer, the class must be <c>public</c>.
    /// </remarks>
    public interface IEngineInitializer
    {
        void Initialize(IEngine engine);
    }
}
