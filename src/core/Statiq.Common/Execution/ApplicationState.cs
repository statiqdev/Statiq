using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    /// <summary>
    /// Represents the state of the application when it was run.
    /// </summary>
    public class ApplicationState : IApplicationState
    {
        public ApplicationState(string[] arguments, string commandName, string input)
        {
            Arguments = arguments ?? new string[] { };
            CommandName = commandName;
            Input = input;
        }

        /// <inheritdoc />
        public string[] Arguments { get; }

        /// <inheritdoc />
        public string CommandName { get; }

        /// <inheritdoc />
        public string Input { get; }
    }
}