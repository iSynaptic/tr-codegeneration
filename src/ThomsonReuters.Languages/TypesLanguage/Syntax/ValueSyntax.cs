using System;
using System.Collections.Generic;
using System.Linq;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax
{
    public class ValueSyntax : MoleculeSyntax, INamespaceSyntaxMember
    {
        public ValueSyntax(IEnumerable<Annotation> annotations, bool isExternal, bool isAbstract, Identifier name, Maybe<TypeReferenceSyntax> @base, IEnumerable<PropertySyntax> properties, IEnumerable<Identifier> equalBy)
            : base(annotations, isExternal, isAbstract, name, @base, properties)
        {
            EqualBy = Guard.NotNull(equalBy, "equalBy");
        }

        public IEnumerable<Identifier> EqualBy { get; private set; }
    }
}