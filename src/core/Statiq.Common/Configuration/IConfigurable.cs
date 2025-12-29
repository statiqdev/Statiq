using System.Diagnostics.CodeAnalysis;

namespace Statiq.Common
{
    /// <summary>
    /// A marker interface that indicates a given object can be configured by the bootstrapper.
    /// </summary>
#pragma warning disable CA1040
    public interface IConfigurable
    {
    }
#pragma warning restore CA1040
}