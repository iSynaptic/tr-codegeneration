using System;
using System.Collections.Generic;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class WebApiPathSyntax : WebApiQuerySyntax
    {
        public WebApiPathSyntax(IEnumerable<Annotation> annotations, TypeReferenceSyntax result, Identifier name, IEnumerable<AtomSyntax> arguments, IEnumerable<AtomSyntax> filters, IEnumerable<IWebApiPathSyntaxMember> members)
            : base(annotations, result, name, arguments, filters)
        {
            Members = Guard.NotNull(members, "members");
        }

        public IEnumerable<IWebApiPathSyntaxMember> Members { get; private set; }

        public override WebApiOperationSyntaxType OperationType
        {
            get { return WebApiOperationSyntaxType.Path; }
        }
    }
}
