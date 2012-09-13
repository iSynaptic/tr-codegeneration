using System;
using System.Collections.Generic;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class WebApiQuerySyntax : WebApiOperationSyntax, IWebApiPathSyntaxMember
    {
        public WebApiQuerySyntax(IEnumerable<Annotation> annotations, TypeReferenceSyntax result, Identifier name, IEnumerable<AtomSyntax> arguments, IEnumerable<AtomSyntax> filters)
            : base(annotations, result, name, arguments)
        {
            Filters = Guard.NotNull(filters, "filters");
        }

        public IEnumerable<AtomSyntax> Filters { get; private set; }

        public override WebApiOperationSyntaxType OperationType
        {
            get { return WebApiOperationSyntaxType.Query; }
        }
    }
}
