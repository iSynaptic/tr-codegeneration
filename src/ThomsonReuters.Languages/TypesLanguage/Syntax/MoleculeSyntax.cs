using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public abstract class MoleculeSyntax : ITypeSyntax, IResolutionRoot
    {
        private Maybe<ISyntaxNode> _Parent;

        protected MoleculeSyntax(IEnumerable<Annotation> annotations, bool isExternal, bool isAbstract, Identifier name, Maybe<TypeReferenceSyntax> @base, IEnumerable<PropertySyntax> properties)
        {
            Annotations = Guard.NotNull(annotations, "annotations");
            IsExternal = isExternal;
            IsAbstract = isAbstract;
            Name = Guard.NotNull(name, "name");
            Base = @base;
            Properties = Guard.NotNull(properties, "properties");
        }

        public Maybe<ISymbol> Resolve(QualifiedIdentifier identifier, SymbolTable table)
        {
            return table.Resolve(FullName + identifier)
                .Or(() => Parent.OfType<IResolutionRoot>().SelectMaybe(x => x.Resolve(identifier, table)));
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

        public bool IsExternal { get; private set; }
        public bool IsAbstract { get; private set; }
        public Identifier Name { get; private set; }

        public Maybe<TypeReferenceSyntax> Base { get; private set; }

        public IEnumerable<PropertySyntax> Properties { get; private set; }
        public IEnumerable<Annotation> Annotations { get; private set; }
    }
}
