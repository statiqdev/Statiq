using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentCollections;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.CodeAnalysis.Analysis
{
    // If types aren't matching (I.e., not linking due to mismatched documents), may need to use ISymbol.OriginalDefinition when
    // creating the document for a symbol (or document metadata) to counteract new symbols due to type substitution for generics
    internal class AnalyzeSymbolVisitor : SymbolVisitor
    {
        private static readonly object XmlDocLock = new object();

        private static readonly SymbolDisplayFormat _documentSourceFormat = new SymbolDisplayFormat(
            memberOptions: SymbolDisplayMemberOptions.IncludeContainingType,
            parameterOptions: SymbolDisplayParameterOptions.None,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.None);

        private readonly ConcurrentCache<string, IDocument> _namespaceDisplayNameToDocument =
            new ConcurrentCache<string, IDocument>(false);
        private readonly ConcurrentCache<string, ConcurrentHashSet<INamespaceSymbol>> _namespaceDisplayNameToSymbols =
            new ConcurrentCache<string, ConcurrentHashSet<INamespaceSymbol>>(false);
        private readonly ConcurrentCache<ISymbol, IDocument> _symbolToDocument =
            new ConcurrentCache<ISymbol, IDocument>(false);
        private readonly ConcurrentHashSet<IMethodSymbol> _extensionMethods =
            new ConcurrentHashSet<IMethodSymbol>();

        private readonly CSharpCompilation _compilation;
        private readonly IExecutionContext _context;
        private readonly Func<ISymbol, Compilation, bool> _symbolPredicate;
        private readonly Func<ISymbol, Compilation, NormalizedPath> _destination;
        private readonly ConcurrentDictionary<string, string> _cssClasses;
        private readonly bool _docsForImplicitSymbols;
        private readonly bool _assemblySymbols;
        private readonly bool _implicitInheritDoc;
        private readonly bool _includeEmptyNamespaces;
        private readonly MethodInfo _getAccessibleMembersInThisAndBaseTypes;
        private readonly Type _documentationCommentCompiler;
        private readonly MethodInfo _documentationCommentCompilerDefaultVisit;
        private readonly MethodInfo _diagnosticBagGetInstance;
        private readonly MethodInfo _diagnosticBagFree;
        private readonly Type _publicModelSymbol;
        private readonly PropertyInfo _publicModelSymbolUnderlyingType;

        private ImmutableArray<KeyValuePair<INamedTypeSymbol, IDocument>> _namedTypes;  // This contains all of the NamedType symbols and documents obtained during the initial processing
        private bool _finished; // When this is true, we're visiting external symbols and should omit certain metadata and don't descend

        public AnalyzeSymbolVisitor(
            CSharpCompilation compilation,
            IExecutionContext context,
            Func<ISymbol, Compilation, bool> symbolPredicate,
            Func<ISymbol, Compilation, NormalizedPath> destination,
            ConcurrentDictionary<string, string> cssClasses,
            bool docsForImplicitSymbols,
            bool assemblySymbols,
            bool implicitInheritDoc,
            bool includeEmptyNamespaces)
        {
            _compilation = compilation;
            _context = context;
            _symbolPredicate = symbolPredicate;
            _destination = destination;
            _cssClasses = cssClasses;
            _docsForImplicitSymbols = docsForImplicitSymbols;
            _assemblySymbols = assemblySymbols;
            _implicitInheritDoc = implicitInheritDoc;
            _includeEmptyNamespaces = includeEmptyNamespaces;

            // Get any reflected methods we need
            Assembly workspacesAssembly = typeof(Workspace).Assembly;
            Assembly csharpAssembly = typeof(CSharpCompilation).Assembly;

            MethodInfo reflectedMethod = FindAccessibleMembersMethod(workspacesAssembly, csharpAssembly);
            if (reflectedMethod == null)
            {
                throw new InvalidOperationException(
                    "Could not locate a suitable 'GetAccessibleMembersInThisAndBaseTypes' method via reflection. Roslyn internals may have changed.");
            }

            _getAccessibleMembersInThisAndBaseTypes = reflectedMethod.MakeGenericMethod(typeof(ISymbol));
            _documentationCommentCompiler = csharpAssembly.GetType("Microsoft.CodeAnalysis.CSharp.DocumentationCommentCompiler");
            _documentationCommentCompilerDefaultVisit = _documentationCommentCompiler.GetMethod("DefaultVisit");

            _publicModelSymbol = csharpAssembly.GetType("Microsoft.CodeAnalysis.CSharp.Symbols.PublicModel.Symbol");
            _publicModelSymbolUnderlyingType = _publicModelSymbol.GetProperty("UnderlyingSymbol", BindingFlags.Instance | BindingFlags.NonPublic);

            Type reflectedType = csharpAssembly.GetType("Microsoft.CodeAnalysis.CSharp.BindingDiagnosticBag");
            _diagnosticBagGetInstance = reflectedType.GetMethod("GetInstance", 0, BindingFlags.Static | BindingFlags.NonPublic, null, Array.Empty<Type>(), null);
            _diagnosticBagFree = reflectedType.GetMethod("Free", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        public IEnumerable<IDocument> Finish()
        {
            _finished = true;
            _namedTypes = _symbolToDocument
                .Where(x => x.Key.Kind == SymbolKind.NamedType)
                .Select(x => new KeyValuePair<INamedTypeSymbol, IDocument>((INamedTypeSymbol)x.Key, x.Value))
                .ToImmutableArray();
            return _symbolToDocument.Select(x => x.Value);
        }

        public override void DefaultVisit(ISymbol symbol)
        {
            if (ShouldIncludeSymbol(symbol))
            {
                AddDocumentCommon(symbol, false, new MetadataItems());
            }

            base.DefaultVisit(symbol);
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            if (ShouldIncludeSymbol(symbol))
            {
                AddDocumentCommon(symbol, true, new MetadataItems
                {
                    { CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString() },
                    { CodeAnalysisKeys.MemberNamespaces, DocumentsFor(symbol.GlobalNamespace.GetNamespaceMembers()) }
                });
            }

            // Descend if not finished, regardless if this namespace was included
            if (!_finished)
            {
                symbol.GlobalNamespace.Accept(this);
            }
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            // Add to the namespace symbol cache
            string displayName = symbol.GetDisplayName();
            ConcurrentHashSet<INamespaceSymbol> symbols = _namespaceDisplayNameToSymbols.GetOrAdd(displayName, _ => new ConcurrentHashSet<INamespaceSymbol>());
            symbols.Add(symbol);

            // Exclude this namespace if it doesn't contain any included nested symbols
            if (ShouldIncludeSymbol(symbol) && (HasIncludedMembers(symbol) || _includeEmptyNamespaces))
            {
                // Create the document
                _namespaceDisplayNameToDocument.AddOrUpdate(
                    displayName,
                    _ => AddNamespaceDocument(symbol, true),
                    (_, existing) =>
                    {
                        // There's already a document for this symbol display name, add it to the symbol-to-document cache
                        _symbolToDocument.TryAdd(symbol, () => existing);
                        return existing;
                    });
            }

            bool HasIncludedMembers(INamespaceSymbol nestedNamespace)
            {
                if (nestedNamespace.GetTypeMembers().Any(ShouldIncludeSymbol))
                {
                    return true;
                }

                foreach (INamespaceSymbol childNestedNamespace in nestedNamespace.GetNamespaceMembers())
                {
                    if (HasIncludedMembers(childNestedNamespace))
                    {
                        return true;
                    }
                }

                return false;
            }

            // Descend if not finished, regardless if this namespace was included
            if (!_finished)
            {
                Parallel.ForEach(symbol.GetMembers(), s => s.Accept(this));
            }
        }

        public override void VisitNamedType(INamedTypeSymbol symbol)
        {
            // Only visit the original definition until we're finished
            INamedTypeSymbol originalDefinition = GetOriginalSymbolDefinition(symbol);
            if (!_finished && !SymbolEqualityComparer.Default.Equals(originalDefinition, symbol))
            {
                VisitNamedType(originalDefinition);
                return;
            }

            if (ShouldIncludeSymbol(symbol))
            {
                MetadataItems metadata = new MetadataItems
                {
                    { CodeAnalysisKeys.SpecificKind, _ => symbol.TypeKind.ToString() },
                    { CodeAnalysisKeys.ContainingType, DocumentFor(symbol.ContainingType) },
                    { CodeAnalysisKeys.MemberTypes, DocumentsFor(symbol.GetTypeMembers()) },
                    { CodeAnalysisKeys.BaseTypes, DocumentsFor(GetBaseTypes(symbol)) },
                    { CodeAnalysisKeys.AllInterfaces, DocumentsFor(symbol.AllInterfaces) },
                    { CodeAnalysisKeys.Members, DocumentsFor(GetAccessibleMembersInThisAndBaseTypes(symbol, symbol).Where(MemberPredicate)) },
                    { CodeAnalysisKeys.Operators, DocumentsFor(GetAccessibleMembersInThisAndBaseTypes(symbol, symbol).Where(OperatorPredicate)) },
                    { CodeAnalysisKeys.ExtensionMethods, _ => DocumentsFor(_extensionMethods.Where(x => x.ReduceExtensionMethod(symbol) is object)) },
                    { CodeAnalysisKeys.Constructors, DocumentsFor(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared)) },
                    { CodeAnalysisKeys.TypeParameters, DocumentsFor(symbol.TypeParameters) },
                    { CodeAnalysisKeys.TypeArguments, DocumentsFor(symbol.TypeArguments) },
                    { CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString() },
                    { CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol) },
                    { CodeAnalysisKeys.IsTupleType, _ => symbol.IsTupleType },
                    { CodeAnalysisKeys.TupleElements, DocumentsFor(symbol.TupleElements) }
                };
                if (!_finished)
                {
                    metadata.AddRange(new[]
                    {
                        new MetadataItem(CodeAnalysisKeys.DerivedTypes, _ => GetDerivedTypes(symbol), true),
                        new MetadataItem(CodeAnalysisKeys.ImplementingTypes, _ => GetImplementingTypes(symbol), true)
                    });
                }
                AddDocumentCommon(symbol, true, metadata);

                // Descend if not finished, and only if this type was included
                if (!_finished)
                {
                    Parallel.ForEach(
                        symbol.GetMembers()
                        .Where(MemberPredicate)
                        .Concat(symbol.Constructors.Where(x => !x.IsImplicitlyDeclared)),
                        s => s.Accept(this));
                }
            }
        }

        public override void VisitTypeParameter(ITypeParameterSymbol symbol)
        {
            if (ShouldIncludeSymbol(symbol))
            {
                AddMemberDocument(symbol, false, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.TypeParameterKind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.DeclaringType, DocumentFor(symbol.DeclaringType)),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        public override void VisitParameter(IParameterSymbol symbol)
        {
            if (ShouldIncludeSymbol(symbol))
            {
                AddMemberDocument(symbol, false, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        public override void VisitMethod(IMethodSymbol symbol)
        {
            // If this is an extension method, record it
            if (!_finished && symbol.IsExtensionMethod)
            {
                _extensionMethods.Add(symbol);
            }

            if (ShouldIncludeSymbol(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.MethodKind == MethodKind.Ordinary ? "Method" : symbol.MethodKind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.TypeParameters, DocumentsFor(symbol.TypeParameters)),
                    new MetadataItem(CodeAnalysisKeys.TypeArguments, DocumentsFor(symbol.TypeArguments)),
                    new MetadataItem(CodeAnalysisKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    new MetadataItem(CodeAnalysisKeys.ReturnType, DocumentFor(symbol.ReturnType)),
                    new MetadataItem(CodeAnalysisKeys.Overridden, DocumentFor(symbol.OverriddenMethod)),
                    new MetadataItem(CodeAnalysisKeys.Implements, DocumentsFor(GetImplements(symbol))),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        public override void VisitField(IFieldSymbol symbol)
        {
            if (ShouldIncludeSymbol(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.HasConstantValue, _ => symbol.HasConstantValue),
                    new MetadataItem(CodeAnalysisKeys.ConstantValue, _ => symbol.ConstantValue),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        public override void VisitEvent(IEventSymbol symbol)
        {
            if (ShouldIncludeSymbol(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.Overridden, DocumentFor(symbol.OverriddenEvent)),
                    new MetadataItem(CodeAnalysisKeys.Implements, DocumentsFor(GetImplements(symbol))),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString())
                });
            }
        }

        public override void VisitProperty(IPropertySymbol symbol)
        {
            if (ShouldIncludeSymbol(symbol))
            {
                AddMemberDocument(symbol, true, new MetadataItems
                {
                    new MetadataItem(CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Parameters, DocumentsFor(symbol.Parameters)),
                    new MetadataItem(CodeAnalysisKeys.Type, DocumentFor(symbol.Type)),
                    new MetadataItem(CodeAnalysisKeys.Overridden, DocumentFor(symbol.OverriddenProperty)),
                    new MetadataItem(CodeAnalysisKeys.Implements, DocumentsFor(GetImplements(symbol))),
                    new MetadataItem(CodeAnalysisKeys.Accessibility, _ => symbol.DeclaredAccessibility.ToString()),
                    new MetadataItem(CodeAnalysisKeys.Attributes, GetAttributeDocuments(symbol))
                });
            }
        }

        // Helpers below...

        private bool ShouldIncludeSymbol<TSymbol>(TSymbol symbol)
            where TSymbol : ISymbol
        {
            // Exclude the global auto-generated F# namespace (need to use .ToString() instead of .Name because it can have dots which act as nested namespaces)
            if (symbol.ToString().Contains("StartupCode$") || (symbol.ContainingNamespace?.ToString().Contains("StartupCode$") ?? false))
            {
                return false;
            }

            // Always include symbols if we're already finished since they're being created for lookups at that point
            if (_finished)
            {
                return true;
            }

            // Exclude implicit symbols
            // but always include the global namespace (might be excluded in the predicate though)
            if (symbol.IsImplicitlyDeclared
                && (!(symbol is INamespaceSymbol namespaceSymbol) || !namespaceSymbol.IsGlobalNamespace))
            {
                return false;
            }

            return _symbolPredicate is null || _symbolPredicate(symbol, _compilation);
        }

        // This was helpful: http://stackoverflow.com/a/30445814/807064
        private IEnumerable<ISymbol> GetAccessibleMembersInThisAndBaseTypes(ITypeSymbol containingType, ISymbol within)
        {
            List<ISymbol> members = ((IEnumerable<ISymbol>)_getAccessibleMembersInThisAndBaseTypes.Invoke(null, new object[] { containingType, within })).ToList();

            // Remove overridden symbols
            ImmutableHashSet<ISymbol> remove = members
                .Select(x => (ISymbol)(x as IMethodSymbol)?.OverriddenMethod ?? (x as IPropertySymbol)?.OverriddenProperty)
                .Where(x => x is object)
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
            members.RemoveAll(x => remove.Contains(x));
            return members;
        }

        internal static IEnumerable<INamedTypeSymbol> GetBaseTypes(ITypeSymbol type)
        {
            INamedTypeSymbol current = type.BaseType;
            while (current is object)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        // Gets the interface symbols that this symbol implements
        internal static IEnumerable<ISymbol> GetImplements(ISymbol symbol) =>
            symbol.ContainingType
                ?.AllInterfaces
                .SelectMany(x => x.GetMembers())
                .Where(x => SymbolEqualityComparer.Default.Equals(symbol, symbol.ContainingType.FindImplementationForInterfaceMember(x)))
                .ToArray();

        private bool MemberPredicate(ISymbol symbol)
        {
            IPropertySymbol propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol?.IsIndexer == true)
            {
                // Special case for indexers
                return true;
            }
            return symbol.CanBeReferencedByName && !symbol.IsImplicitlyDeclared;
        }

        private bool OperatorPredicate(ISymbol symbol) =>
            symbol is IMethodSymbol method && (method.MethodKind == MethodKind.Conversion || method.MethodKind == MethodKind.UserDefinedOperator);

        private IDocument AddMemberDocument(ISymbol symbol, bool xmlDocumentation, MetadataItems items)
        {
            items.AddRange(new[]
            {
                new MetadataItem(CodeAnalysisKeys.ContainingType, DocumentFor(symbol.ContainingType))
            });
            return AddDocumentCommon(symbol, xmlDocumentation, items);
        }

        private IDocument AddNamespaceDocument(INamespaceSymbol symbol, bool xmlDocumentation)
        {
            string displayName = symbol.GetDisplayName();
            MetadataItems items = new MetadataItems
            {
                { CodeAnalysisKeys.SpecificKind, _ => symbol.Kind.ToString() },

                // We need to aggregate the results across all matching namespaces
                { CodeAnalysisKeys.MemberNamespaces, DocumentsFor(_namespaceDisplayNameToSymbols[displayName].SelectMany(x => x.GetNamespaceMembers())) },
                { CodeAnalysisKeys.MemberTypes, DocumentsFor(_namespaceDisplayNameToSymbols[displayName].SelectMany(x => x.GetTypeMembers())) }
            };
            return AddDocumentCommon(symbol, xmlDocumentation, items);
        }

        // Used for everything
        private IDocument AddDocumentCommon(ISymbol symbol, bool xmlDocumentation, MetadataItems items)
        {
            // Get universal metadata
            string commentId = symbol.GetDocumentationCommentId();
            items.AddRange(new[]
            {
                // In general, cache the values that need calculation and don't cache the ones that are just properties of ISymbol
                new MetadataItem(CodeAnalysisKeys.IsResult, !_finished),
                new MetadataItem(CodeAnalysisKeys.SymbolId, _ => symbol.GetId(), true),
                new MetadataItem(CodeAnalysisKeys.CommentId, commentId),
                new MetadataItem(CodeAnalysisKeys.Name, metadata => string.IsNullOrEmpty(symbol.Name) ? metadata.GetString(CodeAnalysisKeys.FullName) : symbol.Name),
                new MetadataItem(CodeAnalysisKeys.FullName, _ => symbol.GetFullName(), true),
                new MetadataItem(CodeAnalysisKeys.DisplayName, _ => symbol.GetDisplayName(), true),
                new MetadataItem(Keys.Title, _ => symbol.GetDisplayName(), true),
                new MetadataItem(CodeAnalysisKeys.QualifiedName, _ => symbol.GetQualifiedName(), true),
                new MetadataItem(CodeAnalysisKeys.Kind, _ => symbol.Kind.ToString()),
                new MetadataItem(CodeAnalysisKeys.ContainingNamespace, DocumentFor(symbol.ContainingNamespace)),
                new MetadataItem(CodeAnalysisKeys.Syntax, _ => GetSyntax(symbol), true),
                new MetadataItem(CodeAnalysisKeys.IsStatic, _ => symbol.IsStatic),
                new MetadataItem(CodeAnalysisKeys.IsAbstract, _ => symbol.IsAbstract),
                new MetadataItem(CodeAnalysisKeys.IsVirtual, _ => symbol.IsVirtual),
                new MetadataItem(CodeAnalysisKeys.IsOverride, _ => symbol.IsOverride),
                new MetadataItem(CodeAnalysisKeys.OriginalDefinition, DocumentFor(GetOriginalSymbolDefinition(symbol))),
                new MetadataItem(CodeAnalysisKeys.Compilation, _compilation)
            });

            // If it's a namespace look up the common symbol using the name, otherwise add the original symbol
            if (symbol is INamespaceSymbol)
            {
                items.Add(new MetadataItem(CodeAnalysisKeys.Symbol, _ => _namespaceDisplayNameToSymbols[symbol.GetDisplayName()].ToImmutableList()));
            }
            else
            {
                items.Add(new MetadataItem(CodeAnalysisKeys.Symbol, symbol));
            }

            // Add the containing assembly, but only if it's not the code analysis compilation
            if (symbol.ContainingAssembly?.Name != _compilation.AssemblyName && _assemblySymbols)
            {
                items.Add(new MetadataItem(CodeAnalysisKeys.ContainingAssembly, DocumentFor(symbol.ContainingAssembly)));
            }

            // XML Documentation
            if (xmlDocumentation && (!_finished || _docsForImplicitSymbols))
            {
                AddXmlDocumentation(symbol, items);
            }

            // Add a destination for initially-processed symbols
            NormalizedPath destination = _finished ? null : _destination(symbol, _compilation);

            // Create the document and add it to caches
            // Use a special ".symbol" extension for the source so we don't conflict with other known file types when consuming
            // We also need to use a special symbol display format to make sure the document sources are short enough,
            // then hash and combine the full display string to get a unique name
            return _symbolToDocument.GetOrAdd(
                symbol,
                (key, args) => args._context.CreateDocument(
                    new NormalizedPath(
                        $"{key.ToDisplayString(_documentSourceFormat)}_{key.GetId()}.symbol",
                        PathKind.Absolute),
                    args.destination,
                    args.items),
                (destination, items, _context));
        }

        private void AddXmlDocumentation(ISymbol symbol, MetadataItems metadata)
        {
            // Get the documentation comments
            INamespaceSymbol namespaceSymbol = symbol as INamespaceSymbol;
            string documentationCommentXml;
            lock (XmlDocLock)
            {
                // Need to lock the XML comment access or it sometimes doesn't get generated when using external XML doc files
                documentationCommentXml = namespaceSymbol is null
                    ? symbol.GetDocumentationCommentXml(expandIncludes: true)
                    : GetNamespaceDocumentationCommentXml(namespaceSymbol);
            }

            // Should we assume inheritdoc?
            if (string.IsNullOrEmpty(documentationCommentXml) && _implicitInheritDoc)
            {
                documentationCommentXml = "<inheritdoc/>";
            }

            // Create and parse the documentation comments
            XmlDocumentationParser xmlDocumentationParser
                = new XmlDocumentationParser(_context, symbol, _compilation, _symbolToDocument, _cssClasses);
            IEnumerable<string> otherHtmlElementNames = xmlDocumentationParser.Parse(documentationCommentXml);

            // Add standard HTML elements
            metadata.AddRange(new[]
            {
                new MetadataItem(CodeAnalysisKeys.CommentXml, documentationCommentXml),
                new MetadataItem(CodeAnalysisKeys.Example, _ => xmlDocumentationParser.Process().Example),
                new MetadataItem(CodeAnalysisKeys.Remarks, _ => xmlDocumentationParser.Process().Remarks),
                new MetadataItem(CodeAnalysisKeys.Summary, _ => xmlDocumentationParser.Process().Summary),
                new MetadataItem(CodeAnalysisKeys.Returns, _ => xmlDocumentationParser.Process().Returns),
                new MetadataItem(CodeAnalysisKeys.Value, _ => xmlDocumentationParser.Process().Value),
                new MetadataItem(CodeAnalysisKeys.Exceptions, _ => xmlDocumentationParser.Process().Exceptions),
                new MetadataItem(CodeAnalysisKeys.Permissions, _ => xmlDocumentationParser.Process().Permissions),
                new MetadataItem(CodeAnalysisKeys.Params, _ => xmlDocumentationParser.Process().Params),
                new MetadataItem(CodeAnalysisKeys.TypeParams, _ => xmlDocumentationParser.Process().TypeParams),
                new MetadataItem(CodeAnalysisKeys.SeeAlso, _ => xmlDocumentationParser.Process().SeeAlso)
            });

            // Add other HTML elements with keys of [ElementName]Html
            metadata.AddRange(otherHtmlElementNames.Select(x =>
                new MetadataItem(
                    FirstLetterToUpper(x) + "Comments",
                    _ => xmlDocumentationParser.Process().OtherComments[x])));
        }

        // This can be removed once changes in https://github.com/dotnet/roslyn/pull/15494 are merged and deployed
        private string GetNamespaceDocumentationCommentXml(INamespaceSymbol symbol)
        {
            // Try and get comments applied to the namespace
            TextWriter writer = new StringWriter();
            object diagnosticBag = _diagnosticBagGetInstance.Invoke(null, new object[] { });
            CancellationToken ct = default(CancellationToken);
            object documentationCompiler = Activator.CreateInstance(
                _documentationCommentCompiler,
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new object[]
                {
                    (string)null,
                    (CSharpCompilation)_compilation,
                    (TextWriter)writer,
                    (SyntaxTree)null,
                    (TextSpan?)null,
                    true,
                    true,
                    diagnosticBag,
                    ct
                },
                null);
            object underlyingSymbol = _publicModelSymbolUnderlyingType.GetValue(symbol);
            _documentationCommentCompilerDefaultVisit.Invoke(documentationCompiler, new object[] { underlyingSymbol });
            _diagnosticBagFree.Invoke(diagnosticBag, new object[] { });
            string docs = writer.ToString();

            // Fall back to looking for a NamespaceDoc class
            if (string.IsNullOrEmpty(docs))
            {
                INamespaceOrTypeSymbol namespaceDoc = symbol.GetMembers("NamespaceDoc").FirstOrDefault();
                if (namespaceDoc is object)
                {
                    return namespaceDoc.GetDocumentationCommentXml(expandIncludes: true);
                }
            }

            return docs ?? string.Empty;
        }

        public static string FirstLetterToUpper(string str)
        {
            if (str is null)
            {
                return null;
            }

            if (str.Length > 1)
            {
                return char.ToUpper(str[0]) + str.Substring(1);
            }

            return str.ToUpper();
        }

        private IReadOnlyList<IDocument> GetDerivedTypes(INamedTypeSymbol symbol) =>
            _namedTypes
                .Where(x => x.Key.BaseType is object && SymbolEqualityComparer.Default.Equals(GetOriginalSymbolDefinition(x.Key.BaseType), GetOriginalSymbolDefinition(symbol)))
                .Select(x => x.Value)
                .ToImmutableArray();

        private IReadOnlyList<IDocument> GetImplementingTypes(INamedTypeSymbol symbol) =>
            _namedTypes
                .Where(x => x.Key.AllInterfaces
                    .Select(GetOriginalSymbolDefinition)
                    .Contains(GetOriginalSymbolDefinition(symbol), SymbolEqualityComparer.Default))
                .Select(x => x.Value)
                .ToImmutableArray();

        private string GetSyntax(ISymbol symbol) => SyntaxHelper.GetSyntax(symbol);

        private IReadOnlyList<IDocument> GetAttributeDocuments(ISymbol symbol) =>
            symbol.GetAttributes().Select(attributeData => _context.CreateDocument(new MetadataItems
            {
                { CodeAnalysisKeys.AttributeData, attributeData },
                { CodeAnalysisKeys.Type, DocumentFor(attributeData.AttributeClass) },
                { CodeAnalysisKeys.Name, attributeData.AttributeClass.Name }
            })).ToList();

        private SymbolDocumentValue DocumentFor(ISymbol symbol) =>
            new SymbolDocumentValue(symbol, this);

        private SymbolDocumentValues DocumentsFor(IEnumerable<ISymbol> symbols) =>
            new SymbolDocumentValues(symbols, this);

        public bool TryGetDocument(ISymbol symbol, out IDocument document)
        {
            if (!_finished)
            {
                throw new InvalidOperationException("Cannot access code analysis document references inside symbol visitor");
            }
            return _symbolToDocument.TryGetValue(symbol, out document);
        }

        // We need this because in many cases we don't really care about concrete generic types, only their definition
        // This converts all concrete generics into their original defintion
        // Unless the symbol is an error, in which case use the current definition since that has extra point-of-usage information (#702)
        // And unless the symbol is a Nullable<T>, in which case use the current definition since the original definition looses the type parameter (#610)
        // This method should always be used instead of ISymbol.OriginalDefinition directly
        private static TSymbol GetOriginalSymbolDefinition<TSymbol>(TSymbol symbol)
            where TSymbol : ISymbol =>
            symbol?.Kind == SymbolKind.ErrorType || symbol?.MetadataName == "Nullable`1" ? symbol : (TSymbol)(symbol?.OriginalDefinition ?? symbol);

        // Method added due to changes in underlying Analyzers. GetAccessibleMembersInThisAndBaseTypes is referenced multiple
        // times so we search for the exact one that we need.
        private MethodInfo FindAccessibleMembersMethod(Assembly workspacesAssembly, Assembly csharpAssembly)
        {
            string targetTypeName = "Microsoft.CodeAnalysis.Shared.Extensions.ITypeSymbolExtensions";
            string methodName = "GetAccessibleMembersInThisAndBaseTypes";
            string param0FullName = "Microsoft.CodeAnalysis.ITypeSymbol";
            string param1FullName = "Microsoft.CodeAnalysis.ISymbol";

            IEnumerable<Assembly> assembliesToSearch = new[] { workspacesAssembly, csharpAssembly }.Distinct();

            foreach (var asm in assembliesToSearch)
            {
                // Try direct type lookup first
                Type candidateType = asm.GetType(targetTypeName, throwOnError: false, ignoreCase: true);

                // Fallback: try to find a type with matching short name (handles internal type moves)
                if (candidateType == null)
                {
                    candidateType = asm.GetTypes().FirstOrDefault(t =>
                        t.Name == "ITypeSymbolExtensions" ||
                        (t.FullName != null && t.FullName.EndsWith(".ITypeSymbolExtensions")));
                }

                if (candidateType == null)
                {
                    continue;
                }

                var candidates = candidateType
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => string.Equals(m.Name, methodName, StringComparison.Ordinal))
                    .Where(m => m.IsGenericMethodDefinition && m.GetGenericArguments().Length == 1);

                foreach (var m in candidates)
                {
                    var ps = m.GetParameters();
                    if (ps.Length != 2) continue;

                    bool p0Matches = ps[0].ParameterType.FullName == param0FullName ||
                                     ps[0].ParameterType.Name == "ITypeSymbol";
                    bool p1Matches = ps[1].ParameterType.FullName == param1FullName ||
                                     ps[1].ParameterType.Name == "ISymbol";

                    if (p0Matches && p1Matches)
                    {
                        return m;
                    }
                }
            }

            return null;
        }
    }
}