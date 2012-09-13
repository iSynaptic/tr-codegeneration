using System;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class SyntaxTree : IUsingsScope, IResolutionRoot
    {
        public SyntaxTree(IEnumerable<QualifiedIdentifier> usings, IEnumerable<NamespaceSyntax> namespaces)
        {
            Usings = Guard.NotNull(usings, "usings");
            Namespaces = Guard.NotNull(namespaces, "namespaces");
        }

        public Maybe<ISyntaxNode> Parent
        {
            get { return Maybe.NoValue; }
            set { throw new ArgumentException("A syntax tree node should have no parent.", "value"); }
        }

        public Maybe<ISymbol> Resolve(QualifiedIdentifier identifier, SymbolTable table)
        {
            return table.Resolve(identifier)
                .Or(() => Usings
                              .Select(x => table.Resolve(x + identifier))
                              .Squash()
                              .FirstOrDefault());
        }


        public IEnumerable<QualifiedIdentifier> Usings { get; private set; }
        public IEnumerable<NamespaceSyntax> Namespaces { get; private set; }
    }
}
