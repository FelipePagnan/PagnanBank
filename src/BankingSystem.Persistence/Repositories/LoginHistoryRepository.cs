using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class LoginHistoryRepository : ILoginHistoryRepository
{
    private readonly BankingDbContext _context;

    public LoginHistoryRepository(BankingDbContext context) => _context = context;

    public async Task AddAsync(LoginHistory entry, CancellationToken ct = default)
        => await _context.LoginHistories.AddAsync(entry, ct);

    public Task<List<LoginHistory>> GetByUserIdAsync(Guid userId, int take = 50, CancellationToken ct = default)
        => _context.LoginHistories.AsNoTracking()
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.TimestampUtc)
            .Take(take)
            .ToListAsync(ct);

    public Task<List<LoginHistory>> GetRecentAsync(int take = 200, CancellationToken ct = default)
        => _context.LoginHistories.AsNoTracking()
            .OrderByDescending(l => l.TimestampUtc)
            .Take(take)
            .ToListAsync(ct);
}
