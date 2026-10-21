using System;

namespace Account.Domain.Framework.Layers.Domain.Entity
{
    public abstract class Entity<T> where T : Entity<T>
    {
        public EntityId Id { get; set; }
        public bool IsActive { get; private set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static T New()
        {
            var entity = Activator.CreateInstance(typeof(T)) as T;
            entity.Id = new EntityId(Guid.NewGuid().ToString());
            entity.IsActive = true;
            entity.CreateDate = DateTime.UtcNow;
            return entity;
        }
    }
}
