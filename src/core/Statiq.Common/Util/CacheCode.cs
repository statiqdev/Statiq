using System;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A similar API to <see cref="HashCode"/> but deterministic
    /// (does not use a random seed) which makes it suitable for
    /// persistent saved cache keys.
    /// </summary>
    public class CacheCode
    {
        // A little context on the convention of 23 and 31:
        // https://stackoverflow.com/questions/5154970/how-do-i-create-a-hashcode-in-net-c-for-a-string-that-is-safe-to-store-in-a#comment51023108_5155015

        private int _cacheCode = 23;

        // All other Add() methods should call this one
        public void Add(int value) => _cacheCode = (_cacheCode * 31) + value;

        public void Add(string value) => Add(value is null ? 0 : (int)Crc32.Calculate(value));

        public void Add(bool value) => Add(value ? 1 : 2); // Don't use 0 for false so it's distinct from a null object

        public void Add(long value) => Add((int)value);

        public void Add(uint value) => Add((int)value);

        public void Add(DateTime value) => Add(value.Ticks);

        public async Task AddAsync(ICacheCode value) => Add(value is null ? 0 : await value.GetCacheCodeAsync());

        public int ToCacheCode() => _cacheCode;
    }
}
