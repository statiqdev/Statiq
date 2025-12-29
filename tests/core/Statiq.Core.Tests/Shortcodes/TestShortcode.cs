using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core.Tests.Shortcodes
{
    public class TestShortcode : IShortcode
    {
        public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
