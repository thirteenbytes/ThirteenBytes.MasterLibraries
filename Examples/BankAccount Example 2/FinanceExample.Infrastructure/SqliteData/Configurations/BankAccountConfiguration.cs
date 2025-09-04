using FinanceExample.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceExample.Infrastructure.SqliteData.Configurations
{
    public class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
    {
        public void Configure(EntityTypeBuilder<BankAccount> builder)
        {
            builder.ToTable("BankAccounts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => BankAccountId.From(value))
                .HasColumnName("Id");

            builder.Property(x => x.AccountHolderId)
                .HasConversion(
                    id => id.Value,
                    value => AccountHolderId.From(value))
                .HasColumnName("AccountHolderId")
                .IsRequired();

            // Configure Money as owned entity
            builder.OwnsOne(x => x.Balance, money =>
            {
                money.Property(m => m.Amount)
                    .HasColumnName("BalanceAmount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                money.Property(m => m.Currency)
                    .HasColumnName("BalanceCurrency")
                    .HasMaxLength(3)
                    .IsRequired();
            });

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasColumnName("Status")
                .IsRequired();

            // Audit properties
            builder.Property(x => x.CreatedDateUtc)
                .HasColumnName("CreatedDateUtc")
                .IsRequired();

            builder.Property(x => x.LastModifiedDateUtc)
                .HasColumnName("LastModifiedDateUtc")
                .IsRequired();

            // Ignore domain events for persistence
            builder.Ignore(x => x.DomainEvents);
        }
    }
}