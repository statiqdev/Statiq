using System.Collections.Generic;
using System.Linq;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    internal class StatiqScriptObject : IScriptObject
    {
        private readonly StatiqDocumentAccessor _documentAccessor;
        private readonly IDocument _document;
        private readonly Dictionary<string, object> _locals;
        private readonly MemberRenamerDelegate _renamer;

        public StatiqScriptObject(IDocument document, MemberRenamerDelegate renamer, IEnumerable<KeyValuePair<string, object>> locals = null)
        {
            _document = document;
            _renamer = renamer;

            _documentAccessor = new StatiqDocumentAccessor(_renamer);
            _locals = locals?.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<string, object>();
        }

        public IEnumerable<string> GetMembers() =>
            _locals.Keys
            .Concat(_documentAccessor.GetMembers(_document))
            .Distinct();

        public bool Contains(string member)
            => _locals.ContainsKey(member) || _documentAccessor.HasMember(_document, member);

        public bool TryGetValue(TemplateContext context, SourceSpan span, string member, out object value)
        {
            if (_locals.TryGetValue(member, out value))
            {
                return true;
            }

            return _documentAccessor.TryGetValue(context, span, _document, member, out value);
        }

        public bool CanWrite(string member) => !_documentAccessor.HasMember(_document, member);
        public bool TrySetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
        {
            _locals[member] = value; // TODO: We are ignoring readOnly.

            return true;
        }

        public void SetValue(TemplateContext context, SourceSpan span, string member, object value, bool readOnly)
            => _locals[member] = value; // TODO: We are ignoring readOnly.

        public bool Remove(string member) => _locals.Remove(member);

        public void SetReadOnly(string member, bool readOnly)
        {
            // TODO: We are ignoring readOnly.
        }

        public IScriptObject Clone(bool deep) => new StatiqScriptObject(_document.Clone(_document.ContentProvider), _renamer, _locals);

        public int Count => _locals.Count + _documentAccessor.GetMemberCount(_document);

        public bool IsReadOnly { get; set; }
    }
}