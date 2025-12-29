using System.Threading.Tasks;

namespace Statiq.Common
{
    /// <summary>
    /// A provider that can instantiate <see cref="IFile"/> and <see cref="IDirectory"/>
    /// objects from their paths.
    /// </summary>
    public interface IFileProvider
    {
        /// <summary>
        /// Gets a file from a specified path.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The file.</returns>
        IFile GetFile(NormalizedPath path);

        /// <summary>
        /// Gets a directory from a specified path.
        /// </summary>
        /// <param name="path">The path to the directory.</param>
        /// <returns>The directory.</returns>
        IDirectory GetDirectory(NormalizedPath path);
    }
}
