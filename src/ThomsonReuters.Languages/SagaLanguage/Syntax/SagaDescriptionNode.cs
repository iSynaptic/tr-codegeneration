using System;
using System.Collections.Generic;

namespace ThomsonReuters.Languages.SagaLanguage.Syntax
{
    public class SagaModuleNode
    {
        public readonly IEnumerable<SagaNode> Sagas;
        public readonly IEnumerable<string> ImportedNamespaces;
        public readonly string Namespace;

        public SagaModuleNode(IEnumerable<SagaNode> sagas, string ns, IEnumerable<string> importedNamespaces)
        {
            Sagas = sagas;
            Namespace = ns;
            ImportedNamespaces = importedNamespaces;
        }
    }
}
