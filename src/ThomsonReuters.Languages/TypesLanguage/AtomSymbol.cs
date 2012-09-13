using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class AtomSymbol : TypeReference, ISymbol, IAnnotatable
    {
        public AtomSymbol(IEnumerable<Annotation> annotations, TypeLookup type, Cardinality cardinality, Identifier name)
            : base(type, cardinality)
        {
            Annotations = Guard.NotNull(annotations, "annotations")
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => new ReadOnlyCollection<Annotation>(x.ToList()))
                .ToReadOnlyDictionary();

            Name = Guard.NotNull(name, "name");
        }

        public Identifier Name { get; private set; }
        public QualifiedIdentifier FullName { get { return Name; } }

        public ReadOnlyDictionary<Identifier, ReadOnlyCollection<Annotation>> Annotations { get; private set; }
    }
}
