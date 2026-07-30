using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Accounts;

public interface IAccountService
{
    Task<List<AccountDto>> GetByUserAsync(Guid userId, CancellationToken ct = default);
    Task<Result<AccountDto>> GetByIdAsync(Guid accountId, CancellationToken ct = default);
    Task<List<TransactionDto>> GetStatementAsync(Guid accountId, int take = 100, CancellationToken ct = default);
}
