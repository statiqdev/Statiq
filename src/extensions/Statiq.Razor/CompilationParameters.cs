using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// Used as a cache key for <see cref="RazorCompiler"/> instances.
    /// </summary>
    internal class CompilationParameters : IEquatable<CompilationParameters>
    {
        public static CompilationParameters Get(int namespacesCacheCode, Type basePageType, bool isDocumentModel)
        {
            string basePageTypeName = GetBasePageType(basePageType);
            CacheCode cacheCode = new CacheCode();
            cacheCode.Add(namespacesCacheCode);
            cacheCode.Add(basePageTypeName);
            cacheCode.Add(isDocumentModel);
            return new CompilationParameters
            {
                BasePageType = basePageTypeName,
                IsDocumentModel = isDocumentModel,
                CacheCode = cacheCode.ToCacheCode()
            };
        }

        // Setters for JSON deserialization, don't actually mutate this object

        public string BasePageType { get; set; }

        public bool IsDocumentModel { get;  set; }

        public int CacheCode { get; set; }

        public override int GetHashCode() => CacheCode;

        public override bool Equals(object obj) => Equals(obj as CompilationParameters);

        public bool Equals([AllowNull] CompilationParameters other)
        {
            if (other is null || other.CacheCode != CacheCode)
            {
                return false;
            }
            return BasePageType == other.BasePageType && IsDocumentModel == other.IsDocumentModel;
        }

        /// <summary>
        /// Gets the type string for the base page type so it can be injected into the page source code by the <see cref="StatiqDocumentClassifierPhase"/>.
        /// </summary>
        private static string GetBasePageType(Type basePageType)
        {
            // We need to distinguish between the default base type and an explicit base type that happens to be the same generic class, so return null for the default
            if (basePageType is null)
            {
                return null;
            }

            string baseType = basePageType.ToString();

            // Open generic
            if (basePageType.IsGenericTypeDefinition)
            {
                if (basePageType.GenericTypeArguments.Length > 1)
                {
                    throw new ArgumentException($"Open generic Razor base pages should only have a single generic type argument, {baseType} has {basePageType.GenericTypeArguments.Length}");
                }
                int tickIndex = baseType.IndexOf('`');
                return $"{baseType.Substring(0, tickIndex)}<TModel>";
            }

            // Closed generic
            if (basePageType.IsGenericType)
            {
                int tickIndex = baseType.IndexOf('`');
                int openBraceIndex = baseType.IndexOf('[');
                return $"{baseType.Substring(0, tickIndex)}<{baseType.Substring(openBraceIndex + 1, baseType.Length - openBraceIndex - 2)}>";
            }

            // Regular type
            return baseType;
        }
    }
}