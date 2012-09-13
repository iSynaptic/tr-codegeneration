using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors
{
    public abstract class ValidationVisitor : TypesLanguageSyntaxVisitor
    {
        private Outcome<string> _Outcome;
        private readonly SymbolTable _SymbolTable;

        protected ValidationVisitor(SymbolTable symbolTable)
        {
            _SymbolTable = Guard.NotNull(symbolTable, "symbolTable");
        }

        public override void VisitNamespaceSyntax(NamespaceSyntax @namespace)
        {
            var lastNamespace = Namespace;
            Namespace = @namespace;

            base.VisitNamespaceSyntax(@namespace);

            Namespace = lastNamespace;
        }

        protected Outcome<string> Validate<T>(IEnumerable<T> subject)
        {
            Visit(subject);
            return _Outcome;
        }

        protected Outcome<string> Validate<T>(T subject)
        {
            Visit(subject);
            return _Outcome;
        }

        protected Maybe<ISymbol> Resolve(TypeReferenceSyntax reference, ISyntaxNode scope)
        {
            return Resolve(reference, scope.ToMaybe());
        }

        protected Maybe<ISymbol> Resolve(TypeReferenceSyntax reference, Maybe<ISyntaxNode> scope)
        {
            return scope.OfType<IResolutionRoot>().SelectMaybe(x => x.Resolve(reference.Name, _SymbolTable));
        }

        protected void FailIf(bool shouldFail, string observation)
        {
            if (shouldFail)
                _Outcome &= Outcome.Failure(observation);
        }

        protected void FailIf(bool shouldFail, string observationFormat, params object[] values)
        {
            _Outcome = _Outcome.FailIf(shouldFail, string.Format(observationFormat, values));
        }

        protected void Fail(string observation)
        {
            _Outcome &= Outcome.Failure(observation);
        }

        protected void Fail(string observationFormat, params object[] values)
        {
            _Outcome &= Outcome.Failure(string.Format(observationFormat, values));
        }

        protected NamespaceSyntax Namespace { get; private set; }
    }
}
