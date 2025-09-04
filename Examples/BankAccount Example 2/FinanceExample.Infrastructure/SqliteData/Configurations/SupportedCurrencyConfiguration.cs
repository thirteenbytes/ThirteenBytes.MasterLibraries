using FinanceExample.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceExample.Infrastructure.SqliteData.Configurations
{
    public class SupportedCurrencyConfiguration : IEntityTypeConfiguration<SupportedCurrency>
    {
        public void Configure(EntityTypeBuilder<SupportedCurrency> builder)
        {
            builder.ToTable("SupportedCurrencies");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => SupportedCurrencyId.From(value))
                .HasColumnName("Id");

            builder.Property(x => x.CurrencyCode)
                .HasColumnName("CurrencyCode")
                .HasMaxLength(3)
                .IsRequired();

            builder.Property(x => x.CurrencyName)
                .HasColumnName("CurrencyName")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.IsActive)
                .HasColumnName("IsActive")
                .IsRequired();

            // Audit properties
            builder.Property(x => x.CreatedDateUtc)
                .HasColumnName("CreatedDateUtc")
                .IsRequired();

            builder.Property(x => x.LastModifiedDateUtc)
                .HasColumnName("LastModifiedDateUtc")
                .IsRequired();

            // Index on currency code for fast lookups
            builder.HasIndex(x => x.CurrencyCode)
                .IsUnique()
                .HasDatabaseName("IX_SupportedCurrencies_CurrencyCode");
        }
    }
}