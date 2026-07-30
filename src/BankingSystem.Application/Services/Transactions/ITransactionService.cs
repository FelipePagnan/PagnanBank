using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Domain.Common;

namespace BankingSystem.Application.Services.Transactions;

public interface ITransactionService
{
    Task<Result<TransactionDto>> DepositAsync(DepositRequest request, CancellationToken ct = default);
    Task<Result<TransactionDto>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default);
    Task<Result> TransferAsync(TransferRequest request, CancellationToken ct = default);

    /// <summary>
    /// Administrative credit/debit of test balance on an account.
    /// </summary>
    Task<Result> AdminAdjustBalanceAsync(Guid accountId, decimal amount, bool credit, string reason, CancellationToken ct = default);
}
