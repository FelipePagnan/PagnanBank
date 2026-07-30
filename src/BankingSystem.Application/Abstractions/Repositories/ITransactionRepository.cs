using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken ct = default);
    Task<List<Transaction>> GetByAccountIdAsync(Guid accountId, int take = 100, CancellationToken ct = default);
}
