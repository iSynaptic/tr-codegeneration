using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class TypeReference
    {
        private readonly TypeLookup _TypeLookup;

        public TypeReference(TypeLookup type, Cardinality cardinality)
        {
            _TypeLookup = Guard.NotNull(type, "type");
            Cardinality = Guard.NotNull(cardinality, "cardinality");
        }

        public IType Type { get { return _TypeLookup.Type; } }
        public Cardinality Cardinality { get; private set; }
    }
}
