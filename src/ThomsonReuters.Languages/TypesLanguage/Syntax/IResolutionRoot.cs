using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public interface IResolutionRoot
    {
        Maybe<ISymbol> Resolve(QualifiedIdentifier identifier, SymbolTable table);
    }
}
