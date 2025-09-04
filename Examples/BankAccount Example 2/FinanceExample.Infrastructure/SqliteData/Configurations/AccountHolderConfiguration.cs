using FinanceExample.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceExample.Infrastructure.SqliteData.Configurations
{
    public class AccountHolderConfiguration : IEntityTypeConfiguration<AccountHolder>
    {
        public void Configure(EntityTypeBuilder<AccountHolder> builder)
        {
            builder.ToTable("AccountHolders");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .HasConversion(
                    id => id.Value,
                    value => AccountHolderId.From(value))
                .HasColumnName("Id");

            // Configure FullName as owned entity
            builder.OwnsOne(x => x.Name, name =>
            {
                name.Property(n => n.FirstName)
                    .HasColumnName("FirstName")
                    .HasMaxLength(50)
                    .IsRequired();

                name.Property(n => n.LastName)
                    .HasColumnName("LastName")
                    .HasMaxLength(50)
                    .IsRequired();
            });

            // Configure EmailAddress as owned entity
            builder.OwnsOne(x => x.EmailAddress, email =>
            {
                email.Property(e => e.Value)
                    .HasColumnName("EmailAddress")
                    .HasMaxLength(256)
                    .IsRequired();
            });

            builder.Property(x => x.HolderType)
                .HasConversion<string>()
                .HasColumnName("HolderType")
                .IsRequired();

            // Audit properties
            builder.Property(x => x.CreatedDateUtc)
                .HasColumnName("CreatedDateUtc")
                .IsRequired();

            builder.Property(x => x.LastModifiedDateUtc)
                .HasColumnName("LastModifiedDateUtc")
                .IsRequired();

        }
    }
}