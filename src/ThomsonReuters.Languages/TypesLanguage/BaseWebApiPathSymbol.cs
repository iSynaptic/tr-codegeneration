using System;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public abstract class BaseWebApiPathSymbol : BaseWebApiQuerySymbol
    {
        protected BaseWebApiPathSymbol(IEnumerable<Annotation> annotations, ISemanticNode parent, TypeReference result, Identifier name, Maybe<AtomSymbol> argument, Func<BaseWebApiQuerySymbol, IEnumerable<AtomSymbol>> filters, Func<BaseWebApiPathSymbol, IEnumerable<IWebApiPathMember>> members)
            : base(annotations, parent, result, name, argument, filters)
        {
            Members = Guard.NotNull(members, "members")(this).ToArray();
        }

        public IEnumerable<IWebApiPathMember> Members { get; private set; }
    }
}