using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Syntax.Visitors
{
    public class ParentAssignmentVisitor : TypesLanguageSyntaxVisitor
    {
        private ISyntaxNode _Parent = null;
        private ParentAssignmentVisitor()
        {
        }

        public static void AssignParent(SyntaxTree tree)
        {
            Guard.NotNull(tree, "tree");

            var visitor = new ParentAssignmentVisitor();
            visitor.Visit(tree);
        }

        public override void VisitSyntaxTree(SyntaxTree tree)
        {
            AsParent(tree, base.VisitSyntaxTree);
        }

        public override void VisitNamespaceSyntax(NamespaceSyntax @namespace)
        {
            @namespace.Parent = _Parent.ToMaybe();
            AsParent(@namespace, base.VisitNamespaceSyntax);
        }

        public override void VisitWebApiSyntax(WebApiSyntax webApi)
        {
            webApi.Parent = _Parent.ToMaybe();
            AsParent(webApi, base.VisitWebApiSyntax);
        }

        public override void VisitWebApiPathSyntax(WebApiPathSyntax path)
        {
            path.Parent = _Parent.ToMaybe();
            AsParent(path, base.VisitWebApiPathSyntax);
        }

        public override void VisitWebApiCommandSyntax(WebApiCommandSyntax command)
        {
            command.Parent = _Parent.ToMaybe();
            AsParent(command, base.VisitWebApiCommandSyntax);
        }

        public override void VisitWebApiQuerySyntax(WebApiQuerySyntax query)
        {
            query.Parent = _Parent.ToMaybe();
            AsParent(query, base.VisitWebApiQuerySyntax);
        }

        public override void VisitEntitySyntax(EntitySyntax entity)
        {
            entity.Parent = _Parent.ToMaybe();
            AsParent(entity, base.VisitEntitySyntax);
        }

        public override void VisitEventSyntax(EventSyntax @event)
        {
            @event.Parent = _Parent.ToMaybe();
            AsParent(@event, base.VisitEventSyntax);
        }

        public override void VisitValueSyntax(ValueSyntax value)
        {
            value.Parent = _Parent.ToMaybe();
            AsParent(value, base.VisitValueSyntax);
        }

        public override void VisitExternalEnumSyntax(ExternalEnumSyntax @enum)
        {
            @enum.Parent = _Parent.ToMaybe();
            AsParent(@enum, base.VisitExternalEnumSyntax);
        }

        public override void VisitPropertySyntax(PropertySyntax property)
        {
            property.Parent = _Parent.ToMaybe();
            AsParent(property, base.VisitPropertySyntax);
        }

        public override void VisitAtomSyntax(AtomSyntax atom)
        {
            atom.Parent = _Parent.ToMaybe();
            AsParent(atom, base.VisitAtomSyntax);
        }

        private void AsParent<T>(T node, Action<T> visitor) where T : ISyntaxNode
        {
            Guard.NotNull(node, "node");
            Guard.NotNull(visitor, "visitor");

            var lastParent = _Parent;
            _Parent = node;

            visitor(node);

            _Parent = lastParent;
        }
    }
}
