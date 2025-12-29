using System.Collections.Generic;
using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Tracks the state of files being written to and their source content.
    /// This helps determine when a file should be overwritten when using <see cref="CleanMode.Unwritten"/>.
    /// </summary>
    public interface IFileWriteTracker
    {
        /// <summary>
        /// Resets the tracking for another execution.
        /// </summary>
        void Reset();

        /// <summary>
        /// Saves the current state to a file.
        /// </summary>
        /// <param name="fileSystem">The current file system.</param>
        /// <param name="destinationFile">The file to save to.</param>
        Task SaveAsync(IReadOnlyFileSystem fileSystem, IFile destinationFile);

        /// <summary>
        /// Restores the state saved by <see cref="SaveAsync(IReadOnlyFileSystem, IFile)"/> to the previous state.
        /// </summary>
        /// <param name="fileSystem">The current file system.</param>
        /// <param name="sourceFile">The file to restore from.</param>
        /// <returns>A message if the file could not be restored, or null if the file was restored successfully.</returns>
        Task<string> RestoreAsync(IReadOnlyFileSystem fileSystem, IFile sourceFile);

        /// <summary>
        /// Tracks a written file using a hash code that represents the file state after the write operation.
        /// </summary>
        /// <param name="path">The path that was written to.</param>
        /// <param name="hashCode">A hash code that represents the file state.</param>
        /// <param name="actualWrite"><c>true</c> if a file what actually written, <c>false</c> if only tracking information was added (used primarily for reporting).</param>
        void TrackWrite(NormalizedPath path, int hashCode, bool actualWrite);

        /// <summary>
        /// Tracks data that was written using a hash code of it's content.
        /// </summary>
        /// <param name="path">The path that was written to.</param>
        /// <param name="hashCode">A hash code that represents the content.</param>
        void TrackContent(NormalizedPath path, int hashCode);

        /// <summary>
        /// Attempts to get a hash code for a written file.
        /// </summary>
        /// <param name="path">The path that was written to.</param>
        /// <param name="hashCode">A hash code that represents the written file.</param>
        /// <returns><c>true</c> if an entry exists for the given path in the writes, <c>false</c> otherwise.</returns>
        bool TryGetCurrentWrite(NormalizedPath path, out int hashCode);

        /// <summary>
        /// Attempts to get a hash code for written content.
        /// </summary>
        /// <param name="path">The path that was written to.</param>
        /// <param name="hashCode">A hash code that represents the written content.</param>
        /// <returns><c>true</c> if an entry exists for the given path in the content, <c>false</c> otherwise.</returns>
        bool TryGetCurrentContent(NormalizedPath path, out int hashCode);

        /// <summary>
        /// Attempts to get a hash code for a previously written file.
        /// </summary>
        /// <param name="path">The path that was written to.</param>
        /// <param name="hashCode">A hash code that represents the written file.</param>
        /// <returns><c>true</c> if an entry exists for the given path in the previous writes, <c>false</c> otherwise.</returns>
        bool TryGetPreviousWrite(NormalizedPath path, out int hashCode);

        /// <summary>
        /// Attempts to get a hash code for previously written content.
        /// </summary>
        /// <param name="path">The path that was written to.</param>
        /// <param name="hashCode">A hash code that represents the written content.</param>
        /// <returns><c>true</c> if an entry exists for the given path in the previous content, <c>false</c> otherwise.</returns>
        bool TryGetPreviousContent(NormalizedPath path, out int hashCode);

        /// <summary>
        /// Gets the path and hash code for all current file writes.
        /// </summary>
        IEnumerable<KeyValuePair<NormalizedPath, int>> CurrentWrites { get; }

        /// <summary>
        /// Gets the path and hash code for all previous file writes (from before the most recent <see cref="Reset"/>.
        /// </summary>
        IEnumerable<KeyValuePair<NormalizedPath, int>> PreviousWrites { get; }

        /// <summary>
        /// Gets the path and hash code for all current file content.
        /// </summary>
        IEnumerable<KeyValuePair<NormalizedPath, int>> CurrentContent { get; }

        /// <summary>
        /// Gets the path and hash code for all previous file content (from before the most recent <see cref="Reset"/>.
        /// </summary>
        IEnumerable<KeyValuePair<NormalizedPath, int>> PreviousContent { get; }

        /// <summary>
        /// Gets the count of how many files were actually written.
        /// </summary>
        public int CurrentActualWritesCount { get; }

        /// <summary>
        /// Gets the count of how many files were written, whether actually or already existed.
        /// </summary>
        public int CurrentTotalWritesCount { get; }
    }
}
