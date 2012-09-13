using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class WebApiCommandSymbol : WebApiOperationSymbol, IWebApiPathMember
    {
        public WebApiCommandSymbol(BaseWebApiPathSymbol parent, IEnumerable<Annotation> annotations, TypeReference result, Identifier name, Maybe<AtomSymbol> argument)
            : base(annotations, parent, result, name, argument)
        {
        }

        public new BaseWebApiPathSymbol Parent { get { return (BaseWebApiPathSymbol) base.Parent; } }

        public override WebApiOperationType OperationType
        {
            get { return WebApiOperationType.Command; }
        }
    }
}
