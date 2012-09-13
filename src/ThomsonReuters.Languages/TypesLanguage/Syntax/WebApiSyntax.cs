using System.Collections.Generic;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class WebApiSyntax : WebApiPathSyntax, ITypeSyntax, INamespaceSyntaxMember
    {
        public WebApiSyntax(IEnumerable<Annotation> annotations, TypeReferenceSyntax result, Identifier name, IEnumerable<AtomSyntax> arguments, IEnumerable<AtomSyntax> filters, IEnumerable<IWebApiPathSyntaxMember> members)
            : base(annotations, result, name, arguments, filters, members)
        {
        }

        public QualifiedIdentifier FullName
        {
            get
            {
                return Parent
                    .OfType<NamespaceSyntax>()
                    .Select(x => x.FullName + Name)
                    .ValueOrDefault(Name);
            }
        }
    }
}
