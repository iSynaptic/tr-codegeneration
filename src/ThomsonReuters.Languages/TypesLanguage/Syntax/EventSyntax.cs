using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class EventSyntax : MoleculeSyntax
    {
        public EventSyntax(IEnumerable<Annotation> annotations, bool isAbstract, Identifier name, Maybe<TypeReferenceSyntax> @base, IEnumerable<PropertySyntax> properties)
            : base(annotations, false, isAbstract, name, @base, properties)
        {
        }
    }
}
