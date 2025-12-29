using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Statiq.Common
{
    /// <summary>
    /// Contains the output documents from execution.
    /// </summary>
    public class ExecutionOutputs
    {
        internal ExecutionOutputs(IEnumerable<IDocument> outputs)
        {
            Outputs = outputs;
        }

        /// <summary>
        /// The output documents from execution. Set this property
        /// to modify the output documents.
        /// </summary>
        public IEnumerable<IDocument> Outputs { get; set; }
    }
}
