using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetaSharp.Transformation;

namespace ThomsonReuters.Languages
{
    public interface ISymbol
    {
        Identifier Name { get; }
        QualifiedIdentifier FullName { get; }
    }
}
