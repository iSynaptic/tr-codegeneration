using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class SymbolTable
    {
        private static readonly List<Type> _BuiltInTypes;
        private static readonly Dictionary<QualifiedIdentifier, BuiltInType> _BuiltInTypesMap = new Dictionary<QualifiedIdentifier, BuiltInType>
        {
            {"void", new BuiltInType(typeof(void))},
            {"bool", new BuiltInType(typeof(bool))},
            {"byte", new BuiltInType(typeof(byte))},
            {"char", new BuiltInType(typeof(char))},
            {"decimal", new BuiltInType(typeof(decimal))},
            {"double", new BuiltInType(typeof(double))},
            {"float", new BuiltInType(typeof(float))},
            {"short", new BuiltInType(typeof(short))},
            {"int", new BuiltInType(typeof(int))},
            {"long", new BuiltInType(typeof(long))},
            {"uint", new BuiltInType(typeof(uint))},
            {"ulong", new BuiltInType(typeof(ulong))},
            {"ushort", new BuiltInType(typeof(ushort))},
            {"sbyte", new BuiltInType(typeof(sbyte))},
            {"string", new BuiltInType(typeof(string))},
            {"guid", new BuiltInType(typeof(Guid))},
            {"datetime", new BuiltInType(typeof(DateTime))}
        };

        private Outcome<string> _Outcome;
        private readonly Dictionary<QualifiedIdentifier, ISymbol> _SymbolMap = new Dictionary<QualifiedIdentifier, ISymbol>();
        private readonly IEnumerable<Assembly> _References;

        static SymbolTable()
        {
            _BuiltInTypes = _BuiltInTypesMap.Values
                .Select(x => x.Type)
                .ToList();
        }

        public SymbolTable(IEnumerable<SyntaxTree> trees, IEnumerable<Assembly> references)
        {
            Guard.NotNull(trees, "trees");
            _References = Guard.NotNull(references, "references").ToArray();

            var visitor = new FindTypeSyntaxVisitor(Define);
            visitor.Visit(trees);
        }

        private void Define(ISymbol node)
        {
            Guard.NotNull(node, "node");

            var existing = _SymbolMap
                .TryGetValue(node.FullName)
                .ValueOrDefault();

            if (existing != null)
            {
                if (existing.GetType() == node.GetType())
                    _Outcome &= Outcome.Failure(string.Format("Duplicate definition of '{0}'.", node.FullName.Last()));
                else
                    _Outcome &= Outcome.Failure(string.Format("'{0}' is defined as both a {1} and a {2}.", node.FullName.Last()), existing.GetType().Name, node.GetType().Name);
            }
            else
                _SymbolMap.Add(node.FullName, node);
        }

        public Outcome<string> Definition
        {
            get { return _Outcome; }
        }

        public Maybe<ISymbol> Resolve(QualifiedIdentifier identifier)
        {
            Guard.NotNull(identifier, "identifier");

            return _BuiltInTypesMap.TryGetValue(identifier).OfType<ISymbol>()
                .Or(() => _SymbolMap.TryGetValue(identifier))
                .Or(() => _References.Select(x => x.GetType(identifier.ToString(), false))
                                     .NotNull()
                                     .Select(x => _BuiltInTypes.Contains(x) ? new BuiltInType(x) : new ExternalType(x))
                                     .FirstOrDefault());
        }
    }
}
