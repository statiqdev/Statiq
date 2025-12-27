using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    internal class StatiqDocumentAccessor : IObjectAccessor
    {
        // Built-in properties we want to expose but aren't available via metadata.
        private static readonly string[] PropertyNames =
        {
            nameof(IDocument.Id),
            nameof(IDocument.Count),
            nameof(IDocument.Keys),
            nameof(IDocument.Values)
        };

        private readonly ConcurrentDictionary<IDocument, ImmutableDictionary<string, string>> _documentMetadataCache;
        private readonly ImmutableDictionary<string, string> _properties;
        private readonly MemberRenamerDelegate _renamer;

        public StatiqDocumentAccessor(MemberRenamerDelegate memberRenamer)
        {
            _renamer = memberRenamer;
            _documentMetadataCache = new ConcurrentDictionary<IDocument, ImmutableDictionary<string, string>>(DocumentIdComparer.Instance);
            _properties = PropertyNames
                .ToImmutableDictionary(Rename);
        }

        public int GetMemberCount(TemplateContext context, SourceSpan span, object target) =>
            GetMemberCount(target as IDocument);

        public int GetMemberCount(IDocument document) =>
            _properties.Count + GetMetadata(document).Count;

        public IEnumerable<string> GetMembers(TemplateContext context, SourceSpan span, object target) =>
            GetMembers(target as IDocument);

        public IEnumerable<string> GetMembers(IDocument document) =>
            _properties.Keys.Concat(GetMetadata(document).Keys).Distinct();

        public bool HasMember(TemplateContext context, SourceSpan span, object target, string member) =>
            HasMember(target as IDocument, member);

        public bool HasMember(IDocument document, string member) =>
            GetMetadata(document).ContainsKey(member) || _properties.ContainsKey(member);

        public bool TryGetValue(TemplateContext context, SourceSpan span, object target, string member, out object value)
        {
            value = null;

            if (!(target is IDocument document))
            {
                return false;
            }

            if (GetMetadata(document).TryGetValue(member, out string metadataName))
            {
                return document.TryGetValue(metadataName, out value);
            }

            if (_properties.TryGetValue(member, out string propertyName))
            {
                value = propertyName switch
                {
                    nameof(IDocument.Id) => document.Id,
                    nameof(IDocument.Count) => document.Count,
                    nameof(IDocument.Keys) => document.Keys.Select(Rename),
                    nameof(IDocument.Values) => document.Values,
                    _ => null
                };

                return true;
            }

            return false;
        }

        public bool TrySetValue(TemplateContext context, SourceSpan span, object target, string member, object value) => false;
        public bool TryGetItem(TemplateContext context, SourceSpan span, object target, object index, out object value)
        {
            throw new NotImplementedException();
        }

        public bool TrySetItem(TemplateContext context, SourceSpan span, object target, object index, object value)
        {
            throw new NotImplementedException();
        }

        public bool HasIndexer { get; }
        public Type IndexType { get; }

        private ImmutableDictionary<string, string> GetMetadata(IDocument document)
        {
            if (document is null)
            {
                return ImmutableDictionary<string, string>.Empty;
            }

            return _documentMetadataCache
                .GetOrAdd(document, x => x.Keys.ToImmutableDictionary(Rename));
        }

        private string Rename(string member) => _renamer?.Invoke(new DocumentMemberInfo(member)) ?? member;
    }
}