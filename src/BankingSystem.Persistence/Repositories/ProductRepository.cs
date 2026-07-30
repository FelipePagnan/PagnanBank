using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly BankingDbContext _context;

    public ProductRepository(BankingDbContext context) => _context = context;

    public Task<List<Product>> GetActiveAsync(CancellationToken ct = default)
        => _context.Products.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category).ThenBy(p => p.Name)
            .ToListAsync(ct);

    public Task<List<Product>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var set = ids.ToList();
        return _context.Products.Where(p => set.Contains(p.Id)).ToListAsync(ct);
    }
}
