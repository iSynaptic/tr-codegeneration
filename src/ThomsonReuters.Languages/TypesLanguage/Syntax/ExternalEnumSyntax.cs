using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class ExternalEnumSyntax : ITypeSyntax, INamespaceSyntaxMember
    {
        private Maybe<ISyntaxNode> _Parent;

        public ExternalEnumSyntax(IEnumerable<Annotation> annotations, Identifier name)
        {
            Annotations = Guard.NotNull(annotations, "annotations");

            Name = Guard.NotNull(name, "name");
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

        public QualifiedIdentifier FullName
        {
            get
            {
                return Parent
                    .OfType<NamespaceSyntax>()
                    .Select(x => x.FullName + Name)
                    .ValueOrDefault(Name);
            }
        }
        
        public Identifier Name { get; private set; }
        public IEnumerable<Annotation> Annotations { get; private set; }
    }
}
