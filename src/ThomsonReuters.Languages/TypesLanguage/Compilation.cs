using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class Compilation : INamespaceParent, ISemanticNode
    {
        public Compilation(Func<Compilation, IEnumerable<NamespaceSymbol>> namespaces)
        {
            Namespaces = Guard.NotNull(namespaces, "namespaces")(this).ToArray();
        }

        public IEnumerable<NamespaceSymbol> Namespaces { get; private set; }
        public Maybe<ISemanticNode> Parent { get { return Maybe<ISemanticNode>.NoValue; } }
    }
}
