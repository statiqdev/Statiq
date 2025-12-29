using System;

namespace Statiq.Common
{
    /// <summary>
    /// An exception that is thrown when there is an error during execution.
    /// </summary>
    public class ExecutionException : Exception
    {
        public ExecutionException(string message)
            : base(message)
        {
        }
    }
}
