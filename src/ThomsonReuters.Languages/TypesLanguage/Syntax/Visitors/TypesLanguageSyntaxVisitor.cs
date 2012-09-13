namespace ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors
{
    public class TypesLanguageSyntaxVisitor : Visitor
    {
        public virtual void VisitSyntaxTree(SyntaxTree tree)
        {
            Visit(tree.Namespaces);
        }

        public virtual void VisitNamespaceSyntax(NamespaceSyntax @namespace)
        {
            Visit(@namespace.Members);
        }

        public virtual void VisitExternalEnumSyntax(ExternalEnumSyntax @enum)
        {
        }

        public virtual void VisitValueSyntax(ValueSyntax value)
        {
            Visit(value.Properties);
        }

        public virtual void VisitEntitySyntax(EntitySyntax entity)
        {
            Visit(entity.Events);
        }

        public virtual void VisitEventSyntax(EventSyntax @event)
        {
            Visit(@event.Properties);
        }

        public virtual void VisitWebApiSyntax(WebApiSyntax webApi)
        {
            Visit(webApi.Filters);
            Visit(webApi.Members);
        }

        public virtual void VisitWebApiPathSyntax(WebApiPathSyntax path)
        {
            Visit(path.Filters);
            Visit(path.Members);
        }

        public virtual void VisitWebApiCommandSyntax(WebApiCommandSyntax command)
        {
            Visit(command.Arguments);
        }

        public virtual void VisitWebApiQuerySyntax(WebApiQuerySyntax query)
        {
            Visit(query.Arguments);
            Visit(query.Filters);
        }

        public virtual void VisitPropertySyntax(PropertySyntax property)
        {
        }

        public virtual void VisitAtomSyntax(AtomSyntax atom)
        {
        }
    }
}
