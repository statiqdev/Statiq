using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConcurrentCollections;
using Statiq.Common;

namespace Statiq.Core
{
    internal class FileWriteTracker : IFileWriteTracker
    {
        private readonly ConcurrentHashSet<NormalizedPath> _currentActualWrites = new ConcurrentHashSet<NormalizedPath>();

        private readonly IFileSystem _fileSystem;

        private ConcurrentDictionary<NormalizedPath, int> _currentWrites
            = new ConcurrentDictionary<NormalizedPath, int>();

        private ConcurrentDictionary<NormalizedPath, int> _currentContent
            = new ConcurrentDictionary<NormalizedPath, int>();

        private ConcurrentDictionary<NormalizedPath, int> _previousWrites
            = new ConcurrentDictionary<NormalizedPath, int>();

        private ConcurrentDictionary<NormalizedPath, int> _previousContent
            = new ConcurrentDictionary<NormalizedPath, int>();

        public FileWriteTracker(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem.ThrowIfNull(nameof(fileSystem));
        }

        public void Reset()
        {
            _currentActualWrites.Clear();
            _previousWrites = _currentWrites;
            _currentWrites = new ConcurrentDictionary<NormalizedPath, int>();
            _previousContent = _currentContent;
            _currentContent = new ConcurrentDictionary<NormalizedPath, int>();
        }

        public async Task SaveAsync(IReadOnlyFileSystem fileSystem, IFile destination)
        {
            TrackerState state = new TrackerState
            {
                Writes = _currentWrites.ToDictionary(x => fileSystem.GetRelativeOutputPathOrSelf(x.Key).FullPath, x => x.Value.ToString()),
                Content = _currentContent.ToDictionary(x => fileSystem.GetRelativeOutputPathOrSelf(x.Key).FullPath, x => x.Value.ToString())
            };
            await destination.SerializeJsonAsync(state);
        }

        public async Task<string> RestoreAsync(IReadOnlyFileSystem fileSystem, IFile source)
        {
            if (!source.Exists)
            {
                return "File does not exist";
            }

            try
            {
                TrackerState state = await source.DeserializeJsonAsync<TrackerState>();
                _previousWrites = new ConcurrentDictionary<NormalizedPath, int>(
                    state.Writes.Select(x => new KeyValuePair<NormalizedPath, int>(fileSystem.GetOutputPath(x.Key), int.Parse(x.Value))));
                _previousContent = new ConcurrentDictionary<NormalizedPath, int>(
                    state.Content.Select(x => new KeyValuePair<NormalizedPath, int>(fileSystem.GetOutputPath(x.Key), int.Parse(x.Value))));
            }
            catch (Exception ex)
            {
                // If we have an error during restore (like deserialization), just ignore it
                _previousWrites = new ConcurrentDictionary<NormalizedPath, int>();
                _previousContent = new ConcurrentDictionary<NormalizedPath, int>();
                return ex.Message;
            }

            return null;
        }

        private class TrackerState
        {
            public Dictionary<string, string> Writes { get; set; }
            public Dictionary<string, string> Content { get; set; }
        }

        public void TrackWrite(NormalizedPath path, int hashCode, bool actualWrite)
        {
            // Don't track writes to temp or cache paths
            if (_fileSystem.GetTempPath().ContainsDescendantOrSelf(path)
                || _fileSystem.GetCachePath().ContainsDescendantOrSelf(path))
            {
                return;
            }

            _currentWrites[path] = hashCode;
            if (actualWrite)
            {
                _currentActualWrites.Add(path);
            }
        }

        public void TrackContent(NormalizedPath path, int hashCode)
        {
            // Don't track writes to temp or cache paths
            if (_fileSystem.GetTempPath().ContainsDescendantOrSelf(path)
                || _fileSystem.GetCachePath().ContainsDescendantOrSelf(path))
            {
                return;
            }

            _currentContent[path] = hashCode;
        }

        public bool TryGetCurrentWrite(NormalizedPath path, out int hashCode) => _currentWrites.TryGetValue(path, out hashCode);

        public bool TryGetCurrentContent(NormalizedPath path, out int hashCode) => _currentContent.TryGetValue(path, out hashCode);

        public bool TryGetPreviousWrite(NormalizedPath path, out int hashCode) => _previousWrites.TryGetValue(path, out hashCode);

        public bool TryGetPreviousContent(NormalizedPath path, out int hashCode) => _previousContent.TryGetValue(path, out hashCode);

        public IEnumerable<KeyValuePair<NormalizedPath, int>> CurrentWrites => _currentWrites.ToArray();

        public IEnumerable<KeyValuePair<NormalizedPath, int>> PreviousWrites => _previousWrites.ToArray();

        public IEnumerable<KeyValuePair<NormalizedPath, int>> CurrentContent => _currentContent.ToArray();

        public IEnumerable<KeyValuePair<NormalizedPath, int>> PreviousContent => _previousContent.ToArray();

        public int CurrentActualWritesCount => _currentActualWrites.Count;

        public int CurrentTotalWritesCount => _currentWrites.Count;
    }
}
