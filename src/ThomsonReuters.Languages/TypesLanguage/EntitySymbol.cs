using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class EntitySymbol : ISemanticNode, IType, INamespaceMember, IAnnotatable
    {
        private Maybe<TypeLookup> _Base;

        public EntitySymbol(NamespaceSymbol parent, IEnumerable<Annotation> annotations, bool isAbstract, Maybe<TypeReference> identityType, Identifier name, Maybe<TypeLookup> @base, Func<EntitySymbol, IEnumerable<EventSymbol>> events)
        {
            Annotations = Guard.NotNull(annotations, "annotations")
                .GroupBy(x => x.Name)
                .ToDictionary(x => x.Key, x => new ReadOnlyCollection<Annotation>(x.ToList()))
                .ToReadOnlyDictionary();

            Parent = Guard.NotNull(parent, "parent");
            IsAbstract = isAbstract;

            IdentityType = identityType;
            Name = Guard.NotNull(name, "name");
            _Base = @base;
            Events = Guard.NotNull(events, "events")(this).ToArray();
        }

        public QualifiedIdentifier FullName
        {
            get { return Parent.FullName + Name; }
        }

        public Maybe<TypeReference> IdentityType { get; private set; }

        public Maybe<EntitySymbol> Base
        {
            get
            {
                return _Base
                    .Select(x => x.Type)
                    .Cast<EntitySymbol>();
            }
        }

        public NamespaceSymbol Parent { get; private set; }
        Maybe<ISemanticNode> ISemanticNode.Parent { get { return Parent.ToMaybe<ISemanticNode>(); } }

        public bool IsAbstract { get; private set; }

        public Identifier Name { get; private set; }

        public IEnumerable<EventSymbol> Events { get; private set; }
        public ReadOnlyDictionary<Identifier, ReadOnlyCollection<Annotation>> Annotations { get; private set; }
    }
}
