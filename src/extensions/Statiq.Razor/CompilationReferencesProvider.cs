using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Statiq.Common;

namespace Statiq.Razor
{
    // Need to use a custom ICompilationReferencesProvider because the default one won't provide a path for the running assembly
    // (see Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPartExtensions.GetReferencePaths() for why,
    // the running assembly returns a DependencyContext when used with "dotnet run" and therefore won't return it's own path)
    // We also need to lazily calculate the reference paths so that late additions to the ClassCatalog get
    // picked up (like compiled theme projects)
    internal class CompilationReferencesProvider : ApplicationPart, ICompilationReferencesProvider
    {
        private static readonly object ReferencePathLock = new object();

        private readonly ClassCatalog _classCatalog;

        private string[] _referencePaths;

        public CompilationReferencesProvider(ClassCatalog classCatalog)
        {
            _classCatalog = classCatalog;
        }

        public override string Name => nameof(CompilationReferencesProvider);

        public IEnumerable<string> GetReferencePaths()
        {
            lock (ReferencePathLock)
            {
                if (_referencePaths is null)
                {
                    HashSet<Assembly> assemblies = new HashSet<Assembly>(new AssemblyComparer());
                    assemblies.AddRange((_classCatalog ?? new ClassCatalog()).GetAssemblies());

                    // And a couple needed assemblies that might not be loaded in the AppDomain yet
                    assemblies.Add(typeof(IHtmlContent).Assembly);
                    assemblies.Add(Assembly.Load(new AssemblyName("Microsoft.CSharp")));

                    _referencePaths = assemblies
                        .Where(x => !x.IsDynamic && !string.IsNullOrEmpty(x.Location))
                        .Select(x => x.Location)
                        .ToArray();
                }

                return _referencePaths;
            }
        }
    }
}