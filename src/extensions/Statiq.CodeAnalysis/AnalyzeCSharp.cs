using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Statiq.CodeAnalysis.Analysis;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    /// <summary>
    /// Performs static code analysis on the input documents, outputting a new document for each symbol.
    /// </summary>
    /// <remarks>
    /// This module acts as the basis for code analysis scenarios such as generating source code documentation.
    /// All input documents are assumed to contain C# source in their content and are used to create a Roslyn
    /// compilation. All symbols (namespaces, types, members, etc.) in the compilation are then recursively
    /// processed and output from this module as documents, one per symbol. The output documents have empty content
    /// and all information about the symbol is contained in the metadata. This lets you pass the output documents
    /// for each symbol on to a template engine like Razor and generate pages for each symbol by having the
    /// template use the document metadata.
    /// </remarks>
    /// <metadata cref="CodeAnalysisKeys.IsResult" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.SymbolId" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Symbol" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Compilation" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Name" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.FullName" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.QualifiedName" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.DisplayName" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Kind" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.SpecificKind" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ContainingNamespace" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ContainingAssembly" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsStatic" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsAbstract" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsVirtual" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.IsOverride" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.CommentId" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.MemberTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.MemberNamespaces" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ContainingType" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.BaseTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.AllInterfaces" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Members" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Operators" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ExtensionMethods" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.DerivedTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ImplementingTypes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Constructors" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.TypeParameters" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Accessibility" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Attributes" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Parameters" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ReturnType" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Overridden" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Type" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.HasConstantValue" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.ConstantValue" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.DeclaringType" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.AttributeData" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.CommentXml" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Example" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Remarks" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Summary" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Returns" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Value" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Exceptions" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Permissions" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Params" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.TypeParams" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.SeeAlso" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.Syntax" usage="Output"/>
    /// <metadata cref="CodeAnalysisKeys.OutputBuildLog" usage="Setting"/>
    /// <category name="Metadata" />
    public class AnalyzeCSharp : Module
    {
        // Use an intermediate Dictionary to initialize with defaults
        private readonly ConcurrentDictionary<string, string> _cssClasses
            = new ConcurrentDictionary<string, string>(
                new Dictionary<string, string>
                {
                    { "table", "table" }
                });

        private readonly List<Config<IEnumerable<string>>> _assemblyGlobs = new List<Config<IEnumerable<string>>>();
        private readonly List<Config<IEnumerable<string>>> _projectGlobs = new List<Config<IEnumerable<string>>>();
        private readonly List<Config<IEnumerable<string>>> _solutionGlobs = new List<Config<IEnumerable<string>>>();

        private Func<ISymbol, Compilation, bool> _symbolPredicate;
        private Func<ISymbol, Compilation, NormalizedPath> _destination;
        private NormalizedPath _destinationPrefix = null;
        private Config<bool> _docsForImplicitSymbols = false;
        private Config<bool> _inputDocuments = true;
        private bool _assemblySymbols = false;
        private Config<bool> _implicitInheritDoc = false;
        private Config<string> _compilationAssemblyName = Config.FromContext(_ => Path.GetRandomFileName());
        private Config<bool> _includeEmptyNamespaces = false;

        /// <summary>
        /// This will assume <c>inheritdoc</c> if a symbol has no other code comments.
        /// </summary>
        /// <param name="implicitInheritDoc">If set to <c>true</c> or <c>null</c>, the symbol will inherit documentation comments
        /// if no other comments are provided.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithImplicitInheritDoc(Config<bool> implicitInheritDoc = null)
        {
            _implicitInheritDoc = (implicitInheritDoc ?? true).EnsureNonDocument(nameof(implicitInheritDoc));
            return this;
        }

        /// <summary>
        /// By default, XML documentation comments are not parsed and rendered for documents that are not part
        /// of the initial result set. This can control that behavior and be used to generate documentation
        /// metadata for all documents, regardless if they were part of the initial result set.
        /// </summary>
        /// <param name="docsForImplicitSymbols">If set to <c>true</c> or <c>null</c>, documentation metadata is generated for XML comments on all symbols.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDocsForImplicitSymbols(Config<bool> docsForImplicitSymbols = null)
        {
            _docsForImplicitSymbols = (docsForImplicitSymbols ?? true).EnsureNonDocument(nameof(docsForImplicitSymbols));
            return this;
        }

        /// <summary>
        /// Controls whether the content of input documents is treated as code and used in the analysis (the default is <c>true</c>).
        /// </summary>
        /// <param name="inputDocuments"><c>true</c> or <c>null</c> to analyze the content of input documents.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithInputDocuments(Config<bool> inputDocuments = null)
        {
            _inputDocuments = (inputDocuments ?? true).EnsureNonDocument(nameof(inputDocuments));
            return this;
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">A globbing pattern indicating the assemblies to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblies(Config<string> assemblies)
        {
            _assemblyGlobs.Add(assemblies.EnsureNonDocument(nameof(assemblies)).MakeEnumerable());
            return this;
        }

        /// <summary>
        /// Analyzes the specified assemblies.
        /// </summary>
        /// <param name="assemblies">Globbing patterns indicating the assemblies to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblies(Config<IEnumerable<string>> assemblies)
        {
            _assemblyGlobs.Add(assemblies.EnsureNonDocument(nameof(assemblies)));
            return this;
        }

        /// <summary>
        /// Analyzes the specified projects.
        /// </summary>
        /// <param name="projects">A globbing pattern indicating the projects to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithProjects(Config<string> projects)
        {
            _projectGlobs.Add(projects.EnsureNonDocument(nameof(projects)).MakeEnumerable());
            return this;
        }

        /// <summary>
        /// Analyzes the specified projects.
        /// </summary>
        /// <param name="projects">Globbing patterns indicating the projects to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithProjects(Config<IEnumerable<string>> projects)
        {
            _projectGlobs.Add(projects.EnsureNonDocument(nameof(projects)));
            return this;
        }

        /// <summary>
        /// Analyzes the specified solutions.
        /// </summary>
        /// <param name="solutions">A globbing pattern indicating the solutions to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithSolutions(Config<string> solutions)
        {
            _solutionGlobs.Add(solutions.EnsureNonDocument(nameof(solutions)).MakeEnumerable());
            return this;
        }

        /// <summary>
        /// Analyzes the specified solutions.
        /// </summary>
        /// <param name="solutions">Globbing patterns indicating the solutions to analyze.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithSolutions(Config<IEnumerable<string>> solutions)
        {
            _solutionGlobs.Add(solutions.EnsureNonDocument(nameof(solutions)));
            return this;
        }

        /// <summary>
        /// Controls which symbols are processed as part of the initial result set.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereSymbol(Func<ISymbol, bool> predicate) =>
            WhereSymbol(predicate is null ? (Func<ISymbol, Compilation, bool>)null : (s, _) => predicate(s));

        /// <summary>
        /// Controls which symbols are processed as part of the initial result set.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereSymbol(Func<ISymbol, Compilation, bool> predicate)
        {
            if (predicate is object)
            {
                Func<ISymbol, Compilation, bool> currentPredicate = _symbolPredicate;
                _symbolPredicate = currentPredicate is null ? predicate : (s, c) => currentPredicate(s, c) && predicate(s, c);
            }
            return this;
        }

        /// <summary>
        /// Restricts the initial result set to named type symbols (I.e., classes, interfaces, etc.). Also allows supplying
        /// an additional predicate on the named type.
        /// </summary>
        /// <param name="predicate">A predicate that returns <c>true</c> if the symbol should be included in the initial result set.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithNamedTypes(Func<INamedTypeSymbol, bool> predicate = null) =>
            WhereSymbol(x => x is INamedTypeSymbol namedTypeSymbol && (predicate?.Invoke(namedTypeSymbol) ?? true));

        /// <summary>
        /// Limits symbols in the initial result set to those in the specified namespaces.
        /// </summary>
        /// <param name="includeGlobal">If set to <c>true</c>, symbols in the unnamed global namespace are included.</param>
        /// <param name="namespaces">The namespaces to include symbols from (if <c>namespaces</c> is <c>null</c>, symbols from all
        /// namespaces are included).</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereNamespaces(bool includeGlobal, params string[] namespaces)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
                if (!(x is INamespaceSymbol namespaceSymbol))
                {
                    return x.ContainingNamespace is object
                           && (namespaces.Length == 0 || namespaces.Any(y => x.ContainingNamespace.ToString().StartsWith(y)));
                }
                if (namespaces.Length == 0)
                {
                    return includeGlobal || !namespaceSymbol.IsGlobalNamespace;
                }
                return (includeGlobal && ((INamespaceSymbol)x).IsGlobalNamespace)
                       || namespaces.Any(y => x.ToString().StartsWith(y));
            });
        }

        /// <summary>
        /// Limits symbols in the initial result set to those in the namespaces that satisfy the specified predicate.
        /// </summary>
        /// <param name="predicate">A predicate that returns true if symbols in the namespace should be included.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WhereNamespaces(Func<string, bool> predicate)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
                if (!(x is INamespaceSymbol namespaceSymbol))
                {
                    return x.ContainingNamespace is object && predicate(x.ContainingNamespace.ToString());
                }
                return predicate(namespaceSymbol.ToString());
            });
        }

        /// <summary>
        /// Limits symbols in the initial result set to those that are public (and optionally protected).
        /// </summary>
        /// <param name="includeProtected">If set to <c>true</c>, protected symbols are also included.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WherePublic(bool includeProtected = true)
        {
            return WhereSymbol(x =>
            {
                if (x is IAssemblySymbol)
                {
                    return true;
                }
                return x.DeclaredAccessibility == Accessibility.Public
                    || (includeProtected && x.DeclaredAccessibility == Accessibility.Protected)
                    || x.DeclaredAccessibility == Accessibility.NotApplicable;
            });
        }

        /// <summary>
        /// While converting XML documentation to HTML, any tags with the specified name will get the specified CSS class(s).
        /// This is helpful to style your XML documentation comment rendering to support the stylesheet of your site.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="cssClasses">The CSS classes to set for the specified tag name. Separate multiple CSS classes
        /// with a space (just like you would in HTML).</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithCssClasses(string tagName, string cssClasses)
        {
            tagName.ThrowIfNull(nameof(tagName));
            if (string.IsNullOrWhiteSpace(cssClasses))
            {
                _cssClasses.TryRemove(tagName, out cssClasses);
            }
            else
            {
                _cssClasses[tagName] = cssClasses;
            }
            return this;
        }

        /// <summary>
        /// Controls whether assembly symbol documents are output.
        /// </summary>
        /// <param name="assemblySymbols"><c>true</c> to output assembly symbol documents.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithAssemblySymbols(bool assemblySymbols = true)
        {
            _assemblySymbols = assemblySymbols;
            return WhereSymbol((s, c) => !(s is IAssemblySymbol) || (_assemblySymbols && s.Name != c.AssemblyName));
        }

        /// <summary>
        /// This changes the default behavior for the generated destination paths, which is to place files in a path
        /// with the same name as their containing namespace. Namespace documents will have the <see cref="Keys.IndexFileName"/> while other type documents
        /// will get a name equal to their SymbolId. Member documents will get the same name as their containing type plus an
        /// anchor to their SymbolId.
        /// </summary>
        /// <param name="destination">
        /// A function that takes the metadata for a given symbol and returns a <see cref="NormalizedPath"/> to use for the destination.
        /// </param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDestination(Func<ISymbol, NormalizedPath> destination) =>
            WithDestination(destination is null ? (Func<ISymbol, Compilation, NormalizedPath>)null : (s, _) => destination(s));

        /// <summary>
        /// This changes the default behavior for the generated destination paths, which is to place files in a path
        /// with the same name as their containing namespace. Namespace documents will have the <see cref="Keys.IndexFileName"/> while other type documents
        /// will get a name equal to their SymbolId. Member documents will get the same name as their containing type plus an
        /// anchor to their SymbolId.
        /// </summary>
        /// <param name="destination">
        /// A function that takes the metadata for a given symbol and returns a <see cref="NormalizedPath"/> to use for the destination.
        /// </param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDestination(Func<ISymbol, Compilation, NormalizedPath> destination)
        {
            _destination = destination;
            return this;
        }

        /// <summary>
        /// Allows you to change the name of the assembly created by the compilation this module produces. This can be
        /// useful for uniquely identifying multiple instances of the module, for example. By default
        /// <see cref="Path.GetRandomFileName"/> is used to derive the compilation assembly name.
        /// </summary>
        /// <param name="compilationAssemblyName">The name of the compilation assembly.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithCompilationAssemblyName(Config<string> compilationAssemblyName)
        {
            _compilationAssemblyName = compilationAssemblyName.EnsureNonDocument(nameof(compilationAssemblyName));
            return this;
        }

        /// <summary>
        /// This lets you add a prefix to the default destination behavior (such as nesting symbol documents inside
        /// a folder like "api/"). Whatever you supply will be combined with the destination. This method has no
        /// effect if you've supplied a custom destination behavior.
        /// </summary>
        /// <param name="destinationPreview">The prefix to use for each generated destination.</param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp WithDestinationPrefix(in NormalizedPath destinationPreview)
        {
            _destinationPrefix = destinationPreview;
            return this;
        }

        /// <summary>
        /// Controls whether symbol documents for empty namespaces (those that contain
        /// only other namespaces but no other symbols recursively) are output. By default
        /// only namespaces that contain other nested symbols are output.
        /// </summary>
        /// <param name="includeEmptyNamespaces">
        /// <c>true</c> to output empty namespaces, <c>false</c> to omit them.
        /// </param>
        /// <returns>The current module instance.</returns>
        public AnalyzeCSharp IncludeEmptyNamespaces(Config<bool> includeEmptyNamespaces = null)
        {
            _includeEmptyNamespaces = (includeEmptyNamespaces ?? true).EnsureNonDocument(nameof(includeEmptyNamespaces));
            return this;
        }

        private static NormalizedPath GetDefaultDestination(ISymbol symbol, in NormalizedPath prefix, IExecutionContext context)
        {
            INamespaceSymbol containingNamespace = symbol.ContainingNamespace;
            NormalizedPath destinationPath;
            string indexFileName = context.Settings.GetIndexFileName();
            string pageFileExtension = context.Settings.GetPageFileExtensions()[0]; // Includes preceding "."

            if (symbol.Kind == SymbolKind.Assembly)
            {
                // Assemblies output to the index page in a folder of their name
                destinationPath = new NormalizedPath($"{symbol.GetDisplayName()}/{indexFileName}");
            }
            else if (symbol.Kind == SymbolKind.Namespace)
            {
                // Namespaces output to the index page in a folder of their full name
                // If this namespace does not have a containing namespace, it's the global namespace
                destinationPath = new NormalizedPath(containingNamespace is null ? $"global/{indexFileName}" : $"{symbol.GetDisplayName()}/{indexFileName}");
            }
            else if (symbol.Kind == SymbolKind.NamedType)
            {
                // Types output to the index page in a folder of their SymbolId under the folder for their namespace
                destinationPath = new NormalizedPath(containingNamespace.ContainingNamespace is null
                    ? $"global/{symbol.GetId()}/{indexFileName}"
                    : $"{containingNamespace.GetDisplayName()}/{symbol.GetId()}/{indexFileName}");
            }
            else
            {
                // Members output to a page equal to their SymbolId under the folder for their type
                NormalizedPath containingPath = GetDefaultDestination(symbol.ContainingType, null, context);
                destinationPath = containingPath.ChangeFileName($"{symbol.GetId()}{pageFileExtension}");
            }

            // Add the prefix
            if (!prefix.IsNull)
            {
                destinationPath = prefix.Combine(destinationPath);
            }

            return destinationPath;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Create the compilation (have to supply an XmlReferenceResolver to handle include XML doc comments)
            MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            CSharpCompilation compilation = CSharpCompilation
                .Create(await _compilationAssemblyName.GetValueAsync(null, context))
                .WithReferences(mscorlib)
                .WithOptions(new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    xmlReferenceResolver: new XmlFileResolver(
                        context.FileSystem.RootPath.FullPath == NormalizedPath.Slash ? null : context.FileSystem.RootPath.FullPath)));

            // Add the input source and references
            List<ISymbol> symbols = new List<ISymbol>();
            compilation = await AddSourceFilesAsync(context, compilation);
            compilation = await AddProjectReferencesAsync(context, symbols, compilation);
            compilation = await AddSolutionReferencesAsync(context, symbols, compilation);
            compilation = await AddAssemblyReferencesAsync(context, symbols, compilation);

            // Get and return the document tree
            symbols.Add(compilation.Assembly.GlobalNamespace);
            AnalyzeSymbolVisitor visitor = new AnalyzeSymbolVisitor(
                compilation,
                context,
                _symbolPredicate,
                _destination ?? ((s, _) => GetDefaultDestination(s, _destinationPrefix, context)),
                _cssClasses,
                await _docsForImplicitSymbols.GetValueAsync(null, context),
                _assemblySymbols,
                await _implicitInheritDoc.GetValueAsync(null, context),
                await _includeEmptyNamespaces.GetValueAsync(null, context));
            Parallel.ForEach(symbols, s => visitor.Visit(s));
            return visitor.Finish();
        }

        private async Task<CSharpCompilation> AddSourceFilesAsync(IExecutionContext context, CSharpCompilation compilation)
        {
            ConcurrentBag<SyntaxTree> syntaxTrees = new ConcurrentBag<SyntaxTree>();
            if (await _inputDocuments.GetValueAsync(null, context))
            {
                // Get syntax trees (supply path so that XML doc includes can be resolved)
                Parallel.ForEach(context.Inputs, AddSyntaxTrees);
                compilation = compilation.AddSyntaxTrees(syntaxTrees);
            }
            return compilation;

            void AddSyntaxTrees(IDocument input)
            {
                using (Stream stream = input.GetContentStream())
                {
                    SourceText sourceText = SourceText.From(stream);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(
                        sourceText,
                        path: input.Source.IsNull ? string.Empty : input.Source.FullPath));
                }
            }
        }

        private static async Task<List<string>> GetGlobValuesAsync(List<Config<IEnumerable<string>>> globConfigs, IExecutionContext context)
        {
            List<string> result = new List<string>();
            foreach (Config<IEnumerable<string>> config in globConfigs)
            {
                IEnumerable<string> globs = await config.GetValueAsync(null, context);
                if (globs is object)
                {
                    result.AddRange(globs.Where(x => !string.IsNullOrWhiteSpace(x)));
                }
            }
            return result;
        }

        private async Task<CSharpCompilation> AddAssemblyReferencesAsync(
            IExecutionContext context, List<ISymbol> symbols, CSharpCompilation compilation)
        {
            List<string> assemblyGlobs = await GetGlobValuesAsync(_assemblyGlobs, context);
            IEnumerable<IFile> assemblyFiles = context.FileSystem.GetInputFiles(assemblyGlobs);
            assemblyFiles = assemblyFiles.Where(x => (x.Path.Extension == ".dll" || x.Path.Extension == ".exe") && x.Exists);
            MetadataReference[] assemblyReferences = assemblyFiles.Select(CreateMetadataReferences).ToArray();
            if (assemblyReferences.Length > 0)
            {
                compilation = compilation.AddReferences(assemblyReferences);
                symbols.AddRange(assemblyReferences
                    .Select(x => (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(x))
                    .Select(x => _assemblySymbols ? x : (ISymbol)x.GlobalNamespace));
            }
            return compilation;

            MetadataReference CreateMetadataReferences(IFile assemblyFile)
            {
                // Create the metadata reference for the compilation
                IFile xmlFile = context.FileSystem.GetFile(assemblyFile.Path.ChangeExtension("xml"));
                if (xmlFile.Exists)
                {
                    context.LogDebug($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} with XML documentation file at {xmlFile.Path.FullPath}");
                    return MetadataReference.CreateFromFile(
                        assemblyFile.Path.FullPath,
                        documentation: XmlDocumentationProvider.CreateFromFile(xmlFile.Path.FullPath));
                }
                context.LogDebug($"Creating metadata reference for assembly {assemblyFile.Path.FullPath} without XML documentation file");
                return (MetadataReference)MetadataReference.CreateFromFile(assemblyFile.Path.FullPath);
            }
        }

        private async Task<CSharpCompilation> AddProjectReferencesAsync(
            IExecutionContext context, List<ISymbol> symbols, CSharpCompilation compilation)
        {
            // Generate a single Workspace and add all of the projects to it
            StringWriter log = new StringWriter();
            AnalyzerManager manager = new AnalyzerManager(new AnalyzerManagerOptions
            {
                LogWriter = log
            });
            AdhocWorkspace workspace = new AdhocWorkspace();
            List<string> projectGlobs = await GetGlobValuesAsync(_projectGlobs, context);
            IEnumerable<IFile> projectFiles = context.FileSystem.GetInputFiles(projectGlobs);
            projectFiles = projectFiles.Where(x => x.Path.Extension == ".csproj" && x.Exists);
            List<Project> projects = new List<Project>();
            foreach (IFile projectFile in projectFiles)
            {
                Project project = workspace.CurrentSolution.Projects.FirstOrDefault(x => new NormalizedPath(x.FilePath).Equals(projectFile.Path));
                if (project is object)
                {
                    context.LogDebug($"Project {projectFile.Path.FullPath} was already in the workspace");
                }
                else
                {
                    context.LogDebug($"Creating workspace project for {projectFile.Path.FullPath}");
                    IProjectAnalyzer analyzer = manager.GetProject(projectFile.Path.FullPath);
                    if (context.Settings.GetBool(CodeAnalysisKeys.OutputBuildLog))
                    {
                        analyzer.AddBinaryLogger();
                    }
                    IAnalyzerResult result = ReadWorkspace.CompileProject(context, analyzer, log);
                    if (result is object)
                    {
                        project = result.AddToWorkspace(workspace);
                        if (!project.Documents.Any())
                        {
                            context.LogWarning($"Project at {projectFile.Path.FullPath} contains no documents, which may be an error (check previous log output for any MSBuild warnings)");
                        }
                    }
                }
                projects.Add(project);
            }
            return await AddProjectReferencesAsync(context, projects, symbols, compilation);
        }

        private async Task<CSharpCompilation> AddSolutionReferencesAsync(
            IExecutionContext context, List<ISymbol> symbols, CSharpCompilation compilation)
        {
            List<string> solutionGlobs = await GetGlobValuesAsync(_solutionGlobs, context);
            IEnumerable<IFile> solutionFiles = context.FileSystem.GetInputFiles(solutionGlobs);
            solutionFiles = solutionFiles.Where(x => x.Path.Extension == ".sln" && x.Exists);
            foreach (IFile solutionFile in solutionFiles)
            {
                context.LogDebug($"Creating workspace solution for {solutionFile.Path.FullPath}");
                StringWriter log = new StringWriter();
                AnalyzerManager manager = new AnalyzerManager(
                    solutionFile.Path.FullPath,
                    new AnalyzerManagerOptions
                    {
                        LogWriter = log
                    });

                IAnalyzerResult[] results = manager.Projects.Values
                    .Select(analyzer =>
                    {
                        if (context.Settings.GetBool(CodeAnalysisKeys.OutputBuildLog))
                        {
                            analyzer.AddBinaryLogger();
                        }
                        return ReadWorkspace.CompileProject(context, analyzer, log);
                    })
                    .Where(x => x is object)
                    .ToArray();

                AdhocWorkspace workspace = new AdhocWorkspace();
                foreach (IAnalyzerResult result in results)
                {
                    result.AddToWorkspace(workspace);
                }

                compilation = await AddProjectReferencesAsync(context, workspace.CurrentSolution.Projects, symbols, compilation);
            }
            return compilation;
        }

        private async Task<CSharpCompilation> AddProjectReferencesAsync(
            IExecutionContext context, IEnumerable<Project> projects, List<ISymbol> symbols, CSharpCompilation compilation)
        {
            // Add a references to the compilation for each project in the solution
            MetadataReference[] compilationReferences = await projects
                .ToAsyncEnumerable()
                .Where(x => x.SupportsCompilation)
                .SelectAwait(async x =>
                {
                    context.LogDebug($"Creating compilation reference for project {x.Name}");
                    Compilation projectCompilation = await x.GetCompilationAsync();
                    return projectCompilation.ToMetadataReference(new[] { x.AssemblyName }.ToImmutableArray());
                })
                .ToArrayAsync();
            if (compilationReferences.Length > 0)
            {
                compilation = compilation.AddReferences(compilationReferences);
                symbols.AddRange(compilationReferences
                    .Select(x => (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(x))
                    .Select(x => _assemblySymbols ? x : (ISymbol)x.GlobalNamespace));
            }
            return compilation;
        }
    }
}