using BankingSystem.Domain.Entities;

namespace BankingSystem.Application.Abstractions.Repositories;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Loan>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Loan loan, CancellationToken ct = default);
    void Update(Loan loan);
}
