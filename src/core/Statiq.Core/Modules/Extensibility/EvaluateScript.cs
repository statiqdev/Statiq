using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Evaluates a C# based script contained in document content.
    /// </summary>
    /// <category name="Extensibility" />
    public class EvaluateScript : ParallelModule
    {
        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            object value = await context.ScriptHelper.EvaluateAsync(await input.GetContentStringAsync(), input);
            return await context.CloneOrCreateDocumentsAsync(input, input.Yield(), value ?? new NullContent());
        }
    }
}