using System;
using System.Reflection;
using Statiq.Common;

namespace Statiq.Scriban
{
    internal class DocumentMemberInfo : MemberInfo
    {
        public DocumentMemberInfo(string name)
        {
            Name = name;
        }

        public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();

        public override bool IsDefined(Type attributeType, bool inherit) => false;

        public override Type DeclaringType { get; } = typeof(IDocument);
        public override MemberTypes MemberType { get; } = MemberTypes.Property;
        public override string Name { get; }
        public override Type ReflectedType { get; } = typeof(IDocument);
    }
}