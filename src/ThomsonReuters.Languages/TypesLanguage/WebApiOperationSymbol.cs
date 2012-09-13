using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public abstract class WebApiOperationSymbol : ISemanticNode, ISymbol, IAnnotatable
    {
        protected WebApiOperationSymbol(IEnumerable<Annotation> annotations, ISemanticNode parent, TypeReference result, Identifier name, Maybe<AtomSymbol> argument)
        {
            Annotations = Guard.NotNull(annotations, "annotations")
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => new ReadOnlyCollection<Annotation>(x.ToList()))
                .ToReadOnlyDictionary();

            Parent = Guard.NotNull(parent, "parent");
            Result = Guard.NotNull(result, "result");
            Name = Guard.NotNull(name, "name");

            Argument = argument;
        }

        public ISemanticNode Parent { get; private set; }
        Maybe<ISemanticNode> ISemanticNode.Parent { get { return Parent.ToMaybe(); } }

        public QualifiedIdentifier FullName 
        {
            get
            {
                return Parent
                    .ToMaybe<ISymbol>()
                    .Select(x => x.FullName + Name)
                    .ValueOrDefault(Name);
            }
        }

        public TypeReference Result { get; private set; }
        public Identifier Name { get; private set; }

        public Maybe<AtomSymbol> Argument { get; private set; }

        public abstract WebApiOperationType OperationType { get; }

        public ReadOnlyDictionary<Identifier, ReadOnlyCollection<Annotation>> Annotations { get; private set; }
    }
}
