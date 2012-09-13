using System;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class TypeReferenceSyntax : ISyntaxNode
    {
        private Maybe<ISyntaxNode> _Parent;

        public TypeReferenceSyntax(QualifiedIdentifier name, Cardinality cardinality)
        {
            Name = Guard.NotNull(name, "name");
            Cardinality = Guard.NotNull(cardinality, "cardinality");
        }

        public Maybe<ISyntaxNode> Parent
        {
            get { return _Parent; }
            set
            {
                if (_Parent.HasValue)
                    throw new ArgumentException("You cannot change the node parent once it has already been set.", "value");

                _Parent = value;
            }
        }

        public QualifiedIdentifier Name { get; private set; }
        public Cardinality Cardinality { get; private set; }
    }
}
