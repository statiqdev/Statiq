using System;
using Microsoft.CodeAnalysis;
using Statiq.Common;

namespace Statiq.CodeAnalysis
{
    public static class ISymbolExtensions
    {
        /// <summary>
        /// Gets a unique ID for the symbol. Note that the symbol ID is
        /// not fully-qualified and is therefore only unique within a namespace.
        /// Nested types are prefixed with their parent type names.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>A unique (within a namespace) ID.</returns>
        public static string GetId(this ISymbol symbol)
        {
            symbol.ThrowIfNull(nameof(symbol));

            if (symbol is IAssemblySymbol)
            {
                return symbol.Name + ".dll";
            }
            if (symbol is INamespaceOrTypeSymbol)
            {
                // Get the ID for this symbol, replacing non-alpha/numeric/-/. with _
                char[] id = symbol.MetadataName.ToCharArray();
                for (int c = 0; c < id.Length; c++)
                {
                    if (!char.IsLetterOrDigit(id[c]) && id[c] != '-' && id[c] != '.')
                    {
                        id[c] = '_';
                    }
                }
                string symbolId = new string(id);

                // Prefix with the symbol ID of the containing type if there is one
                if (symbol.ContainingType is object)
                {
                    symbolId = symbol.ContainingType.GetId() + "." + symbolId;
                }

                return symbolId;
            }

            // Get a hash for anything other than namespaces or types
            return BitConverter.ToString(BitConverter.GetBytes(Crc32.Calculate(symbol.GetDocumentationCommentId() ?? GetFullName(symbol)))).Replace("-", string.Empty);
        }

        /// <summary>
        /// Gets the full name of the symbol. For namespaces, this is the name of the namespace.
        /// For types, this includes all generic type parameters.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>The full name of the symbol.</returns>
        public static string GetFullName(this ISymbol symbol)
        {
            symbol.ThrowIfNull(nameof(symbol));

            return symbol.ToDisplayString(new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType,
                memberOptions: SymbolDisplayMemberOptions.IncludeParameters,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes));
        }

        /// <summary>
        /// Gets the qualified name of the symbol which includes all containing namespaces.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>The qualified name of the symbol.</returns>
        public static string GetQualifiedName(this ISymbol symbol)
        {
            symbol.ThrowIfNull(nameof(symbol));

            return symbol.ToDisplayString(new SymbolDisplayFormat(
                memberOptions: SymbolDisplayMemberOptions.IncludeContainingType | SymbolDisplayMemberOptions.IncludeParameters,
                parameterOptions: SymbolDisplayParameterOptions.IncludeType,
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters));
        }

        /// <summary>
        /// Gets a display name for the symbol.
        /// For namespaces this is the same as the qualified name.
        /// For types this is the same as the full name.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns>The display name.</returns>
        public static string GetDisplayName(this ISymbol symbol)
        {
            if (symbol is IAssemblySymbol)
            {
                // Add .dll to assembly names
                return symbol.Name + ".dll";
            }
            if (symbol.Kind == SymbolKind.Namespace)
            {
                // Use "global" for the global namespace display name since it's a reserved keyword and it's used to refer to the global namespace in code
                return symbol.ContainingNamespace is null ? "global" : GetQualifiedName(symbol);
            }
            return GetFullName(symbol);
        }
    }
}