using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Transactions;
using iSynaptic.Commons;

namespace ThomsonReuters.CodeGeneration
{
    public abstract class ProjectionRepository<TEntity, TIdentity, TDataSource> : Repository<TEntity, TIdentity>
        where TEntity : Entity<TIdentity>
        where TIdentity : IEquatable<TIdentity>
        where TDataSource : IDisposable
    {
        private readonly List<Action<Entity<TIdentity>.Event, TDataSource>> _EventHandlers = null;

        protected ProjectionRepository(Func<TDataSource> dataSourceFactory)
        {
            DataSourceFactory = Guard.NotNull(dataSourceFactory, "dataSourceFactory");
            _EventHandlers = GetEventHandlers().ToList();
        }

        private IEnumerable<Action<Entity<TIdentity>.Event, TDataSource>> GetEventHandlers()
        {
            var domainEventType = typeof(Entity<TIdentity>.Event);

            return GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.ReturnType == typeof (void) && x.Name.StartsWith("Handle"))
                .Select(x => new {Method = x, Parameters = x.GetParameters()})
                .Where(x => x.Parameters != null &&
                            x.Parameters.Length == 2 &&
                            domainEventType.IsAssignableFrom(x.Parameters[0].ParameterType) &&
                            typeof(TDataSource).IsAssignableFrom(x.Parameters[1].ParameterType))
                .Select(x => BuildEventHandler(x.Parameters[0].ParameterType, x.Method));
        }

        private Action<Entity<TIdentity>.Event, TDataSource> BuildEventHandler(Type eventType, MethodInfo method)
        {
            var @event = Expression.Parameter(typeof(Entity<TIdentity>.Event));
            var context = Expression.Parameter(typeof(TDataSource));

            var call = Expression.IfThen(Expression.TypeIs(@event, eventType),
                                         Expression.Call(Expression.Constant(this), method,
                                                         Expression.Convert(@event, eventType),
                                                         context));

            return Expression.Lambda<Action<Entity<TIdentity>.Event, TDataSource>>(call, @event, context)
                .Compile();
        }

        protected override void SaveEventStream(TIdentity id, IEnumerable<Entity<TIdentity>.Event> events)
        {
            using(var ts = new TransactionScope())
            using (var ds = DataSourceFactory())
            {
                foreach (var @event in events)
                    OnHandleEvent(@event, ds);

                ts.Complete();
            }
        }

        protected virtual void OnHandleEvent(Entity<TIdentity>.Event @event, TDataSource ds)
        {
            foreach (var eventHandler in _EventHandlers)
                eventHandler(@event, ds);
        }

        protected Func<TDataSource> DataSourceFactory { get; set; }
    }
}
