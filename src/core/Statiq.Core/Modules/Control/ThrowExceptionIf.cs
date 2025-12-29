using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Throws an exception if a condition is <c>true</c>.
    /// </summary>
    /// <category name="Control" />
    public class ThrowExceptionIf : ThrowException
    {
        public ThrowExceptionIf(Config<bool> condition)
            : base(condition.CombineWith(Config.FromValue("Condition was false"), (c, m) => c ? m : null))
        {
        }

        /// <summary>
        /// Throws the specified exception if the condition is <c>true</c>.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="exception">The exception to throw or <c>null</c> not to throw an exception.</param>
        public ThrowExceptionIf(Config<bool> condition, Config<Exception> exception)
            : base(condition.CombineWith(exception, (c, e) => c ? e : null))
        {
        }

        /// <summary>
        /// Throws a <see cref="ExecutionException"/> with the specified message if the condition is <c>true</c>.
        /// </summary>
        /// <param name="condition">The condition to check.</param>
        /// <param name="message">The message of the exception to throw or <c>null</c> not to throw an exception.</param>
        public ThrowExceptionIf(Config<bool> condition, Config<string> message)
            : base(condition.CombineWith(message, (c, m) => c ? m : null))
        {
        }
    }
}