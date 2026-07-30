using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Account?> GetByNumberAsync(string number, CancellationToken ct = default);
    Task<List<Account>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Account account, CancellationToken ct = default);
    void Update(Account account);
    Task<bool> NumberExistsAsync(string number, CancellationToken ct = default);
}
