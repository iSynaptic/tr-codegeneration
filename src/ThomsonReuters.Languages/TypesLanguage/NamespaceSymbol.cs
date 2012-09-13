using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class NamespaceSymbol : INamespaceParent, INamespaceMember, ISymbol, ISemanticNode
    {
        public NamespaceSymbol(INamespaceParent parent, Identifier name, Func<NamespaceSymbol, IEnumerable<INamespaceMember>> members)
        {
            Parent = Guard.NotNull(parent, "parent");
            Name = Guard.NotNull(name, "name");

            Members = Guard.NotNull(members, "members")(this).ToArray();
        }

        public INamespaceParent Parent { get; private set; }
        public Identifier Name { get; private set; }

        public QualifiedIdentifier FullName
        {
            get
            {
                var parent = Parent as ISymbol;
                if (parent != null)
                    return parent.FullName + Name;

                return Name;
            }
        }

        public IEnumerable<INamespaceMember> Members { get; private set; }

        Maybe<ISemanticNode> ISemanticNode.Parent
        {
            get { return Parent.ToMaybe<ISemanticNode>(); }
        }
    }
}
