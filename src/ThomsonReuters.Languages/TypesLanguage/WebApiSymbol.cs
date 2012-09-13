using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class WebApiSymbol : BaseWebApiPathSymbol, IType, INamespaceMember
    {
        public WebApiSymbol(NamespaceSymbol parent, IEnumerable<Annotation> annotations, TypeReference result, Identifier name, Maybe<AtomSymbol> argument, Func<BaseWebApiQuerySymbol, IEnumerable<AtomSymbol>> filters, Func<BaseWebApiPathSymbol, IEnumerable<IWebApiPathMember>> members)
            : base(annotations, parent, result, name, argument, filters, members)
        {
        }

        public new NamespaceSymbol Parent { get { return (NamespaceSymbol) base.Parent; } }
    }
}
