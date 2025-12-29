using System;

namespace Statiq.Razor
{
    /// <summary>
    /// A key for generated assemblies which is a JSON serializable combination
    /// of <see cref="CompilationParameters"/> and <see cref="CompilerCacheKey"/>.
    /// </summary>
    internal class AssemblyCacheKey : IEquatable<AssemblyCacheKey>
    {
        public static AssemblyCacheKey Get(CompilationParameters compilationParameters, CompilerCacheKey compilerCacheKey)
        {
            Common.CacheCode cacheCode = new Common.CacheCode();
            cacheCode.Add(compilationParameters.CacheCode);
            cacheCode.Add(compilerCacheKey.CacheCode);
            return new AssemblyCacheKey
            {
                CompilationParameters = compilationParameters,
                CompilerCacheKey = compilerCacheKey,
                CacheCode = cacheCode.ToCacheCode()
            };
        }

        // Setters for JSON deserialization, don't actually mutate this object

        public CompilationParameters CompilationParameters { get; set; }

        public CompilerCacheKey CompilerCacheKey { get; set; }

        public int CacheCode { get; set; }

        public override int GetHashCode() => CacheCode;

        public override bool Equals(object obj) => Equals(obj as AssemblyCacheKey);

        public bool Equals(AssemblyCacheKey other)
        {
            if (other is null || other.CacheCode != CacheCode)
            {
                return false;
            }
            return CompilationParameters.Equals(other.CompilationParameters)
                && CompilerCacheKey.Equals(other.CompilerCacheKey);
        }
    }
}