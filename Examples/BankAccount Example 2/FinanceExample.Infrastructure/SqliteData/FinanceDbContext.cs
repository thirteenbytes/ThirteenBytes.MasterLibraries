using FinanceExample.Infrastructure.SqliteData.Configurations;
using Microsoft.EntityFrameworkCore;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Clock;
using ThirteenBytes.DDDPatterns.Primitives.Abstractions.Data;

namespace FinanceExample.Infrastructure.SqliteData
{
    public class FinanceDbContext : DbContext, IUnitOfWork
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public FinanceDbContext(DbContextOptions<FinanceDbContext> options, IDateTimeProvider dateTimeProvider)
            : base(options)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BankAccountConfiguration());
            modelBuilder.ApplyConfiguration(new SupportedCurrencyConfiguration());
            modelBuilder.ApplyConfiguration(new AccountHolderConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<IAuditEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            var now = _dateTimeProvider.UtcNow;

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedDateUtc = now;
                    entry.Entity.LastModifiedDateUtc = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.LastModifiedDateUtc = now;
                    entry.Property(e => e.CreatedDateUtc).IsModified = false;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}