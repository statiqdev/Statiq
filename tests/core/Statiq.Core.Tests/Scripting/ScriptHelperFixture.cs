using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Scripting
{
    [TestFixture]
    public class ScriptHelperFixture : BaseFixture
    {
        public class ParseTests : ScriptHelperFixture
        {
            [Test]
            public void AddsReturnStatement()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code = "int x = 0;";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 1
int x = 0;
return null;
}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
            }

            [Test]
            public void ConvertsExpressionToReturnStatement()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code = "1 + 2";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;
return
#line 1
1 + 2
;
}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
            }

            [Test]
            public void AddsReturnForExpressionWithSemi()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code = "1 + 2;";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 1
1 + 2;
return null;
}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
            }

            [Test]
            public void EmitsReturnStatement()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code = "return 0;";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 1
return 0;
return null;
}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
            }

            [Test]
            public void LiftsClassDeclarations()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code =
@"public class Foo
{
    int X { get; set; }
}
public class Bar : Foo
{
    public string Y()
    {
        return X.ToString();
    }
}
int x = 1 + 2;
Pipelines.Add(Content());
public class Baz
{
}";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 12
int x = 1 + 2;
Pipelines.Add(Content());

return null;
}";
                const string expectedEnd =
@"#line 1
public class Foo
{
    int X { get; set; }
}
public class Bar : Foo
{
    public string Y()
    {
        return X.ToString();
    }
}
#line 14
public class Baz
{
}
public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
                actual.ShouldEndWith(expectedEnd.Replace("\r\n", "\n"));
            }

            [Test]
            public void LiftsUsingDirectives()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code =
@"using Red.Blue;
using Yellow;
public static class Foo
{
    public static string Bar(this string x) => x;
}
Pipelines.Add(Content());
public string Self(string x)
{
    return x.ToLower();
}";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;
using Red.Blue;
using Yellow;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 7
Pipelines.Add(Content());

return null;
}
#line 8
public string Self(string x)
{
    return x.ToLower();
}";

                const string expectedEnd =
@"#line 3
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
                actual.ShouldEndWith(expectedEnd.Replace("\r\n", "\n"));
            }

            [Test]
            public void LiftsMethodDeclarations()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code =
@"public static class Foo
{
    public static string Bar(this string x) => x;
}
Pipelines.Add(Content());
public string Self(string x)
{
    return x.ToLower();
}";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 5
Pipelines.Add(Content());

return null;
}
#line 6
public string Self(string x)
{
    return x.ToLower();
}";

                const string expectedEnd =
@"#line 1
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
                actual.ShouldEndWith(expectedEnd.Replace("\r\n", "\n"));
            }

            [Test]
            public void LiftsExtensionMethodDeclarations()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code =
@"public static class Foo
{
    public static string Bar(this string x) => x;
}
Pipelines.Add(Content());
public static string Self(this string x)
{
    return x.ToLower();
}";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 5
Pipelines.Add(Content());

return null;
}";

                const string expectedEnd =
@"#line 1
public static class Foo
{
    public static string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{
#line 6
public static string Self(this string x)
{
    return x.ToLower();
}
}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
                actual.ShouldEndWith(expectedEnd.Replace("\r\n", "\n"));
            }

            [Test]
            public void LiftsCommentsWithDeclarations()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code =
@"// XYZ
public class Foo
{
    // ABC
    public string Bar(this string x) => x;
}
// 123
Pipelines.Add(Content());
// QWE
public string Self(string x)
{
    // RTY
    return x.ToLower();
}";
                const string expectedStart =
@"using Foo.Bar;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Statiq.Core;

public class Script : ScriptBase, IExecutionState, IMetadata
{
public Script(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) { }
public override async Task<object> EvaluateAsync()
{
await Task.CompletedTask;

#line 7
// 123
Pipelines.Add(Content());

return null;
}
#line 9
// QWE
public string Self(string x)
{
    // RTY
    return x.ToLower();
}";

                const string expectedEnd =
@"#line 1
// XYZ
public class Foo
{
    // ABC
    public string Bar(this string x) => x;
}

public static class ScriptExtensionMethods
{

}";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual = actual.Replace("\r\n", "\n");
                actual.ShouldStartWith(expectedStart.Replace("\r\n", "\n"));
                actual.ShouldEndWith(expectedEnd.Replace("\r\n", "\n"));
            }

            [Test]
            public void ContainsMetadataExtensions()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext()
                {
                    Namespaces = new TestNamespacesCollection(new[] { "Foo.Bar" })
                };
                const string code = "int x = 0;";

                // When
                string actual = ScriptHelper.Parse(code, context);

                // Then
                actual.ShouldContain("public string GetString(string key, string defaultValue = default) => Metadata.GetString(key, defaultValue);");
            }
        }
    }
}
