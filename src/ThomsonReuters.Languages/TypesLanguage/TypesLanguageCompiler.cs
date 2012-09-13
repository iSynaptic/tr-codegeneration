using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ThomsonReuters.Languages.TypesLanguage.Syntax;
using ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class TypesLanguageCompilerInput
    {
        public TypesLanguageCompilerInput(string text, string sourceName)
        {
            Text = Guard.NotNull(text, "text");
            SourceName = sourceName;
        }

        public string Text { get; private set; }
        public string SourceName { get; private set; }
    }

    public class TypesLanguageCompiler
    {
        private readonly SymbolTable _SymbolTable = null;
        private readonly Dictionary<QualifiedIdentifier, IType> _TypeMap = new Dictionary<QualifiedIdentifier, IType>();

        private TypesLanguageCompiler(SymbolTable symbolTable)
        {
            _SymbolTable = Guard.NotNull(symbolTable, "symbolTable");
        }

        public static Result<Compilation, string> Compile(IEnumerable<string> input, IEnumerable<Assembly> references)
        {
            return Compile(input.Select(x => new TypesLanguageCompilerInput(File.ReadAllText(x), new FileInfo(x).Name)), references);
        }

        public static Result<Compilation, string> Compile(IEnumerable<TypesLanguageCompilerInput> input, IEnumerable<Assembly> references)
        {
            Guard.NotNull(input, "input");
            Guard.NotNull(references, "references");

            var parseResults = input.Select(x => TypesLanguageParser.Parse(x.Text, x.SourceName)).ToArray();

            var outcome = parseResults
                .Select(x => x.ToOutcome()
                    .Inform(y => y.Line
                        .Join(y.Column, (l, c) => string.Format("{0} ({1}: line:{2}, column:{3})", y.Message, y.SourceName ?? "{input}", l, c)).ValueOrDefault(() => y.Message)))
                .Combine();

            if (!outcome.WasSuccessful)
                return outcome.ToResult();

            var trees = parseResults
                .Select(x => x.ToMaybe())
                .Squash()
                .ToArray();

            var symbolTable = new SymbolTable(trees, references);
            outcome &= symbolTable.Definition &
                       SemanticRulesVisitor.Validate(trees, symbolTable);

            if (!outcome.WasSuccessful)
                return outcome.Combine(symbolTable.Definition).ToResult();

            var compiler = new TypesLanguageCompiler(symbolTable);

            return compiler.BuildCompilation(trees)
                .ToResult<Compilation, string>();
        }

        private T Map<T>(QualifiedIdentifier name, T type)
            where T : IType
        {
            _TypeMap.Add(name, type);
            return type;
        }

        private Compilation BuildCompilation(IEnumerable<SyntaxTree> trees)
        {
            var groups = NamespaceSyntaxGroup.GroupNamespaces(trees).ToArray();
            return new Compilation(c => BuildNamespaces(c, groups));
        }

        private IEnumerable<NamespaceSymbol> BuildNamespaces(INamespaceParent parent, IEnumerable<NamespaceSyntaxGroup> namespaceSyntaxes)
        {
            return namespaceSyntaxes.Select(ns => new NamespaceSymbol(parent,
                                                                      ns.FullName.Last(),
                                                                      x => BuildTypes(x, ns.Namespaces.SelectMany(y => y.Members).Unless(y => y is NamespaceSyntax))
                                                                               .Concat(BuildNamespaces(x, ns.Groups)).ToArray()));
        }

        private IEnumerable<INamespaceMember> BuildTypes(NamespaceSymbol parent, IEnumerable<INamespaceSyntaxMember> members)
        {
            foreach (var member in members)
            {
                var entity = member as EntitySyntax;
                if (entity != null)
                    yield return BuildEntity(parent, entity);

                var value = member as ValueSyntax;
                if (value != null)
                    yield return BuildValue(parent, value);

                var externalEnum = member as ExternalEnumSyntax;
                if (externalEnum != null)
                    yield return BuildExternalEnum(parent, externalEnum);

                var webApi = member as WebApiSyntax;
                if (webApi != null)
                    yield return BuildWebApi(parent, webApi);
            }
        }

        private INamespaceMember BuildWebApi(NamespaceSymbol parent, WebApiSyntax webApi)
        {
            return new WebApiSymbol(parent,
                                    webApi.Annotations,
                                    Resolve(webApi.Result, webApi.Parent),
                                    webApi.Name,
                                    webApi.Arguments
                                        .NotNull()
                                        .TrySingle()
                                        .Select(x => BuildAtom(x, webApi))
                                        .Run(),
                                    p => BuildAtoms(webApi.Filters, webApi),
                                    p => BuildWebApiPathMembers(p, webApi.Members));
        }

        private IEnumerable<WebApiPathSymbol> BuildWebApiPaths(BaseWebApiPathSymbol parent, IEnumerable<WebApiPathSyntax> paths)
        {
            return paths.Select(path => new WebApiPathSymbol(parent,
                                                             path.Annotations,
                                                             Resolve(path.Result, path.Parent),
                                                             path.Name,
                                                             path.Arguments
                                                                .NotNull()
                                                                .TrySingle()
                                                                .Select(x => BuildAtom(x, path))
                                                                .Run(),
                                                             p => BuildAtoms(path.Filters, path),
                                                             p => BuildWebApiPathMembers(p, path.Members)));
        }

        private IEnumerable<IWebApiPathMember> BuildWebApiPathMembers(BaseWebApiPathSymbol parent, IEnumerable<IWebApiPathSyntaxMember> members)
        {
            var ops = members.ToArray();

            return BuildWebApiCommands(parent, ops.OfType<WebApiCommandSyntax>()).OfType<IWebApiPathMember>()
                .Concat(BuildWebApiQueries(parent, ops.Where(x => x.GetType() == typeof(WebApiQuerySyntax)).OfType<WebApiQuerySyntax>()))
                .Concat(BuildWebApiPaths(parent, ops.OfType<WebApiPathSyntax>()));
        }

        private IEnumerable<WebApiCommandSymbol> BuildWebApiCommands(BaseWebApiPathSymbol parent, IEnumerable<WebApiCommandSyntax> commands)
        {
            foreach (var command in commands)
            {
                var argument = command.Arguments
                    .NotNull()
                    .TrySingle()
                    .Select(x => BuildAtom(x, command))
                    .Run();

                yield return new WebApiCommandSymbol(parent, command.Annotations, Resolve(command.Result, command), command.Name, argument);
            }
        }

        private IEnumerable<WebApiQuerySymbol> BuildWebApiQueries(BaseWebApiPathSymbol parent, IEnumerable<WebApiQuerySyntax> queries)
        {
            foreach (var query in queries)
            {
                var argument = query.Arguments
                    .NotNull()
                    .TrySingle()
                    .Select(x => BuildAtom(x, query))
                    .Run();

                yield return new WebApiQuerySymbol(parent, query.Annotations, Resolve(query.Result, query), query.Name, argument, q => BuildAtoms(query.Filters, query));
            }
        }

        private INamespaceMember BuildExternalEnum(NamespaceSymbol parent, ExternalEnumSyntax @enum)
        {
            return Map(@enum.FullName, new ExternalEnumSymbol(parent, @enum.Annotations, @enum.Name));
        }

        private INamespaceMember BuildEntity(NamespaceSymbol parent, EntitySyntax entity)
        {
            var baseEntity = entity.Base.Select(x => Lookup(x.Name, entity.Parent.ValueOrDefault()));

            return Map(entity.FullName, new EntitySymbol(parent,
                                          entity.Annotations,
                                          entity.IsAbstract,
                                          entity.IdentityType.Select(x => Resolve(x, entity)),
                                          entity.Name,
                                          baseEntity,
                                          v => BuildEvents(v, entity.Events, entity)));
        }

        private IEnumerable<EventSymbol> BuildEvents(EntitySymbol parent, IEnumerable<EventSyntax> events, ISyntaxNode context)
        {
            return events.Select(@event => Map(@event.FullName, new EventSymbol(parent,
                                                                                @event.Annotations,
                                                                                @event.IsAbstract,
                                                                                @event.Name,
                                                                                @event.Base.Select(x => Lookup(x.Name, @event.Parent.ValueOrDefault())),
                                                                                BuildAtoms(@event.Properties, @event))));
        }

        private INamespaceMember BuildValue(NamespaceSymbol parent, ValueSyntax value)
        {
            var baseValue = value.Base.Select(x => Lookup(x.Name, value.Parent.ValueOrDefault()));

            if(value.IsExternal)
            {
                return Map(value.FullName, new ExternalValueSymbol(parent,
                                                                   value.Annotations,
                                                                   value.Name,
                                                                   baseValue));
            }

            return Map(value.FullName, new ComplexValueSymbol(parent,
                                          value.Annotations,
                                          value.IsAbstract,
                                          value.Name,
                                          baseValue,
                                          BuildAtoms(value.Properties, value),
                                          value.EqualBy));
        }

        private IEnumerable<AtomSymbol> BuildAtoms(IEnumerable<AtomSyntax> atoms, ISyntaxNode context)
        {
            return atoms.Select(atom => BuildAtom(atom, context));
        }

        private AtomSymbol BuildAtom(AtomSyntax atom, ISyntaxNode context)
        {
            Guard.NotNull(atom, "atom");
            Guard.NotNull(context, "context");

            return new AtomSymbol(atom.Annotations, Lookup(atom.Type.Name, context), atom.Type.Cardinality, atom.Name);
        }

        private Maybe<TypeReference> Resolve(Maybe<TypeReferenceSyntax> typeReference, Maybe<ISyntaxNode> context)
        {
            return typeReference.Select(x => Resolve(x, context.ValueOrDefault()));
        }

        private TypeReference Resolve(TypeReferenceSyntax typeReference, Maybe<ISyntaxNode> context)
        {
            return Resolve(typeReference, context.ValueOrDefault());
        }

        private TypeReference Resolve(TypeReferenceSyntax typeReference, ISyntaxNode context)
        {
            Guard.NotNull(typeReference, "typeReference");
            Guard.NotNull(context, "context");

            return new TypeReference(Lookup(typeReference.Name, context), typeReference.Cardinality);
        }

        private TypeLookup Lookup(QualifiedIdentifier identifier, ISyntaxNode context)
        {
            return new TypeLookup(() => context.ToMaybe<IResolutionRoot>()
                .SelectMaybe(x => x.Resolve(identifier, _SymbolTable))
                .Let(x => x.OfType<IType>()
                    .Or(() => x.OfType<ISymbol>().SelectMaybe(y => _TypeMap.TryGetValue(y.FullName))))
                .ValueOrDefault());
        }
    }
}
