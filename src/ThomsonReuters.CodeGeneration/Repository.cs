using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using iSynaptic.Commons;

namespace ThomsonReuters.CodeGeneration
{
    public static class Repository
    {
        public static TEntity Get<TEntity, TIdentity>(TIdentity id)
            where TEntity : Entity<TIdentity>
            where TIdentity : IEquatable<TIdentity>
        {
            return GetRepository<TEntity, TIdentity>()
                .Get(id);
        }

        public static void Save<TEntity, TIdentity>(TEntity entity)
            where TEntity : Entity<TIdentity>
            where TIdentity : IEquatable<TIdentity>
        {
            GetRepository<TEntity, TIdentity>()
                .Save(entity);
        }

        private static Repository<TEntity, TIdentity> GetRepository<TEntity, TIdentity>()
            where TEntity : Entity<TIdentity>
            where TIdentity : IEquatable<TIdentity>
        {
            return Ioc.TryResolve<Repository<TEntity, TIdentity>>()
                .ThrowOnNoValue(new InvalidOperationException(string.Format("Unable to find repository for entity type '{0}'.", typeof(TEntity).Name)))
                .ValueOrDefault();
        }
    }

    public abstract class Repository<TEntity, TIdentity>
        where TEntity : Entity<TIdentity>
        where TIdentity : IEquatable<TIdentity>
    {
        public virtual TEntity Get(TIdentity id)
        {
            var events = GetEventStream(id)
                .ToArray();

            if (events.Length <= 0)
                return null;

            var entityType = events[0].GetType().DeclaringType;
            if (typeof(TEntity).IsAssignableFrom(entityType) != true)
                throw new InvalidOperationException(string.Format("Event stream is for a {0} entity.", entityType.Name));

            var entity = (TEntity)FormatterServices.GetSafeUninitializedObject(entityType);
            entity.LoadFromEventStream(events);

            return entity;
        }

        public virtual void Save(TEntity entity)
        {
            Guard.NotNull(entity, "entity");

            SaveEventStream(entity.Id, entity.GetUncommittedEvents());
            entity.CommitEvents();
        }

        public abstract IEnumerable<Entity<TIdentity>.Event> GetEventStream(TIdentity id);
        protected abstract void SaveEventStream(TIdentity id, IEnumerable<Entity<TIdentity>.Event> events);
    }
}
