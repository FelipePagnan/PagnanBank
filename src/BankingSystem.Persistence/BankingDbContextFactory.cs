using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BankingSystem.Persistence;

/// <summary>
/// Design-time factory so 'dotnet ef migrations add ...' works with this
/// project as the target even though the DbContext is configured at runtime
/// by the Desktop composition root.
/// </summary>
public sealed class BankingDbContextFactory : IDesignTimeDbContextFactory<BankingDbContext>
{
    public BankingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<BankingDbContext>()
            .UseSqlite("Data Source=banking_designtime.db")
            .Options;

        return new BankingDbContext(options);
    }
}
