using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Optimizes a file name.
    /// </summary>
    /// <remarks>
    /// This module takes the destination file name (or the value of a specified
    /// metadata key) and optimizes it by removing reserved characters, white-listing characters, etc.
    /// </remarks>
    /// <category name="Metadata" />
    public class OptimizeFileName : ParallelSyncMultiConfigModule
    {
        // Config keys
        private const string Path = nameof(Path); // null = Destination
        private const string OutputKey = nameof(OutputKey); // null = Destination
        private const string ReservedChars = nameof(ReservedChars);
        private const string TrimDotKey = nameof(TrimDotKey);
        private const string CollapseSpacesKey = nameof(CollapseSpacesKey);
        private const string SpacesToDashesKey = nameof(SpacesToDashesKey);
        private const string ToLowerKey = nameof(ToLowerKey);

        /// <summary>
        /// Optimizes the destination file name of each input document.
        /// </summary>
        public OptimizeFileName()
            : base(null, true)
        {
        }

        /// <summary>
        /// Optimizes the file name stored in the given metadata key and stores it back in the same key.
        /// </summary>
        /// <param name="key">The key containing the file name to optimize.</param>
        public OptimizeFileName(string key)
            : base(
                new Dictionary<string, IConfig>
                {
                    { Path, Config.FromDocument<NormalizedPath>(key) },
                    { OutputKey, Config.FromValue(key) }
                },
                true)
        {
            key.ThrowIfNull(nameof(key));
        }

        /// <summary>
        /// Optimizes the file name stored in the given metadata key and stores it at the provided key.
        /// </summary>
        /// <param name="inputKey">The metadata key to use for the input file name.</param>
        /// <param name="outputKey">The metadata key to use for the optimized file name.</param>
        public OptimizeFileName(string inputKey, string outputKey)
            : base(
                  new Dictionary<string, IConfig>
                  {
                      { Path, Config.FromDocument<NormalizedPath>(inputKey) },
                      { OutputKey, Config.FromValue(outputKey) }
                  },
                  true)
        {
            inputKey.ThrowIfNull(nameof(inputKey));
            outputKey.ThrowIfNull(nameof(outputKey));
        }

        /// <summary>
        /// Optimizes the file name in the resulting path and sets the specified metadata key.
        /// </summary>
        /// <param name="path">A delegate that should return a <see cref="NormalizedPath"/> with a file name to optimize.</param>
        /// <param name="outputKey">The metadata key to use for the optimized file name.</param>
        public OptimizeFileName(Config<NormalizedPath> path, string outputKey)
            : base(
                  new Dictionary<string, IConfig>
                  {
                      { Path, path },
                      { OutputKey, Config.FromValue(outputKey) }
                  },
                  true)
        {
            path.ThrowIfNull(nameof(path));
            outputKey.ThrowIfNull(nameof(outputKey));
        }

        /// <summary>
        /// Specifies the characters not to allow in the file name.
        /// </summary>
        /// <param name="reservedCharacters">The reserved characters.</param>
        /// <returns>The current module instance.</returns>
        public OptimizeFileName WithReservedCharacters(Config<string> reservedCharacters) => (OptimizeFileName)SetConfig(ReservedChars, reservedCharacters);

        public OptimizeFileName TrimDot(Config<bool> trimDot) => (OptimizeFileName)SetConfig(TrimDotKey, trimDot);

        public OptimizeFileName CollapseSpaces(Config<bool> collapseSpaecs) => (OptimizeFileName)SetConfig(CollapseSpacesKey, collapseSpaecs);

        public OptimizeFileName SpacesToDashes(Config<bool> spacesToDashes) => (OptimizeFileName)SetConfig(SpacesToDashesKey, spacesToDashes);

        public OptimizeFileName ToLower(Config<bool> toLower) => (OptimizeFileName)SetConfig(ToLowerKey, toLower);

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, IMetadata values)
        {
            // Get file name
            NormalizedPath path = values.GetPath(Path);
            if (path.IsNull)
            {
                if (input.Destination.IsNull)
                {
                    return input.Yield();
                }
                path = input.Destination;
            }
            if (path.IsNull)
            {
                return input.Yield();
            }

            // Get params
            string reservedChars = values.GetString(ReservedChars, NormalizedPath.OptimizeFileNameReservedChars);
            bool trimDot = values.GetBool(TrimDotKey, true);
            bool collapseSpaces = values.GetBool(CollapseSpacesKey, true);
            bool spacesToDashes = values.GetBool(SpacesToDashesKey, true);
            bool toLower = values.GetBool(ToLowerKey, true);
            string outputKey = values.GetString(OutputKey);

            // Optimize the file name in the path
            path = path.OptimizeFileName(reservedChars, trimDot, collapseSpaces, spacesToDashes, toLower);
            if (string.IsNullOrWhiteSpace(outputKey))
            {
                // No output key so set the destination
                return input.Clone(path).Yield();
            }

            // Set the output key with the optimized path
            return input.Clone(
                new MetadataItems
                {
                    { outputKey, path }
                })
                .Yield();
        }
    }
}