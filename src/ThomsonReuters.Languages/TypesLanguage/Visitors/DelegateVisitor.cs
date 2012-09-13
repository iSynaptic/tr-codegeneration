using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iSynaptic.Commons;

namespace ThomsonReuters.Languages.TypesLanguage.Visitors
{
    public class DelegateVisitor : TypesLanguageVisitor
    {
        public Func<object, Func<object, bool>, bool> OnNotInterestedIn { get; set; }
        protected override bool NotInterestedIn(object subject)
        {
            return Execute(subject, OnNotInterestedIn, base.NotInterestedIn);
        }

        public Action<AtomSymbol, Action<AtomSymbol>> OnAtom { get; set; }
        public override void VisitAtom(AtomSymbol atom)
        {
            Execute(atom, OnAtom, base.VisitAtom);
        }

        public Action<Compilation, Action<Compilation>> OnCompilation { get; set; }
        public override void VisitCompilation(Compilation compilation)
        {
            Execute(compilation, OnCompilation, base.VisitCompilation);
        }

        public Action<ComplexValueSymbol, Action<ComplexValueSymbol>> OnComplexValue { get; set; }
        public override void VisitComplexValue(ComplexValueSymbol value)
        {
            Execute(value, OnComplexValue, base.VisitComplexValue);
        }

        public Action<EntitySymbol, Action<EntitySymbol>> OnEntity { get; set; }
        public override void VisitEntity(EntitySymbol value)
        {
            Execute(value, OnEntity, base.VisitEntity);
        }

        public Action<EventSymbol, Action<EventSymbol>> OnEvent { get; set; }
        public override void VisitEvent(EventSymbol value)
        {
            Execute(value, OnEvent, base.VisitEvent);
        }

        public Action<NamespaceSymbol, Action<NamespaceSymbol>> OnNamespace { get; set; }
        public override void VisitNamespace(NamespaceSymbol value)
        {
            Execute(value, OnNamespace, base.VisitNamespace);
        }

        public Action<WebApiSymbol, Action<WebApiSymbol>> OnWebApi { get; set; }
        public override void VisitWebApi(WebApiSymbol value)
        {
            Execute(value, OnWebApi, base.VisitWebApi);
        }

        public Action<WebApiCommandSymbol, Action<WebApiCommandSymbol>> OnWebApiCommand { get; set; }
        public override void VisitWebApiCommand(WebApiCommandSymbol command)
        {
            Execute(command, OnWebApiCommand, base.VisitWebApiCommand);
        }

        public Action<WebApiPathSymbol, Action<WebApiPathSymbol>> OnWebApiPath { get; set; }
        public override void VisitWebApiPath(WebApiPathSymbol value)
        {
            Execute(value, OnWebApiPath, base.VisitWebApiPath);
        }

        public Action<WebApiQuerySymbol, Action<WebApiQuerySymbol>> OnWebApiQuery { get; set; }
        public override void VisitWebApiQuery(WebApiQuerySymbol value)
        {
            Execute(value, OnWebApiQuery, base.VisitWebApiQuery);
        }

        protected void Execute<T>(T subject, Action<T, Action<T>> operation, Action<T> fallback)
        {
            Guard.NotNull(fallback, "fallback");

            if (operation != null)
            {
                operation(subject, fallback);
                return;
            }

            fallback(subject);
        }

        protected TResult Execute<T, TResult>(T subject, Func<T, Func<T, TResult>, TResult> operation, Func<T, TResult> fallback)
        {
            Guard.NotNull(fallback, "fallback");

            return operation != null 
                ? operation(subject, fallback) 
                : fallback(subject);
        }
    }
}
