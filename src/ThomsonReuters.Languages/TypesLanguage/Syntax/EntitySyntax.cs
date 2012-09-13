using System;
using System.Collections.Generic;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class EntitySyntax : ITypeSyntax, INamespaceSyntaxMember, IResolutionRoot
    {
        private Maybe<ISyntaxNode> _Parent;

        public EntitySyntax(IEnumerable<Annotation> annotations, bool isAbstract, Maybe<TypeReferenceSyntax> identityType, Identifier name, Maybe<TypeReferenceSyntax> @base, IEnumerable<EventSyntax> events)
        {
            Annotations = Guard.NotNull(annotations, "annotations");

            IsAbstract = isAbstract;
            IdentityType = identityType;
            Name = Guard.NotNull(name, "name");
            Base = @base;
            Events = Guard.NotNull(events, "events");
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

        public bool IsAbstract { get; private set; }
        public Maybe<TypeReferenceSyntax> IdentityType { get; private set; }
        public Identifier Name { get; private set; }

        public Maybe<TypeReferenceSyntax> Base { get; private set; }
        public IEnumerable<EventSyntax> Events { get; private set; }

        public IEnumerable<Annotation> Annotations { get; private set; }
    }
}