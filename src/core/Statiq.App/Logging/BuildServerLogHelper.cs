using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    internal class BuildServerLogHelper
    {
        private static readonly Dictionary<string, (string Prefix, string FilePrefix, string Suffix)> _buildServers =
            new Dictionary<string, (string Prefix, string FilePrefix, string Suffix)>
            {
                { "TF_BUILD", ("##vso[task.logissue type=", ";sourcepath=", "]") },
                { "GITHUB_ACTIONS", ("::", " file=", "::") }
            };

        private readonly IDirectory _repositoryDirectory;
        private readonly string _prefix;
        private readonly string _filePrefix;
        private readonly string _suffix;

        internal BuildServerLogHelper(IReadOnlyFileSystem fileSystem)
        {
            // Get the current build server
            KeyValuePair<string, (string Prefix, string FilePrefix, string Suffix)> match = _buildServers.FirstOrDefault(x => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(x.Key)));
            if (match.Key is object)
            {
                _prefix = match.Value.Prefix;
                _filePrefix = match.Value.FilePrefix;
                _suffix = match.Value.Suffix;

                // Get the repository directory
                if (fileSystem is object)
                {
                    _repositoryDirectory = fileSystem.GetRootDirectory();
                    while (_repositoryDirectory is object && _repositoryDirectory.GetDirectory(".git") is null)
                    {
                        _repositoryDirectory = _repositoryDirectory.Parent;
                    }
                }
            }
        }

        public bool IsBuildServer => _prefix is object;

        public string GetMessage(LogLevel logLevel, StatiqLogState documentLogState, string message)
        {
            // Return null if we're not on a build server or the log level doesn't meet the threshold
            if (_prefix is null || logLevel == LogLevel.None || logLevel < LogLevel.Warning)
            {
                return null;
            }

            StringBuilder builder = new StringBuilder(_prefix);
            builder.Append(logLevel == LogLevel.Warning ? "warning" : "error");
            if (documentLogState is object && documentLogState.Document is object && !documentLogState.Document.Source.IsNullOrEmpty && _repositoryDirectory is object)
            {
                NormalizedPath relativePath = _repositoryDirectory.Path.GetRelativePath(documentLogState.Document.Source);
                if (!relativePath.IsNullOrEmpty)
                {
                    builder.Append(_filePrefix);
                    builder.Append(relativePath.FullPath);
                }
            }
            builder.Append(_suffix);
            builder.Append(message);

            return builder.ToString();
        }
    }
}
