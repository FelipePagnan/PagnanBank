using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingSystem.Persistence.Configurations;

public sealed class InvestmentConfiguration : IEntityTypeConfiguration<Investment>
{
    public void Configure(EntityTypeBuilder<Investment> builder)
    {
        builder.ToTable("Investments");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProductName).IsRequired().HasMaxLength(120);
        builder.Property(i => i.Principal).HasPrecision(18, 2);
        builder.Property(i => i.AnnualRatePercent).HasPrecision(9, 4);
        builder.Property(i => i.RedeemedAmount).HasPrecision(18, 2);
        builder.Property(i => i.Status).HasConversion<int>();

        builder.HasOne(i => i.Account)
            .WithMany()
            .HasForeignKey(i => i.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.AccountId);
    }
}
