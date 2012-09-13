using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class EventSymbol : MoleculeSymbol, IType
    {
        public EventSymbol(EntitySymbol parent, IEnumerable<Annotation> annotations, bool isAbstract, Identifier name, Maybe<TypeLookup> @base, IEnumerable<AtomSymbol> properties)
            : base(parent, annotations, isAbstract, name, @base, properties)
        {
        }

        public new EntitySymbol Parent { get { return (EntitySymbol)base.Parent; } }

        public new Maybe<EventSymbol> Base
        {
            get { return base.Base.Cast<EventSymbol>(); }
        }
    }
}
