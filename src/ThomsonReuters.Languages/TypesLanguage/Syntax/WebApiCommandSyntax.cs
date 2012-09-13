using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class WebApiCommandSyntax : WebApiOperationSyntax, IWebApiPathSyntaxMember
    {
        public WebApiCommandSyntax(IEnumerable<Annotation> annotations, TypeReferenceSyntax result, Identifier name, IEnumerable<AtomSyntax> arguments)
            : base(annotations, result, name, arguments)
        {
        }

        public override WebApiOperationSyntaxType OperationType
        {
            get { return WebApiOperationSyntaxType.Command; }
        }
    }
}
