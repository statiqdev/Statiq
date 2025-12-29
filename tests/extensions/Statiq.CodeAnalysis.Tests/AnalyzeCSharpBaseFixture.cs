using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.CodeAnalysis.Tests
{
    public abstract class AnalyzeCSharpBaseFixture : BaseFixture
    {
        protected TestDocument GetDocument(string content) => new TestDocument(content);

        protected TestExecutionContext GetContext() => new TestExecutionContext
        {
            FileSystem = new TestFileSystem
            {
                RootPath = new NormalizedPath(TestContext.CurrentContext.TestDirectory)
            }
        };

        protected IDocument GetResult(IReadOnlyList<IDocument> results, string name)
        {
            return results.Single(x => x["Name"].Equals(name));
        }

        protected IDocument GetMember(IReadOnlyList<IDocument> results, string className, string memberName)
        {
            return GetResult(results, className)
                .Get<IEnumerable<IDocument>>("Members")
                .Single(x => x["Name"].Equals(memberName));
        }

        protected IDocument GetParameter(IReadOnlyList<IDocument> results, string className, string methodName, string parameterName)
        {
            return GetResult(results, className)
                .Get<IEnumerable<IDocument>>("Members")
                .Single(x => x["Name"].Equals(methodName))
                .Get<IEnumerable<IDocument>>("Parameters")
                .Cast<TestDocument>()
                .Single(x => x["Name"].Equals(parameterName));
        }
    }
}
