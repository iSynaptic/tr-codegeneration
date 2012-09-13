using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public abstract class MoleculeSymbol : ISemanticNode, ISymbol, IAnnotatable
    {
        private Maybe<TypeLookup> _Base;

        protected MoleculeSymbol(ISymbol parent, IEnumerable<Annotation> annotations, bool isAbstract, Identifier name, Maybe<TypeLookup> @base, IEnumerable<AtomSymbol> properties)
        {
            Annotations = Guard.NotNull(annotations, "annotations")
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => new ReadOnlyCollection<Annotation>(x.ToList()))
                .ToReadOnlyDictionary();

            Parent = Guard.NotNull(parent, "parent");
            IsAbstract = isAbstract;
            Name = Guard.NotNull(name, "name");
            _Base = @base;
            Properties = Guard.NotNull(properties, "properties").ToArray();
        }

        public ISymbol Parent { get; private set; }
        Maybe<ISemanticNode> ISemanticNode.Parent { get { return Parent.ToMaybe<ISemanticNode>(); } }

        public QualifiedIdentifier FullName
        {
            get { return Parent.FullName + Name; }
        }

        public bool IsAbstract { get; private set; }
        public Identifier Name { get; private set; }
        public IEnumerable<AtomSymbol> Properties { get; private set; }

        public Maybe<MoleculeSymbol> Base
        {
            get
            {
                return _Base
                    .Select(x => x.Type)
                    .Cast<MoleculeSymbol>();
            }
        }

        public ReadOnlyDictionary<Identifier, ReadOnlyCollection<Annotation>> Annotations { get; private set; }
    }
}
