using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    public static class GuardExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T ThrowIfNull<T>(this T obj, string paramName) =>
            obj is null ? throw new ArgumentNullException(paramName) : obj;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ThrowIfNullOrEmpty(this string str, string paramName)
        {
            str.ThrowIfNull(paramName);
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException(paramName + " cannot be empty");
            }
            return str;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ThrowIfNullOrWhiteSpace(this string str, string paramName)
        {
            str.ThrowIfNull(paramName);
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException(paramName + " cannot be empty or white space");
            }
            return str;
        }
    }
}