using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingSystem.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Branch).IsRequired().HasMaxLength(10);
        builder.Property(a => a.Number).IsRequired().HasMaxLength(20);
        builder.Property(a => a.Type).HasConversion<int>();
        builder.Property(a => a.Status).HasConversion<int>();
        builder.Property(a => a.Balance).HasPrecision(18, 2);
        builder.Property(a => a.DailyLimit).HasPrecision(18, 2);

        builder.HasIndex(a => a.Number).IsUnique();

        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.Account!)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
