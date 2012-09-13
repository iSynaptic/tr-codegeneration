using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public abstract class WebApiOperationSyntax : ISyntaxNode, IResolutionRoot
    {
        private Maybe<ISyntaxNode> _Parent;

        protected WebApiOperationSyntax(IEnumerable<Annotation> annotations, TypeReferenceSyntax result, Identifier name, IEnumerable<AtomSyntax> arguments)
        {
            Annotations = Guard.NotNull(annotations, "annotations");

            Result = Guard.NotNull(result, "result");
            Name = Guard.NotNull(name, "name");

            Arguments = Guard.NotNull(arguments, "arguments");
        }

        public Maybe<ISymbol> Resolve(QualifiedIdentifier identifier, SymbolTable table)
        {
            return table.Resolve(Name + identifier)
                .Or(() => Parent.OfType<IResolutionRoot>().SelectMaybe(x => x.Resolve(identifier, table)));
        }

        public TypeReferenceSyntax Result { get; private set; }
        public Identifier Name { get; private set; }

        public IEnumerable<AtomSyntax> Arguments { get; private set; }

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

        public IEnumerable<Annotation> Annotations { get; private set; }
        public abstract WebApiOperationSyntaxType OperationType { get; }
    }

    public enum WebApiOperationSyntaxType
    {
        Command,
        Query,
        Path
    }
}
