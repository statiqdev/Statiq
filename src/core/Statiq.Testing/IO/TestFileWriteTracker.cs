using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ConcurrentCollections;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestFileWriteTracker : IFileWriteTracker
    {
        private readonly ConcurrentHashSet<NormalizedPath> _currentActualWrites = new ConcurrentHashSet<NormalizedPath>();

        private ConcurrentDictionary<NormalizedPath, int> _currentWrites
            = new ConcurrentDictionary<NormalizedPath, int>();

        private ConcurrentDictionary<NormalizedPath, int> _currentContent
            = new ConcurrentDictionary<NormalizedPath, int>();

        private ConcurrentDictionary<NormalizedPath, int> _previousWrites
            = new ConcurrentDictionary<NormalizedPath, int>();

        private ConcurrentDictionary<NormalizedPath, int> _previousContent
            = new ConcurrentDictionary<NormalizedPath, int>();

        public void Reset()
        {
            _currentActualWrites.Clear();
            _previousWrites = _currentWrites;
            _currentWrites = new ConcurrentDictionary<NormalizedPath, int>();
            _previousContent = _currentContent;
            _currentContent = new ConcurrentDictionary<NormalizedPath, int>();
        }

        public Task SaveAsync(IReadOnlyFileSystem fileSystem, IFile destinationFile) => throw new NotImplementedException();

        public Task<string> RestoreAsync(IReadOnlyFileSystem fileSystem, IFile sourceFile) => throw new NotImplementedException();

        public void TrackWrite(NormalizedPath path, int hashCode, bool actualWrite)
        {
            _currentWrites[path] = hashCode;
            if (actualWrite)
            {
                _currentActualWrites.Add(path);
            }
        }

        public void TrackContent(NormalizedPath path, int hashCode) => _currentContent[path] = hashCode;

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
