namespace Statiq.Common
{
    public static class IModuleExtensions
    {
        public static ForEachDocument ForEachDocument(this IModule module) => new ForEachDocument(module);
    }
}
