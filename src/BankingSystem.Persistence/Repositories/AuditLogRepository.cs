using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankingSystem.Persistence.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly BankingDbContext _context;

    public AuditLogRepository(BankingDbContext context) => _context = context;

    public async Task AddAsync(AuditLog log, CancellationToken ct = default)
        => await _context.AuditLogs.AddAsync(log, ct);

    public Task<List<AuditLog>> GetRecentAsync(int take = 200, CancellationToken ct = default)
        => _context.AuditLogs.AsNoTracking()
            .OrderByDescending(a => a.TimestampUtc)
            .Take(take)
            .ToListAsync(ct);
}
