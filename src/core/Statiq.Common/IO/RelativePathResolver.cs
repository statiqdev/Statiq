using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    internal static class RelativePathResolver
    {
        public static NormalizedPath Resolve(in NormalizedPath source, in NormalizedPath target)
        {
            source.ThrowIfNull(nameof(source));
            target.ThrowIfNull(nameof(target));

            // Make sure they're both either relative or absolute
            if (source.IsAbsolute != target.IsAbsolute)
            {
                throw new ArgumentException("Paths must both be relative or both be absolute");
            }

            // Special case if source is dot
            if (source.Segments.Length == 1
                && source.Segments[0].SequenceEqual(NormalizedPath.Dot.ToMemory())
                && target.Segments.Length != 0)
            {
                return target;
            }

            // Check if they're the same path
            if (source.FullPath == target.FullPath)
            {
                return NormalizedPath.Empty;
            }

            // Special case if source is just root
            if (source.IsAbsolute && source == NormalizedPath.AbsoluteRoot)
            {
                return new NormalizedPath(string.Join(NormalizedPath.Slash, target.Segments));
            }

            // If target is empty return source
            if (target.IsNullOrEmpty)
            {
                return source;
            }

            // Check if source is empty or they share the same root
            if (target.Segments.Length == 0
                || source.Segments.Length == 0
                || !source.Segments[0].SequenceEqual(target.Segments[0]))
            {
                return target;
            }

            int minimumSegmentsLength = Math.Min(source.Segments.Length, target.Segments.Length);

            int lastCommonRoot = -1;

            // Find common root
            for (int x = 0; x < minimumSegmentsLength; x++)
            {
                if (!source.Segments[x].SequenceEqual(target.Segments[x]))
                {
                    break;
                }

                lastCommonRoot = x;
            }

            if (lastCommonRoot == -1)
            {
                return target;
            }

            // Add relative folders in from path
            List<ReadOnlyMemory<char>> relativeSegments = new List<ReadOnlyMemory<char>>();
            for (int x = lastCommonRoot + 1; x < source.Segments.Length; x++)
            {
                if (source.Segments[x].Length > 0)
                {
                    relativeSegments.Add(NormalizedPath.DotDot.AsMemory());
                }
            }

            // Add to folders to path
            for (int x = lastCommonRoot + 1; x < target.Segments.Length; x++)
            {
                relativeSegments.Add(target.Segments[x]);
            }

            // Create relative path
            return new NormalizedPath(string.Join("/", relativeSegments));
        }
    }
}
