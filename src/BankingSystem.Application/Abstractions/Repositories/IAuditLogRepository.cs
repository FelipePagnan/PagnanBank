using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog log, CancellationToken ct = default);
    Task<List<AuditLog>> GetRecentAsync(int take = 200, CancellationToken ct = default);
}
