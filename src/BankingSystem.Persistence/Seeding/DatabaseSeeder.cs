using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Seeding;

/// <summary>
/// Ensures the database exists and seeds default users plus a product catalog
/// on first run so the application is usable immediately.
/// </summary>
public sealed class DatabaseSeeder
{
    private readonly BankingDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public DatabaseSeeder(BankingDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Create the schema directly from the model. Switch to MigrateAsync()
        // once migrations are generated (see README -> "Migrations").
        await _context.Database.EnsureCreatedAsync(ct);

        // Schema guard: if the database was created by an earlier version and is
        // missing tables added later, EnsureCreated does nothing. Detect that and
        // rebuild so the app keeps working without a manual delete.
        // (Acceptable for a demo/dev SQLite database.)
        if (!await NewTablesExistAsync(ct))
        {
            await _context.Database.EnsureDeletedAsync(ct);
            await _context.Database.EnsureCreatedAsync(ct);
        }

        await SeedProductsAsync(ct);
        await SeedUsersAsync(ct);
    }

    private async Task SeedUsersAsync(CancellationToken ct)
    {
        if (await _context.Users.AnyAsync(ct))
            return;

        var admin = new User
        {
            FullName = "Administrador do Sistema",
            Email = "admin@bank.local",
            Cpf = "00000000000",
            PasswordHash = _passwordHasher.Hash("Admin@123"),
            Role = UserRole.Administrator,
            Status = UserStatus.Active
        };

        var client = new User
        {
            FullName = "Cliente Demonstração",
            Email = "cliente@bank.local",
            Cpf = "11111111111",
            PasswordHash = _passwordHasher.Hash("Cliente@123"),
            Role = UserRole.Client,
            Status = UserStatus.Active
        };
        client.Accounts.Add(new Account
        {
            UserId = client.Id,
            Branch = "0001",
            Number = "10001-0",
            Type = AccountType.Checking,
            Status = AccountStatus.Active,
            Balance = 2500m,
            DailyLimit = 5000m
        });

        await _context.Users.AddRangeAsync(new[] { admin, client }, ct);
        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedProductsAsync(CancellationToken ct)
    {
        if (await _context.Products.AnyAsync(ct))
            return;

        var products = new[]
        {
            new Product { Name = "Fone Bluetooth", Category = "Eletrônicos", Description = "Fone sem fio com cancelamento de ruído.", Price = 299.90m, CashbackPercent = 3m },
            new Product { Name = "Teclado Mecânico", Category = "Eletrônicos", Description = "Teclado mecânico RGB switch marrom.", Price = 389.00m, CashbackPercent = 5m },
            new Product { Name = "Cadeira Gamer", Category = "Móveis", Description = "Cadeira ergonômica reclinável.", Price = 1249.00m, CashbackPercent = 2m },
            new Product { Name = "Cafeteira Espresso", Category = "Casa", Description = "Cafeteira automática 15 bar.", Price = 749.90m, CashbackPercent = 4m },
            new Product { Name = "Smartwatch", Category = "Eletrônicos", Description = "Relógio inteligente à prova d'água.", Price = 549.00m, CashbackPercent = 6m },
            new Product { Name = "Mochila Executiva", Category = "Acessórios", Description = "Mochila para notebook 15,6\".", Price = 219.90m, CashbackPercent = 3m },
            new Product { Name = "Monitor 27\" 4K", Category = "Eletrônicos", Description = "Monitor IPS 4K 60Hz.", Price = 1899.00m, CashbackPercent = 2.5m },
            new Product { Name = "Luminária de Mesa", Category = "Casa", Description = "Luminária LED com regulagem.", Price = 129.90m, CashbackPercent = 5m }
        };

        await _context.Products.AddRangeAsync(products, ct);
        await _context.SaveChangesAsync(ct);
    }

    /// <summary>Returns false when a table introduced in a later version is missing.</summary>
    private async Task<bool> NewTablesExistAsync(CancellationToken ct)
    {
        try
        {
            await _context.Investments.AnyAsync(ct);
            await _context.Loans.AnyAsync(ct);
            await _context.Cards.AnyAsync(ct);
            await _context.Products.AnyAsync(ct);
            await _context.Orders.AnyAsync(ct);
            await _context.OrderItems.AnyAsync(ct);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
