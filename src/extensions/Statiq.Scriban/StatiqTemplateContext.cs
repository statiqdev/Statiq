using Scriban;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    public class StatiqTemplateContext : TemplateContext
    {
        protected override IObjectAccessor GetMemberAccessorImpl(object target)
        {
            if (target is IDocument)
            {
                return new StatiqDocumentAccessor(MemberRenamer);
            }

            return base.GetMemberAccessorImpl(target);
        }
    }
}