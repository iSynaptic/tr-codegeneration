using System;
using System.Collections.Generic;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class WebApiQuerySymbol : BaseWebApiQuerySymbol, IWebApiPathMember
    {
        public WebApiQuerySymbol(BaseWebApiPathSymbol parent, IEnumerable<Annotation> annotations, TypeReference result, Identifier name, Maybe<AtomSymbol> argument, Func<BaseWebApiQuerySymbol, IEnumerable<AtomSymbol>> filters)
            : base(annotations, parent, result, name, argument, filters)
        {
        }

        public new BaseWebApiPathSymbol Parent { get { return (BaseWebApiPathSymbol)base.Parent; } }
    }
}
