using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class ExternalType : IType
    {
        public ExternalType(Type type)
        {
            Type = Guard.NotNull(type, "type");
        }

        public Type Type { get; private set; }

        public Identifier Name { get { return new Identifier(Type.Name); } }

        public QualifiedIdentifier FullName
        {
            get { return new QualifiedIdentifier(Type.FullName); }
        }
    }
}
