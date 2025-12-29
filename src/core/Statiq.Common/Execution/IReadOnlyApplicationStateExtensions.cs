using System;
using System.Collections.Generic;

namespace Statiq.Common
{
    public static class IReadOnlyApplicationStateExtensions
    {
        /// <summary>
        /// Determines if the application was run with the specified command.
        /// </summary>
        /// <param name="applicationState">The application state.</param>
        /// <param name="command">The command.</param>
        /// <returns><c>true</c> if the application was run with the specified command, <c>false</c> otherwise.</returns>
        public static bool IsCommand(this IApplicationState applicationState, string command) =>
            applicationState.CommandName.Equals(command, StringComparison.OrdinalIgnoreCase);
    }
}
