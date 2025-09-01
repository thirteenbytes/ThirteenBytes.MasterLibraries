namespace ThirteenBytes.DDDPatterns.Primitives.Abstractions
{
    public interface IAuditEntity
    {
        public DateTime CreatedDateUtc { get; set;  }
        public DateTime LastModifiedDateUtc { get; set; }
    }
}
