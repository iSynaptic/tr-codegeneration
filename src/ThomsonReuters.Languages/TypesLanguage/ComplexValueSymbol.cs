using System;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class ComplexValueSymbol : MoleculeSymbol, IValueSymbol, INamespaceMember
    {
        public ComplexValueSymbol(NamespaceSymbol parent, IEnumerable<Annotation> annotations,  bool isAbstract, Identifier name, Maybe<TypeLookup> @base, IEnumerable<AtomSymbol> properties, IEnumerable<Identifier> equalBy)
            : base(parent, annotations, isAbstract, name, @base, properties)
        {
            EqualBy = Guard.NotNull(equalBy, "equalBy").ToArray();
        }

        public new Maybe<ComplexValueSymbol> Base 
        {
            get { return base.Base.Cast<ComplexValueSymbol>(); }
        }

        public new NamespaceSymbol Parent { get { return (NamespaceSymbol) base.Parent; } }

        public IEnumerable<Identifier> EqualBy { get; private set; }
    }
}
