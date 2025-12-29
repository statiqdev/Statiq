using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Sets the media type of each document.
    /// </summary>
    /// <category name="Content" />
    public class SetMediaType : ParallelSyncConfigModule<string>
    {
        public SetMediaType(Config<string> mediaType)
            : base(mediaType, true)
        {
        }

        protected override IEnumerable<IDocument> ExecuteConfig(IDocument input, IExecutionContext context, string value) =>
            input.MediaTypeEquals(value) ? input.Yield() : input.Clone(input.ContentProvider.CloneWithMediaType(value)).Yield();
    }
}