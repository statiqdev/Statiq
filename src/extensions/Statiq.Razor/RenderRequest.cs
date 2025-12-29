using System;
using System.Collections.Generic;
using System.IO;
using Statiq.Common;

namespace Statiq.Razor
{
    /// <summary>
    /// All the required parameters to render a Razor view.
    /// </summary>
    internal class RenderRequest
    {
        public Stream Output { get; set; }

        public string RelativePath { get; set; }

        public string ViewStartLocation { get; set; }

        public string LayoutLocation { get; set; }

        public Type BaseType { get; set; }

        public object Model { get; set; }

        public IExecutionContext Context { get; set; }

        public IDocument Document { get; set; }

        public IEnumerable<KeyValuePair<string, object>> ViewData { get; set; }
    }
}