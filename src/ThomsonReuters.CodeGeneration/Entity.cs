using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using iSynaptic.Commons;
using iSynaptic.Commons.Linq;
using iSynaptic.Commons.Collections.Generic;

namespace ThomsonReuters.CodeGeneration
{
    public abstract class Entity<TIdentity>
        where TIdentity : IEquatable<TIdentity>
    {
        private static readonly Action<Event, Event> _EventParentSetter = null;

        static Entity()
        {
            var childEvent = Expression.Parameter(typeof (Event));
            var parentEvent = Expression.Parameter(typeof (Event));

            _EventParentSetter = 
                Expression.Lambda<Action<Event, Event>>(
                        Expression.Assign(
                            Expression.Field(childEvent, "_EventParent"),
                            parentEvent),
                        childEvent,
                        parentEvent).Compile();
        }

        public class Event
        {
            public static readonly Event Null = new Event();
            private Event _EventParent = null;

            private Event()
            {
                _EventParent = this;
            }

            protected Event(TIdentity entityId, int entityVersion = 1, DateTime? eventRecordedAt = null)
            {
                EventId = Guid.NewGuid();
                EventRecordedAt = eventRecordedAt ?? SystemClock.UtcNow;
                EntityId = entityId;
                EntityVersion = entityVersion;
                _EventParent = Null;
            }

            protected Event(Event eventParent, DateTime? eventRecordedAt = null)
            {
                Guard.NotNull(eventParent, "eventParent");

                if(eventParent == Null)
                    throw new ArgumentException("You cannot use Null as the event parent.");

                EventId = Guid.NewGuid();
                EventRecordedAt = eventRecordedAt ?? SystemClock.UtcNow;
                EntityId = eventParent.EntityId;
                EntityVersion = eventParent.EntityVersion + 1;
                _EventParent = eventParent;
            }

            public Guid EventId { get; private set; }
            public DateTime EventRecordedAt { get; private set; }
            public Event EventParent { get { return _EventParent ?? Null; } }

            public TIdentity EntityId { get; private set; }
            public int EntityVersion { get; private set; }
        }

        #region Event Applicator Initialization

        private static readonly Dictionary<Type, Dictionary<Type, Action<object, Event>>> _EventApplicators =
            new Dictionary<Type, Dictionary<Type, Action<object, Event>>>();

        private static Dictionary<Type, Action<object, Event>> GetEventApplicators( Type entityType )
        {
            Guard.NotNull(entityType, "entityType");

            lock( _EventApplicators )
            {
                if( _EventApplicators.ContainsKey( entityType ) )
                    return _EventApplicators[entityType];

                var domainEventType = typeof( Event );
                var result = entityType.GetMethods( BindingFlags.NonPublic | BindingFlags.Instance )
                    .Where( x => x.ReturnType == typeof( void ) && x.Name == "On" )
                    .Select( x => new { Method = x, Parameters = x.GetParameters() } )
                    .Where( x => x.Parameters != null && x.Parameters.Length == 1 && domainEventType.IsAssignableFrom( x.Parameters[0].ParameterType ) )
                    .Select( x => KeyValuePair.Create( x.Parameters[0].ParameterType, BuildEventApplicator( entityType, x.Parameters[0].ParameterType, x.Method ) ) )
                    .ToDictionary();

                _EventApplicators.Add(entityType, result);

                return result;
            }
        }

        private static Action<object, Event> BuildEventApplicator(Type entityType, Type eventType, MethodInfo method)
        {
            var @entity = Expression.Parameter(typeof(object));
            var domainEvent = Expression.Parameter(typeof(Event));

            var call = Expression.Call(Expression.Convert(@entity, entityType), method,
                                       Expression.Convert(domainEvent, eventType));

            return Expression.Lambda<Action<object, Event>>(call, @entity, domainEvent)
                .Compile();
        }

        #endregion

        private Event _LastCommittedEvent = Event.Null;
        private Event _LastUncommittedEvent = Event.Null;


        public IEnumerable<Event> GetEvents()
        {
            if (GetMostRecentEvent() == null)
                return Enumerable.Empty<Event>();

            return GetMostRecentEvent()
                .Recurse(x => x.EventParent != Event.Null
                                  ? new Maybe<Event>(x.EventParent)
                                  : Maybe<Event>.NoValue)
                .Reverse();
                
        }

        public IEnumerable<Event> GetUncommittedEvents()
        {
            return GetEvents()
                .SkipWhile(x => x.EventParent != _LastCommittedEvent);
        }

        protected virtual void PrepareToLoadFromEventStream() { }

        internal void LoadFromEventStream(IEnumerable<Event> events)
        {
            PrepareToLoadFromEventStream();

            _LastUncommittedEvent = null;
            _LastCommittedEvent = null;

            foreach(var @event in events)
                ApplyEvent(@event, false);
        }

        protected void ApplyNewEvent(Func<Event, Event> eventFactory)
        {
            Guard.NotNull(eventFactory, "eventFactory");
            
            var mostRecentEvent = GetMostRecentEvent();
            var newEvent = eventFactory(mostRecentEvent);

            if(ReferenceEquals(newEvent.EventParent, mostRecentEvent) != true)
                throw new ArgumentException("The event factory did not use the provided entity as the new entities parent.", "entityFactory");

            ApplyEvent(newEvent, true);
        }

        private void ApplyEvent(Event @event, bool isNew)
        {
            Guard.NotNull(@event, "@event");

            var eventType = @event.GetType();

            GetType()
                .Recurse(x => x.BaseType)
                .Select( GetEventApplicators )
                .Select(x => x.TryGetValue(eventType))
                .FirstOrDefault(x => x.HasValue)
                .Run(applicator => applicator(this, @event));

            if (!isNew)
            {
                _EventParentSetter(@event, GetMostRecentEvent());
                _LastCommittedEvent = @event;
            }
            else
                _LastUncommittedEvent = @event;
        }

        internal void CommitEvents()
        {
            _LastCommittedEvent = _LastUncommittedEvent;
            _LastUncommittedEvent = null;
        }

        protected Event GetMostRecentEvent()
        {
            return _LastUncommittedEvent ?? _LastCommittedEvent;
        }

        public TIdentity Id { get { return GetMostRecentEvent().EntityId; } }
        public int EntityVersion { get { return GetMostRecentEvent().EntityVersion; } }
    }
}
