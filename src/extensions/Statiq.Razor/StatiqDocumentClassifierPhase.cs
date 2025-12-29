using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Statiq.Common;

namespace Statiq.Razor
{
    internal class StatiqDocumentClassifierPhase : RazorEnginePhaseBase
    {
        private static string _urlResolutionTagHelperTypeName = typeof(UrlResolutionTagHelper).FullName;

        private readonly string _baseType;  // null indicates the default StatiqRazorPage<>
        private readonly bool _isDocumentModel;
        private readonly string[] _namespaces;

        // Views
        public StatiqDocumentClassifierPhase(string baseType, bool isDocumentModel, string[] namespaces, RazorEngine engine)
        {
            _baseType = baseType;
            _isDocumentModel = isDocumentModel;
            _namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
            Engine = engine;
        }

        // Layouts and partials
        public StatiqDocumentClassifierPhase(string[] namespaces, RazorEngine engine)
        {
            _namespaces = namespaces ?? throw new ArgumentNullException(nameof(namespaces));
            Engine = engine;
        }

        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            // Get the top-level nodes
            DocumentIntermediateNode documentNode = codeDocument.GetDocumentIntermediateNode();
            NamespaceDeclarationIntermediateNode namespaceDeclaration =
                documentNode.Children.OfType<NamespaceDeclarationIntermediateNode>().Single();
            ClassDeclarationIntermediateNode classDeclaration =
                namespaceDeclaration.Children.OfType<ClassDeclarationIntermediateNode>().Single();

            // Make changes
            SetBaseType(documentNode, classDeclaration);
            AddNamespaces(namespaceDeclaration);
            RemoveUrlResolutionTagHelper(classDeclaration);

            // Set the new document
            codeDocument.SetDocumentIntermediateNode(documentNode);
        }

        // Get the class declaration and set the base type
        private void SetBaseType(DocumentIntermediateNode documentNode, ClassDeclarationIntermediateNode classDeclaration)
        {
            classDeclaration.BaseType = GetBaseType(documentNode);
        }

        private string GetBaseType(DocumentIntermediateNode documentNode)
        {
            // Get the model type (will return "dynamic" if there's no model directive)
            string modelType = ModelDirective.GetModelType(documentNode);

            // Null indicates the default base page
            if (_baseType is null)
            {
                // Use IDocument as the model type if not otherwise specified so that extensions, etc. work as expected
                if (modelType == "dynamic" && _isDocumentModel)
                {
                    return "Statiq.Razor.StatiqRazorPage<Statiq.Common.IDocument>";
                }

                return $"Statiq.Razor.StatiqRazorPage<TModel>";
            }

            // An explicit base page type was specified, so just replace the model generic type if there is one
            return _baseType.Replace("<TModel>", "<" + modelType + ">");
        }

        // Add namespaces
        private void AddNamespaces(NamespaceDeclarationIntermediateNode namespaceDeclaration)
        {
            HashSet<string> existing = namespaceDeclaration.Children
                .OfType<UsingDirectiveIntermediateNode>()
                .Select(x => x.Content)
                .ToHashSet();
            int insertIndex = namespaceDeclaration.Children.IndexOf(
                namespaceDeclaration.Children.OfType<UsingDirectiveIntermediateNode>().First());
            foreach (string ns in _namespaces.Where(x => !existing.Contains(x)))
            {
                namespaceDeclaration.Children.Insert(
                    insertIndex,
                    new UsingDirectiveIntermediateNode()
                    {
                        Content = ns
                    });
            }
        }

        // Remove the UrlResolutionTagHelper so that ~/ links don't get rewritten
        // See https://github.com/aspnet/Announcements/issues/57 and https://github.com/statiqdev/Statiq.Framework/issues/170
        private void RemoveUrlResolutionTagHelper(ClassDeclarationIntermediateNode classDeclaration)
        {
            // First remove the declaration
            MethodDeclarationIntermediateNode executeMethodDeclaration =
                classDeclaration.Children.OfType<MethodDeclarationIntermediateNode>().Single();
            executeMethodDeclaration.Children.RemoveAll(x => x is DirectiveIntermediateNode directive
                && directive.DirectiveName == "addTagHelper"
                && directive.Tokens.Any(x => x.Content.Contains(nameof(UrlResolutionTagHelper))));

            // By this time the tag helper has already been applied, so switch it out to one that doesn't do anything
            foreach (TagHelperIntermediateNode tagHelperNode in classDeclaration.FindDescendantNodes<TagHelperIntermediateNode>())
            {
                for (int i = 0; i < tagHelperNode.TagHelpers.Count; i++)
                {
                    if (tagHelperNode.TagHelpers[i].GetTypeName() == _urlResolutionTagHelperTypeName)
                    {
                        TagHelperDescriptorBuilder descriptorBuilder =
                            TagHelperDescriptorBuilder.Create(typeof(NoOpTagHelper).FullName, typeof(NoOpTagHelper).Assembly.GetName().Name);
                        descriptorBuilder.SetTypeName(typeof(NoOpTagHelper).FullName);
                        tagHelperNode.TagHelpers[i] = descriptorBuilder.Build();
                    }
                }
            }
        }
    }
}