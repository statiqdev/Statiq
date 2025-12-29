using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Outputs log messages during execution.
    /// </summary>
    /// <remarks>
    /// This module has no effect on documents and the input documents are passed through to output documents.
    /// </remarks>
    /// <category name="Extensibility" />
    public class LogMessage : SyncConfigModule<string>
    {
        private LogLevel _logLevel = LogLevel.Information;

        /// <summary>
        /// Logs the string value of the returned object. This allows
        /// you to log different content for each document depending on the input document.
        /// </summary>
        /// <param name="content">A delegate that returns the content to context.Log.</param>
        public LogMessage(Config<string> content)
            : base(content, false)
        {
        }

        /// <summary>
        /// Logs the string value of the returned object with a specified log level. This allows
        /// you to log different content for each document depending on the input document.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="content">A delegate that returns the content to context.Log.</param>
        public LogMessage(LogLevel logLevel, Config<string> content)
            : base(content, false)
        {
            _logLevel = logLevel;
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, string value)
        {
            context.Log(_logLevel, value);
            return input is null ? context.Inputs : input.Yield();
        }
    }
}