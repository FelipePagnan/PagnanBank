using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface IInvestmentRepository
{
    Task<Investment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Investment>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Investment investment, CancellationToken ct = default);
    void Update(Investment investment);
}
