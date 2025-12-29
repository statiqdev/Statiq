using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Statiq.Core
{
    /// <summary>
    /// This class manages referenced assemblies. We need to do a little deconfliction
    /// because the <see cref="AppDomain"/> can contain multiple assemblies with the same
    /// simple name but Roslyn doesn't like that: <c>Metadata: CS1704: An assembly with the
    /// same simple name 'LINQPadQuery' has already been imported. Try removing one of the
    /// references (e.g. ...) or sign them to enable side-by-side.</c> This shows up in
    /// LINQPad particularly because it loads multiple scripts with the same simple name
    /// into the same <see cref="AppDomain"/>. We can only provide a single assembly
    /// per simple name to Roslyn, so use the highest version (and give preference to
    /// the calling assemblies). See also https://github.com/dotnet/roslyn/issues/5657.
    /// </summary>
    internal class CompilationReferences : IEnumerable<Assembly>
    {
        private readonly Dictionary<string, Assembly> _referencesBySimpleName = new Dictionary<string, Assembly>();

        public bool TryAddReference(Assembly assembly, bool force = false)
        {
            // If no location or if it's a dynamic assembly, just ignore
            AssemblyName assemblyName = assembly?.GetName();
            if (assemblyName is null || assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location))
            {
                return false;
            }

            // We can keep the existing one if it's the exact same location or it has a higher version
            if (!force
                && _referencesBySimpleName.TryGetValue(assemblyName.Name, out Assembly existing)
                && (existing.Location.Equals(assembly.Location) || existing.GetName()?.Version >= assemblyName?.Version))
            {
                return false;
            }

            // Add the assembly
            _referencesBySimpleName[assemblyName.Name] = assembly;
            return true;
        }

        public IEnumerator<Assembly> GetEnumerator() => _referencesBySimpleName.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
