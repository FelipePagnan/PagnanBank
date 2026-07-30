using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingSystem.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Operation).IsRequired().HasMaxLength(80);
        builder.Property(a => a.Module).IsRequired().HasMaxLength(80);
        builder.Property(a => a.UserName).HasMaxLength(150);
        builder.Property(a => a.Details).HasMaxLength(1000);
        builder.Property(a => a.Result).HasConversion<int>();

        builder.HasIndex(a => a.TimestampUtc);
    }
}
