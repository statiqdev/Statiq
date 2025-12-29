using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// Cleans files and folders from the file system, usually before or after an execution
    /// using the <see cref="CleanMode"/> setting.
    /// </summary>
    public interface IFileCleaner
    {
        /// <summary>
        /// The currently set clean mode.
        /// </summary>
        CleanMode CleanMode { get; }

        /// <summary>
        /// Performs pre-execution clean up.
        /// </summary>
        Task CleanBeforeExecutionAsync();

        /// <summary>
        /// Performs post-execution clean up.
        /// </summary>
        Task CleanAfterExecutionAsync();

        /// <summary>
        /// Recursively deletes a directory and then recreates it.
        /// </summary>
        /// <param name="directory">The directory to clean.</param>
        /// <param name="name">A name for logging purposes.</param>
        void CleanDirectory(IDirectory directory, string name = null);
    }
}
