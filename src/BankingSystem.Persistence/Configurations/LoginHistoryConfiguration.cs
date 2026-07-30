using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingSystem.Persistence.Configurations;

public sealed class LoginHistoryConfiguration : IEntityTypeConfiguration<LoginHistory>
{
    public void Configure(EntityTypeBuilder<LoginHistory> builder)
    {
        builder.ToTable("LoginHistories");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Email).HasMaxLength(150);
        builder.Property(l => l.Machine).HasMaxLength(150);

        builder.HasIndex(l => l.UserId);
    }
}
