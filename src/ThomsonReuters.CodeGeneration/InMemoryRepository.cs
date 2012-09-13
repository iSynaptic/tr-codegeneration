using System;
using System.Collections.Generic;

namespace ThomsonReuters.CodeGeneration
{
    public class InMemoryRepository<TEntity, TIdentity> : Repository<TEntity, TIdentity>
        where TEntity : Entity<TIdentity>
        where TIdentity : IEquatable<TIdentity>
    {
        private List<Entity<TIdentity>.Event> _Events = null;

        public override IEnumerable<Entity<TIdentity>.Event> GetEventStream(TIdentity id)
        {
            return Events;
        }

        protected override void SaveEventStream(TIdentity id, IEnumerable<Entity<TIdentity>.Event> events)
        {
            Events.AddRange(events);
        }

        public List<Entity<TIdentity>.Event> Events
        {
            get { return _Events ?? (_Events = new List<Entity<TIdentity>.Event>()); }
        }
    }
}