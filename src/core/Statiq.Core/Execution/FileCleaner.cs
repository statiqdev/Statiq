using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    public class FileCleaner : IFileCleaner
    {
        private const string CacheFileName = "writecache.json";

        private readonly IReadOnlySettings _settings;
        private readonly IReadOnlyFileSystem _fileSystem;
        private readonly ILogger _logger;

        private bool _firstExecution = true;

        public FileCleaner(IReadOnlySettings settings, IReadOnlyFileSystem fileSystem, ILogger logger)
        {
            _settings = settings.ThrowIfNull(nameof(fileSystem));
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
            _logger = logger.ThrowIfNull(nameof(logger));
        }

        /// <inheritdoc/>
        public virtual CleanMode CleanMode => _settings.Get(Keys.CleanMode, CleanMode.Unwritten);

        /// <inheritdoc/>
        public virtual async Task CleanBeforeExecutionAsync()
        {
            _fileSystem.WriteTracker.Reset();

            // Always clean the temp directory between executions
            CleanDirectory(_fileSystem.GetTempDirectory(), "temp");

            // If this is the first execution and we're caching, see if we've got a write tracker cache
            if (_firstExecution && _settings.GetBool(Keys.UseCache))
            {
                IFile cacheFile = _fileSystem.GetCacheFile(CacheFileName);
                string result = await _fileSystem.WriteTracker.RestoreAsync(_fileSystem, cacheFile);
                if (result is null)
                {
                    // If we were able to restore, don't treat this as the first execution anymore
                    _logger.LogInformation($"Restored write tracking data from {cacheFile}");
                    _firstExecution = false;
                }
                else
                {
                    _logger.LogDebug($"Could not restore write tracking data from {cacheFile}: {result}");
                }
            }

            // Clean if we need to
            if (CleanMode != CleanMode.None)
            {
                if (CleanMode == CleanMode.Full || _firstExecution)
                {
                    CleanDirectory(_fileSystem.GetOutputDirectory(), "output");
                }
                else if (CleanMode == CleanMode.Self)
                {
                    CleanSelf();
                }
            }

            _firstExecution = false;
        }

        /// <inheritdoc/>
        public virtual async Task CleanAfterExecutionAsync()
        {
            if (CleanMode == CleanMode.Unwritten)
            {
                CleanUnwritten();
            }

            // Save the write tracker state, but only if we're caching
            if (_settings.GetBool(Keys.UseCache))
            {
                IFile cacheFile = _fileSystem.GetCacheFile(CacheFileName);
                await _fileSystem.WriteTracker.SaveAsync(_fileSystem, cacheFile);
                _logger.LogDebug($"Saved write tracking data to {cacheFile}");
            }
        }

        /// <inheritdoc/>
        public virtual void CleanDirectory(IDirectory directory, string name = null)
        {
            _ = directory.ThrowIfNull(nameof(directory));

            name = name is null ? string.Empty : (name + " ");
            try
            {
                _logger.LogDebug($"Cleaning {name}directory {directory.Path.FullPath}...");
                if (directory.Exists)
                {
                    directory.Delete(true);
                }
                directory.Create();
                _logger.LogInformation($"Cleaned {name}directory {directory.Path.FullPath}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while cleaning {name}directory {directory.Path.FullPath}: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Deletes all files we wrote on the last execution.
        /// Called before execution.
        /// </summary>
        private void CleanSelf()
        {
            IDirectory directory = _fileSystem.GetOutputDirectory();

            try
            {
                _logger.LogDebug($"Cleaning files written to output directory {directory.Path.FullPath} during previous execution...");
                int count = 0;
                foreach (KeyValuePair<NormalizedPath, int> write in _fileSystem.WriteTracker.PreviousWrites)
                {
                    if (directory.Path.ContainsDescendant(write.Key))
                    {
                        IFile file = _fileSystem.GetFile(write.Key);
                        if (file.Exists)
                        {
                            file.Delete();
                            count++;
                        }
                    }
                }
                _logger.LogInformation($"Cleaned {count} files written to output directory {directory.Path.FullPath} during previous execution");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while cleaning files written to output directory {directory.Path.FullPath} during previous execution: {0} - {1}", ex.GetType(), ex.Message);
            }
        }

        /// <summary>
        /// Compares what we wrote this time to what we wrote last time and deletes the files that we didn't write this time.
        /// Called after execution.
        /// </summary>
        private void CleanUnwritten()
        {
            IDirectory directory = _fileSystem.GetOutputDirectory();

            try
            {
                _logger.LogDebug($"Cleaning files not written to output directory {directory.Path.FullPath} during current execution...");
                int count = 0;
                foreach (KeyValuePair<NormalizedPath, int> write in _fileSystem.WriteTracker.PreviousWrites)
                {
                    if (directory.Path.ContainsDescendant(write.Key))
                    {
                        IFile file = _fileSystem.GetFile(write.Key);

                        // Only delete it if the file exists and was not written during the current execution
                        if (file.Exists && !_fileSystem.WriteTracker.TryGetCurrentWrite(file.Path, out int _))
                        {
                            file.Delete();
                            count++;
                        }
                    }
                }
                _logger.LogInformation($"Cleaned {count} files not written to output directory {directory.Path.FullPath} during current execution");
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Error while cleaning files not written to output directory {directory.Path.FullPath} during current execution: {0} - {1}", ex.GetType(), ex.Message);
            }
        }
    }
}