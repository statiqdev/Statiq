using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Copies the specified meta key to a new meta key, with an optional format argument.
    /// </summary>
    /// <category name="Metadata" />
    public class CopyMetadata : ParallelModule
    {
        private readonly string _fromKey;
        private readonly string _toKey;
        private string _format;
        private Func<string, string> _formatFunc;

        /// <summary>
        /// The specified object in fromKey is copied to toKey. If a format is provided, the fromKey value is processed through string.Format before being copied (if the existing value is a DateTime, the format is passed as the argument to ToString).
        /// </summary>
        /// <param name="fromKey">The metadata key to copy from.</param>
        /// <param name="toKey">The metadata key to copy to.</param>
        /// <param name="format">The formatting to apply to the new value.</param>
        public CopyMetadata(string fromKey, string toKey, string format = null)
        {
            _fromKey = fromKey.ThrowIfNull(nameof(fromKey));
            _toKey = toKey.ThrowIfNull(nameof(toKey));
            _format = format;
        }

        /// <summary>
        /// Specifies the format to use when copying the value.
        /// </summary>
        /// <param name="format">The format to use.</param>
        /// <returns>The current module instance.</returns>
        public CopyMetadata WithFormat(string format)
        {
            _format = format.ThrowIfNull(nameof(format));
            return this;
        }

        /// <summary>
        /// Specifies the format to use when copying the value.
        /// </summary>
        /// <param name="format">A function to get the format to use.</param>
        /// <returns>The current module instance.</returns>
        public CopyMetadata WithFormat(Func<string, string> format)
        {
            _formatFunc = format.ThrowIfNull(nameof(format));
            return this;
        }

        protected override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            if (input.TryGetValue(_fromKey, out object existingValue))
            {
                if (_format is object)
                {
                    if (existingValue is DateTime)
                    {
                        existingValue = ((DateTime)existingValue).ToString(_format);
                    }
                    else
                    {
                        existingValue = string.Format(_format, existingValue);
                    }
                }

                if (_formatFunc is object)
                {
                    existingValue = _formatFunc.Invoke(existingValue.ToString());
                }

                return input.Clone(new[] { new KeyValuePair<string, object>(_toKey, existingValue) }).YieldAsync();
            }
            else
            {
                return input.YieldAsync();
            }
        }
    }
}