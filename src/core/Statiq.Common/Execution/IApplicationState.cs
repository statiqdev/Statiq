using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents the state of the application when it was run.
    /// </summary>
    public interface IApplicationState
    {
        /// <summary>
        /// Gets the raw arguments passed to the application (the first argument is typically the "command").
        /// </summary>
        /// <remarks>
        /// The value will never be null (though it may be empty).
        /// </remarks>
        public string[] Arguments { get; }

        /// <summary>
        /// The CLI command that was run.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Gets any input that was passed to the application (for example, on stdin via piping).
        /// </summary>
        /// <remarks>
        /// The value may be null if unset or if no input was passed to the application.
        /// </remarks>
        public string Input { get; }
    }
}