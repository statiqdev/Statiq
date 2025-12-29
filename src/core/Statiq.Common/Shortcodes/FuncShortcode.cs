using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Statiq.Common
{
    internal class FuncShortcode : IShortcode
    {
        private readonly Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<ShortcodeResult>>> _func;

        public FuncShortcode(Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<IEnumerable<ShortcodeResult>>> func)
        {
            _func = func;
        }

        public FuncShortcode(Func<KeyValuePair<string, string>[], string, IDocument, IExecutionContext, Task<ShortcodeResult>> func)
        {
            _func = async (a, b, c, d) => func is object ? new[] { await func(a, b, c, d) } : null;
        }

        public async Task<IEnumerable<ShortcodeResult>> ExecuteAsync(
            KeyValuePair<string, string>[] args,
            string content,
            IDocument document,
            IExecutionContext context) =>
            await _func?.Invoke(args, content, document, context);
    }
}
