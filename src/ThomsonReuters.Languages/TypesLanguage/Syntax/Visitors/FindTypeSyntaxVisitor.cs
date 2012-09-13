using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;
using iSynaptic.Commons.Collections.Generic;
using iSynaptic.Commons.Linq;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors
{
    public class FindTypeSyntaxVisitor : TypesLanguageSyntaxVisitor
    {
        private readonly Action<ITypeSyntax> _OnTypeSyntax;

        public FindTypeSyntaxVisitor(Action<ITypeSyntax> onTypeSyntax)
        {
            _OnTypeSyntax = Guard.NotNull(onTypeSyntax, "onTypeSyntax");
        }

        public override void VisitEntitySyntax(EntitySyntax entity)
        {
            _OnTypeSyntax(entity);
            base.VisitEntitySyntax(entity);
        }

        public override void VisitEventSyntax(EventSyntax @event)
        {
            _OnTypeSyntax(@event);
            base.VisitEventSyntax(@event);
        }

        public override void VisitExternalEnumSyntax(ExternalEnumSyntax @enum)
        {
            _OnTypeSyntax(@enum);
            base.VisitExternalEnumSyntax(@enum);
        }

        public override void VisitValueSyntax(ValueSyntax value)
        {
            _OnTypeSyntax(value);
            base.VisitValueSyntax(value);
        }

        public override void VisitWebApiSyntax(WebApiSyntax webApi)
        {
            _OnTypeSyntax(webApi);
            base.VisitWebApiSyntax(webApi);
        }
    }
}
