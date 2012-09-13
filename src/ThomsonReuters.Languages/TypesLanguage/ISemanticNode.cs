using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public interface ISemanticNode
    {
        Maybe<ISemanticNode> Parent { get; }
    }
}
