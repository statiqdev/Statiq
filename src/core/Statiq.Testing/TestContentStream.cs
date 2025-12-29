using System.IO;
using System.Text;
using Statiq.Common;

namespace Statiq.Testing
{
    public class TestContentStream : DelegatingStream, IContentProviderFactory
    {
        private readonly IExecutionState _executionState;

        public TestContentStream(IExecutionState executionState, string content)
            : base(string.IsNullOrEmpty(content) ? new MemoryStream() : new MemoryStream(Encoding.UTF8.GetBytes(content)))
        {
            _executionState = executionState;
        }

        public IContentProvider GetContentProvider() => GetContentProvider(null);

        public IContentProvider GetContentProvider(string mediaType) => new MemoryContent(((MemoryStream)Stream).ToArray(), mediaType);
    }
}
