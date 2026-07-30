using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface ILoginHistoryRepository
{
    Task AddAsync(LoginHistory entry, CancellationToken ct = default);
    Task<List<LoginHistory>> GetByUserIdAsync(Guid userId, int take = 50, CancellationToken ct = default);
    Task<List<LoginHistory>> GetRecentAsync(int take = 200, CancellationToken ct = default);
}
