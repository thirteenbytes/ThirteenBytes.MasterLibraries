namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    /// <summary>
    /// Base class for audited entities that support soft delete.
    /// Inheritance: Entity ? AuditEntity ? DeletableAuditEntity
    /// </summary>
    /// <typeparam name="TId">The type of the entity's identifier.</typeparam>
    public abstract class DeletableAuditEntity<TId> : AuditEntity<TId>, IDeletable
        where TId : notnull
    {
        /// <summary>
        /// Gets the UTC timestamp when the entity was soft deleted.
        /// Null = active, non-null = deleted.
        /// </summary>
        public DateTime? DeletedAtUtc { get; private set; }

        /// <summary>
        /// Marks the entity as soft deleted. Virtual for custom deletion logic.
        /// </summary>
        /// <param name="deletedAtUtc">The UTC timestamp when deletion occurred.</param>
        public virtual void MarkAsDeleted(DateTime deletedAtUtc)
        {
            DeletedAtUtc = deletedAtUtc;
        }

        /// <summary>
        /// Restores entity to active state. Virtual for custom restoration logic.
        /// </summary>
        public virtual void Restore()
        {
            DeletedAtUtc = null;
        }

        /// <summary>
        /// Parameterless constructor for Entity Framework Core materialization.
        /// Should not be used directly in domain code.
        /// </summary>
        protected DeletableAuditEntity() { }

        /// <summary>
        /// Domain constructor that creates a deletable audited entity with the specified identifier.
        /// Audit timestamps and deletion state will be managed by the domain and persistence layer.
        /// </summary>
        /// <param name="id">The identifier for the entity. Cannot be null.</param>
        protected DeletableAuditEntity(TId id) : base(id) { }
    }
}
