using System;

namespace Account.Domain.Framework.Layers.Domain.Entity
{
    public sealed class EntityId
    {
        public string Value { get; }

        public EntityId(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Entity id is required.", nameof(value));

            Value = value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
