using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public interface IWebApiPathMember : ISymbol, ISemanticNode, IAnnotatable
    {
        new BaseWebApiPathSymbol Parent { get; }

        TypeReference Result { get; }
        Maybe<AtomSymbol> Argument { get; }
        WebApiOperationType OperationType { get; }
    }
}
