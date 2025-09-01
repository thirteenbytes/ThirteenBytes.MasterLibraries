using System;

namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base audited entity using strongly-typed IDs.
    /// Timestamps are managed by the persistence layer (e.g., InMemoryDatabase).
    /// </summary>
    public abstract class AuditEntity<TId, TValue> : Entity<TId, TValue>, IAuditEntity
        where TId : IEntityId<TId, TValue>
        where TValue : notnull, IEquatable<TValue>
    {
        public DateTime CreatedDateUtc { get; set; }
        public DateTime LastModifiedDateUtc { get; set; }

        // EF Core needs this
        protected AuditEntity() { }

        // Domain ctor (lets you pass an ID)
        protected AuditEntity(TId id) : base(id) { }
    }
}