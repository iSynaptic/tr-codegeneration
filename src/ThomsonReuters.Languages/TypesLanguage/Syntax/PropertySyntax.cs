using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class PropertySyntax : AtomSyntax
    {
        public PropertySyntax(IEnumerable<Annotation> annotations, TypeReferenceSyntax type, Identifier name, Maybe<Identifier> aliases)
            : base(annotations, type, name)
        {
            Aliases = aliases;
        }

        public Maybe<Identifier> Aliases { get; private set; }
    }
}
