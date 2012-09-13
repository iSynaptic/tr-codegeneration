using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ThomsonReuters.Languages.TypesLanguage
{
    public class TypesLanguageVisitor : Visitor
    {
        public virtual void VisitCompilation(Compilation compilation)
        {
            Visit(compilation.Namespaces);
        }

        public virtual void VisitNamespace(NamespaceSymbol @namespace)
        {
            Visit(@namespace.Members);
        }

        public virtual void VisitEntity(EntitySymbol entity)
        {
            Visit(entity.Events);
        }

        public virtual void VisitEvent(EventSymbol @event)
        {
            Visit(@event.Properties);
        }

        public virtual void VisitComplexValue(ComplexValueSymbol value)
        {
            Visit(value.Properties);
        }

        public virtual void VisitWebApi(WebApiSymbol webApi)
        {
            VisitBaseWebApiPath(webApi);
        }

        public virtual void VisitWebApiPath(WebApiPathSymbol path)
        {
            VisitBaseWebApiPath(path);
        }

        private void VisitBaseWebApiPath(BaseWebApiPathSymbol path)
        {
            Visit(path.Filters);
            Visit(path.Members);
        }

        public virtual void VisitWebApiCommand(WebApiCommandSymbol command)
        {
        }

        public virtual void VisitWebApiQuery(WebApiQuerySymbol query)
        {
            Visit(query.Filters);
        }

        public virtual void VisitAtom(AtomSymbol atom)
        {
        }
    }
}
