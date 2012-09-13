using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class WebApiPathSymbol : BaseWebApiPathSymbol, IWebApiPathMember
    {
        public WebApiPathSymbol(BaseWebApiPathSymbol parent, IEnumerable<Annotation> annotations, TypeReference result, Identifier name, Maybe<AtomSymbol> argument, Func<BaseWebApiQuerySymbol, IEnumerable<AtomSymbol>> filters, Func<BaseWebApiPathSymbol, IEnumerable<IWebApiPathMember>> members)
            : base(annotations, parent, result, name, argument, filters, members)
        {
        }

        public new BaseWebApiPathSymbol Parent { get { return (BaseWebApiPathSymbol)base.Parent; } }
    }
}
