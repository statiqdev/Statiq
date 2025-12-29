using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    public class ScriptHelper : IScriptHelper
    {
        public const string AssemblyName = "ScriptAssembly";
        public const string ScriptClassName = "Script";
        public const string FactoryClassName = ScriptClassName + "Factory";

        // Keyed by script
        private static readonly ConcurrentCache<string, ScriptFactoryBase> _cachedScriptFactories =
            new ConcurrentCache<string, ScriptFactoryBase>(false);

        private readonly IExecutionState _executionState;

        public ScriptHelper(IExecutionState executionState)
        {
            _executionState = executionState.ThrowIfNull(nameof(executionState));
        }

        /// <inheritdoc/>
        public virtual async Task<object> EvaluateAsync(string code, IMetadata metadata)
        {
            ScriptFactoryBase scriptFactory = _cachedScriptFactories.GetOrAdd(
                code,
                (key, t) =>
                {
                    IExecutionContext.Current.LogDebug($"Script cache miss for script `{(key.Length > 20 ? (key.Substring(0, 19) + "...") : key)}`");
                    byte[] rawAssembly = t.Compile(key);
                    Type scriptFactoryType = LoadFactory(rawAssembly);
                    return (ScriptFactoryBase)Activator.CreateInstance(scriptFactoryType);
                },
                this);
            ScriptBase script = scriptFactory.GetScript(metadata, _executionState, IExecutionContext.Current);
            return await script.EvaluateAsync();
        }

        /// <summary>
        /// Loads a script factory from a raw script assembly.
        /// </summary>
        /// <remarks>
        /// This loads the assembly and finds the first script factory type.
        /// </remarks>
        /// <param name="rawAssembly">The raw assembly bytes.</param>
        /// <returns>The script factory or <c>null</c> if a script factory was not found in the assembly.</returns>
        private static Type LoadFactory(byte[] rawAssembly)
        {
            rawAssembly.ThrowIfNull(nameof(rawAssembly));
            Assembly assembly = Assembly.Load(rawAssembly);
            return Array.Find(assembly.GetExportedTypes(), t => t.Name == FactoryClassName);
        }

        /// <summary>
        /// Compiles a script into an in-memory script assembly for later evaluation.
        /// </summary>
        /// <param name="code">The code to compile.</param>
        /// <returns>Raw assembly bytes.</returns>
        protected virtual byte[] Compile(string code)
        {
            code.ThrowIfNull(nameof(code));

            // Parse the code
            code = Parse(code, _executionState);

            // Get the compilation
            CSharpParseOptions parseOptions = new CSharpParseOptions();
            SourceText sourceText = SourceText.From(code, Encoding.UTF8);
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText, parseOptions, AssemblyName);
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).
                WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
                {
                    // ensure that specific warnings about assembly references are always suppressed
                    // https://github.com/dotnet/roslyn/issues/5501
                    { "CS1701", ReportDiagnostic.Suppress },
                    { "CS1702", ReportDiagnostic.Suppress },
                    { "CS1705", ReportDiagnostic.Suppress },

                    // we don't care about unreachable code
                    { "CS0162", ReportDiagnostic.Suppress },
                });

            // Create the compilation
            string assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            IEnumerable<MetadataReference> references = GetScriptReferences().Select(x => MetadataReference.CreateFromFile(x.Location));
            CSharpCompilation compilation = CSharpCompilation.Create(AssemblyName, new[] { syntaxTree }, references, compilationOptions);

            // For some reason, Roslyn really wants these added by filename
            // See http://stackoverflow.com/questions/23907305/roslyn-has-no-reference-to-system-runtime
            compilation = compilation.AddReferences(
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "mscorlib.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Core.dll")),
                MetadataReference.CreateFromFile(Path.Combine(assemblyPath, "System.Runtime.dll")));

            // Emit the assembly
            ILogger logger = _executionState.Services.GetRequiredService<ILogger<ScriptHelper>>();
            using (MemoryStream memoryStream = _executionState.MemoryStreamFactory.GetStream())
            {
                EmitResult result = compilation.Emit(memoryStream);
                LogAndEnsureCompilationSuccess(result, logger);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream.ToArray();
            }
        }

        public virtual IEnumerable<Assembly> GetScriptReferences()
        {
            CompilationReferences references = new CompilationReferences();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                references.TryAddReference(assembly);
            }
            foreach (Assembly assembly in _executionState.ClassCatalog.GetAssemblies())
            {
                references.TryAddReference(assembly);
            }
            references.TryAddReference(Assembly.GetEntryAssembly(), true);
            references.TryAddReference(Assembly.GetCallingAssembly(), true);
            references.TryAddReference(Assembly.GetExecutingAssembly(), true);
            return references;
        }

        public virtual IEnumerable<string> GetScriptNamespaces() => GetScriptNamespaces(_executionState);

        private static IEnumerable<string> GetScriptNamespaces(IExecutionState executionState)
        {
            HashSet<string> namespaces = new HashSet<string>(executionState.Namespaces);
            namespaces.Add("System");
            namespaces.Add("System.Collections");
            namespaces.Add("System.Collections.Generic");
            namespaces.Add("System.Linq");
            namespaces.Add("System.Text");
            namespaces.Add("Statiq.Core");
            return namespaces;
        }

        public static void LogAndEnsureCompilationSuccess(EmitResult result, ILogger logger, string name = null)
        {
            // Log warnings
            List<string> warningMessages = result.Diagnostics
                .Where(x => x.Severity == DiagnosticSeverity.Warning)
                .Select(GetCompilationErrorMessage)
                .ToList();
            if (warningMessages.Count > 0)
            {
                logger.LogWarning(
                    "{0} warnings compiling {1}:{2}{3}",
                    warningMessages.Count,
                    name ?? "script",
                    Environment.NewLine,
                    string.Join(Environment.NewLine, warningMessages));
            }

            // Log errors
            List<string> errorMessages = result.Diagnostics
                .Where(x => x.Severity == DiagnosticSeverity.Error)
                .Select(GetCompilationErrorMessage)
                .ToList();
            if (errorMessages.Count > 0)
            {
                logger.LogError(
                    "{0} errors compiling {1}:{2}{3}",
                    errorMessages.Count,
                    name ?? "script",
                    Environment.NewLine,
                    string.Join(Environment.NewLine, errorMessages));
            }

            // Throw for errors or not success
            if (!result.Success || errorMessages.Count > 0)
            {
                throw new ScriptCompilationException(errorMessages);
            }
        }

        private static string GetCompilationErrorMessage(Diagnostic diagnostic)
        {
            string line = diagnostic.Location.IsInSource ? "Line " + (diagnostic.Location.GetMappedLineSpan().Span.Start.Line + 1) : "Metadata";
            return $"{line}: {diagnostic.Id}: {diagnostic.GetMessage()}";
        }

        // Internal for testing
        // metadataPropertyKeys.Key = property name, metadataPropertyKeys.Value = property type
        internal static string Parse(string code, IExecutionState executionState)
        {
            // Generate a syntax tree from the code
            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions(kind: SourceCodeKind.Script), cancellationToken: executionState.CancellationToken);

            // "Lift" class and method declarations
            LiftingWalker liftingWalker = new LiftingWalker();
            liftingWalker.Visit(syntaxTree.GetRoot(executionState.CancellationToken));

            // Get the using statements
            string usingStatements = string.Join(Environment.NewLine, GetScriptNamespaces(executionState).Select(x => "using " + x + ";"));

            // Determine if we need a return statement
            string preScript = null;
            string postScript = null;
            if (!code.Contains(';'))
            {
                // This is an expression, so add the return before and semicolon after
                preScript = "return";
                postScript = ";";
            }
            else if (!liftingWalker.HasReturnStatement)
            {
                // Includes multiple statements but no return so add one
                postScript = "return null;";
            }

            // Get call signatures for IExecutionState and IMetadata
            string executionStateCalls = string.Join(
                Environment.NewLine,
                ReflectionHelper.GetCallSignatures(typeof(IExecutionState), nameof(ScriptBase.ExecutionState)));
            string metadataCalls = string.Join(
                Environment.NewLine,
                ReflectionHelper.GetCallSignatures(typeof(IMetadata), nameof(ScriptBase.Metadata)));

            // Return the fully parsed script
            return
