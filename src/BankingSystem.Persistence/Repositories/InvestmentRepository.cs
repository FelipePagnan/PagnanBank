using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class InvestmentRepository : IInvestmentRepository
{
    private readonly BankingDbContext _context;

    public InvestmentRepository(BankingDbContext context) => _context = context;

    public Task<Investment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _context.Investments.Include(i => i.Account).FirstOrDefaultAsync(i => i.Id == id, ct);

    public Task<List<Investment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _context.Investments
            .Include(i => i.Account)
            .Where(i => i.Account!.UserId == userId)
            .OrderByDescending(i => i.StartDateUtc)
            .ToListAsync(ct);

    public async Task AddAsync(Investment investment, CancellationToken ct = default)
        => await _context.Investments.AddAsync(investment, ct);

    public void Update(Investment investment) => _context.Investments.Update(investment);
}
