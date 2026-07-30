using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Mapping;
using BankingSystem.Application.DTOs.Accounts;
using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Accounts;

public sealed class AccountService : IAccountService
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;

    public AccountService(IAccountRepository accounts, ITransactionRepository transactions)
    {
        _accounts = accounts;
        _transactions = transactions;
    }

    public async Task<List<AccountDto>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var accounts = await _accounts.GetByUserIdAsync(userId, ct);
        return accounts.Select(a => a.ToDto()).ToList();
    }

    public async Task<Result<AccountDto>> GetByIdAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await _accounts.GetByIdAsync(accountId, ct);
        return account is null
            ? Result.Failure<AccountDto>(DomainErrors.Accounts.NotFound)
            : Result.Success(account.ToDto());
    }

    public async Task<List<TransactionDto>> GetStatementAsync(Guid accountId, int take = 100, CancellationToken ct = default)
    {
        var transactions = await _transactions.GetByAccountIdAsync(accountId, take, ct);
        return transactions.Select(t => t.ToDto()).ToList();
    }
}
