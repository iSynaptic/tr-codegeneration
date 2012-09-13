using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class NamespaceSyntaxGroup : ISymbol
    {
        public NamespaceSyntaxGroup(QualifiedIdentifier fullName, IEnumerable<NamespaceSyntax> namespaces, IEnumerable<NamespaceSyntaxGroup> groups)
        {
            FullName = Guard.NotNull(fullName, "fullName");
            Namespaces = Guard.NotNull(namespaces, "namespaces").ToArray();
            Groups = Guard.NotNull(groups, "groups").ToArray();
        }

        public static IEnumerable<NamespaceSyntaxGroup> GroupNamespaces(IEnumerable<SyntaxTree> trees)
        {
            Guard.NotNull(trees, "trees");

            return GroupNamespaces(trees.SelectMany(x => x.Namespaces));
        }

        public static IEnumerable<NamespaceSyntaxGroup> GroupNamespaces(IEnumerable<NamespaceSyntax> namespaces)
        {
            Guard.NotNull(namespaces, "namespaces");

            var flattened = namespaces.Select(x => new { Parent = (NamespaceSyntax)null, Namespace = x })
                .Recurse(x => x.Namespace.Members.OfType<NamespaceSyntax>().Select(y => new { Parent = x.Namespace, Namespace = y }))
                .GroupBy(x => x.Parent != null ? new QualifiedIdentifier(x.Parent.Name.Concat(x.Namespace.Name)) : x.Namespace.Name)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Namespace).ToArray());

            return GroupNamespaces(null, flattened, 0).ToArray();
        }

        private static IEnumerable<NamespaceSyntaxGroup> GroupNamespaces(QualifiedIdentifier parentName, Dictionary<QualifiedIdentifier, NamespaceSyntax[]> input, int skip)
        {
            var identifiers = input
                .Where(x => ReferenceEquals(parentName, null) || x.Key.StartsWith(parentName))
                .Select(x => x.Key
                    .OfType<Identifier>()
                    .Skip(skip)
                    .FirstOrDefault())
                .NotNull()
                .Distinct();

            foreach (var identifier in identifiers)
            {
                var qualifiedIdentifier = !ReferenceEquals(parentName, null)
                    ? new QualifiedIdentifier(parentName.Concat(new[] { identifier }))
                    : new QualifiedIdentifier(identifier);

                var namespaces = input
                    .TryGetValue(qualifiedIdentifier)
                    .ValueOrDefault(new NamespaceSyntax[] { });

                yield return new NamespaceSyntaxGroup(qualifiedIdentifier,
                                                      namespaces,
                                                      GroupNamespaces(qualifiedIdentifier, input, skip + 1));
            }
        }

        public Identifier Name { get { return FullName.Last(); } }
        public QualifiedIdentifier FullName { get; private set; }
        public IEnumerable<NamespaceSyntax> Namespaces { get; private set; }

        public IEnumerable<NamespaceSyntaxGroup> Groups { get; private set; }

        public Maybe<ISyntaxNode> Parent
        {
            get { return Maybe.NoValue; }
            set { }
        }
    }
}