$@"{usingStatements}
{liftingWalker.UsingDirectives}
public class {ScriptClassName} : ScriptBase, IExecutionState, IMetadata
{{
public {ScriptClassName}(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
    : base(metadata, executionState, executionContext) {{ }}
public override async Task<object> EvaluateAsync()
{{
await Task.CompletedTask;
{preScript}
{liftingWalker.ScriptCode}
{postScript}
}}
{liftingWalker.MethodDeclarations}
{executionStateCalls}
{metadataCalls}
}}
public class {FactoryClassName} : ScriptFactoryBase
{{
public override ScriptBase GetScript(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
{{
return new {ScriptClassName}(metadata, executionState, executionContext);
}}
}}
{liftingWalker.TypeDeclarations}
public static class ScriptExtensionMethods
{{
{liftingWalker.ExtensionMethodDeclarations}
}}";
        }

        // https://stackoverflow.com/a/34268364/807064
        internal static string GetValidIdentifier(string identifier)
        {
            StringBuilder sb = new StringBuilder();
            identifier = identifier.Trim();
            if (!SyntaxFacts.IsIdentifierStartCharacter(identifier[0]))
            {
                sb.Append("_");
            }
            foreach (char ch in identifier)
            {
                if (SyntaxFacts.IsIdentifierPartCharacter(ch))
                {
                    sb.Append(ch);
                }
            }
            string result = sb.ToString();
            if (SyntaxFacts.GetKeywordKind(result) != SyntaxKind.None)
            {
                result = "@" + result;
            }
            return result;
        }
    }
}