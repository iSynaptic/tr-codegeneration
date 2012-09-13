using System;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class NamespaceSyntax : INamespaceSyntaxMember, IUsingsScope, ISymbol, IResolutionRoot
    {
        private Maybe<ISyntaxNode> _Parent;

        public NamespaceSyntax(QualifiedIdentifier name, IEnumerable<QualifiedIdentifier> usings, IEnumerable<INamespaceSyntaxMember> members)
        {
            Name = Guard.NotNull(name, "name");
            Usings = Guard.NotNull(usings, "usings");
            Members = Guard.NotNull(members, "members");
        }

        public Maybe<ISymbol> Resolve(QualifiedIdentifier identifier, SymbolTable table)
        {
            var parentFullName = Parent.OfType<ISymbol>().Select(x => x.FullName);

            return table.Resolve(FullName + identifier)
                    .Or(() => Usings
                                .Select(x => table.Resolve(x + identifier))
                                .Squash()
                                .TryFirst())
                    .Or(() => Name.Parent
                                .Recurse(x => x.Parent)
                                .Select(x => parentFullName
                                        .Select(y => y + x + identifier)
                                        .Or(() => x + identifier)
                                        .SelectMaybe(table.Resolve))
                                .Squash()
                                .TryFirst())
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

        public QualifiedIdentifier Name { get; private set; }

        Identifier ISymbol.Name { get { return FullName.Last(); } }

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

        public IEnumerable<QualifiedIdentifier> Usings { get; private set; }
        public IEnumerable<INamespaceSyntaxMember> Members { get; private set; }
    }
}
