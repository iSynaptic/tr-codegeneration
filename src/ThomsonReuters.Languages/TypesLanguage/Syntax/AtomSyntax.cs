using System;
using System.Collections.Generic;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class AtomSyntax : ISyntaxNode
    {
        private Maybe<ISyntaxNode> _Parent;

        public AtomSyntax(IEnumerable<Annotation> annotations, TypeReferenceSyntax type, Identifier name)
        {
            Annotations = Guard.NotNull(annotations, "annotations");

            Type = Guard.NotNull(type, "type");
            Name = Guard.NotNull(name, "name");
        }

        public Maybe<ISyntaxNode> Parent
        {
            get { return _Parent; }
            set
            {
                if(_Parent.HasValue)
                    throw new ArgumentException("You cannot change the node parent once it has already been set.", "value");

                _Parent = value;
            }
        }

        public TypeReferenceSyntax Type { get; private set; }
        public Identifier Name { get; private set; }

        public IEnumerable<Annotation> Annotations { get; private set; }
    }
}