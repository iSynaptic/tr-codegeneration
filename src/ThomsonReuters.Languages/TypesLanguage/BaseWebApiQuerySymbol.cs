using System;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public abstract class BaseWebApiQuerySymbol : WebApiOperationSymbol
    {
        protected BaseWebApiQuerySymbol(IEnumerable<Annotation> annotations, ISemanticNode parent, TypeReference result, Identifier name, Maybe<AtomSymbol> argument, Func<BaseWebApiQuerySymbol, IEnumerable<AtomSymbol>> filters)
            : base(annotations, parent, result, name, argument)
        {
            Filters = Guard.NotNull(filters, "filters")(this).ToArray();
        }

        public IEnumerable<AtomSymbol> Filters { get; private set; }

        public override WebApiOperationType OperationType
        {
            get { return WebApiOperationType.Query; }
        }
    }
}