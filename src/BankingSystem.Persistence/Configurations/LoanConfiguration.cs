using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingSystem.Persistence.Configurations;

public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Principal).HasPrecision(18, 2);
        builder.Property(l => l.AnnualRatePercent).HasPrecision(9, 4);
        builder.Property(l => l.InstallmentAmount).HasPrecision(18, 2);
        builder.Property(l => l.TotalAmount).HasPrecision(18, 2);
        builder.Property(l => l.Status).HasConversion<int>();

        // Outstanding is a computed, get-only property -> ignored by EF convention.

        builder.HasOne(l => l.Account)
            .WithMany()
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.AccountId);
    }
}
