namespace Statiq.Common
{
    public enum CleanMode
    {
        /// <summary>
        /// Cleans the entire output folder before the initial execution,
        /// then cleans only those files written or copied during the previous
        /// execution before each following execution.
        /// </summary>
        Self,

        /// <summary>
        /// Does not clean the output folder before each execution.
        /// </summary>
        None,

        /// <summary>
        /// Cleans the entire output folder before each execution.
        /// </summary>
        Full,

        /// <summary>
        /// Cleans files after each execution that were written or copied
        /// during the previous execution but not during the current
        /// execution. This mode also uses content hashing and file
        /// attributes to avoid copying files when there's already a
        /// duplicate file in the output folder.
        /// </summary>
        Unwritten
    }
}
