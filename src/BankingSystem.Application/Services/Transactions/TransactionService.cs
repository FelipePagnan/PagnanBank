using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.Common.Mapping;
using BankingSystem.Application.DTOs.Transactions;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Common;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using FluentValidation;

namespace BankingSystem.Application.Services.Transactions;

public sealed class TransactionService : ITransactionService
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;
    private readonly IDateTimeProvider _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IValidator<DepositRequest> _depositValidator;
    private readonly IValidator<WithdrawRequest> _withdrawValidator;
    private readonly IValidator<TransferRequest> _transferValidator;

    public TransactionService(
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        IAuditService audit,
        IDateTimeProvider clock,
        ICurrentUser currentUser,
        IValidator<DepositRequest> depositValidator,
        IValidator<WithdrawRequest> withdrawValidator,
        IValidator<TransferRequest> transferValidator)
    {
        _accounts = accounts;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _clock = clock;
        _currentUser = currentUser;
        _depositValidator = depositValidator;
        _withdrawValidator = withdrawValidator;
        _transferValidator = transferValidator;
    }

    public async Task<Result<TransactionDto>> DepositAsync(DepositRequest request, CancellationToken ct = default)
    {
        var validation = await _depositValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<TransactionDto>(DomainErrors.Validation.Rule(Join(validation)));

        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure<TransactionDto>(DomainErrors.Accounts.NotFound);
        if (!account.IsActive)
            return Result.Failure<TransactionDto>(DomainErrors.Accounts.Blocked);

        account.Balance += request.Amount;
        account.UpdatedAtUtc = _clock.UtcNow;

        var transaction = BuildTransaction(account, TransactionType.Deposit, request.Amount, request.Description);
        _accounts.Update(account);
        await _transactions.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("Deposit", "Transactions", OperationResult.Success,
            $"Depósito de {request.Amount:C} na conta {account.Number}.", account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success(transaction.ToDto());
    }

    public async Task<Result<TransactionDto>> WithdrawAsync(WithdrawRequest request, CancellationToken ct = default)
    {
        var validation = await _withdrawValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<TransactionDto>(DomainErrors.Validation.Rule(Join(validation)));

        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure<TransactionDto>(DomainErrors.Accounts.NotFound);
        if (!account.IsActive)
            return Result.Failure<TransactionDto>(DomainErrors.Accounts.Blocked);
        if (account.Balance < request.Amount)
            return Result.Failure<TransactionDto>(DomainErrors.Accounts.InsufficientFunds);

        account.Balance -= request.Amount;
        account.UpdatedAtUtc = _clock.UtcNow;

        var transaction = BuildTransaction(account, TransactionType.Withdraw, request.Amount, request.Description);
        _accounts.Update(account);
        await _transactions.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("Withdraw", "Transactions", OperationResult.Success,
            $"Saque de {request.Amount:C} na conta {account.Number}.", account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success(transaction.ToDto());
    }

    public async Task<Result> TransferAsync(TransferRequest request, CancellationToken ct = default)
    {
        var validation = await _transferValidator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure(DomainErrors.Validation.Rule(Join(validation)));

        var source = await _accounts.GetByIdAsync(request.SourceAccountId, ct);
        if (source is null)
            return Result.Failure(DomainErrors.Accounts.NotFound);
        if (!source.IsActive)
            return Result.Failure(DomainErrors.Accounts.Blocked);

        var destination = await _accounts.GetByNumberAsync(request.DestinationAccountNumber.Trim(), ct);
        if (destination is null)
            return Result.Failure(DomainErrors.Accounts.DestinationNotFound);
        if (destination.Id == source.Id)
            return Result.Failure(DomainErrors.Accounts.SameAccount);
        if (!destination.IsActive)
            return Result.Failure(DomainErrors.Accounts.Blocked);
        if (source.Balance < request.Amount)
            return Result.Failure(DomainErrors.Accounts.InsufficientFunds);

        var outType = request.IsPix ? TransactionType.PixOut : TransactionType.TransferOut;
        var inType = request.IsPix ? TransactionType.PixIn : TransactionType.TransferIn;
        var label = request.IsPix ? "PIX" : "Transferência";
        var description = string.IsNullOrWhiteSpace(request.Description) ? label : request.Description;

        // Apply both sides then commit once => atomic within a single SaveChanges.
        source.Balance -= request.Amount;
        source.UpdatedAtUtc = _clock.UtcNow;
        destination.Balance += request.Amount;
        destination.UpdatedAtUtc = _clock.UtcNow;

        var outTx = BuildTransaction(source, outType, request.Amount, $"{description} para {destination.Number}");
        outTx.CounterpartAccountId = destination.Id;

        var inTx = BuildTransaction(destination, inType, request.Amount, $"{description} de {source.Number}");
        inTx.CounterpartAccountId = source.Id;

        _accounts.Update(source);
        _accounts.Update(destination);
        await _transactions.AddAsync(outTx, ct);
        await _transactions.AddAsync(inTx, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync(request.IsPix ? "Pix" : "Transfer", "Transactions", OperationResult.Success,
            $"{label} de {request.Amount:C} de {source.Number} para {destination.Number}.",
            source.UserId, source.User?.FullName ?? "", ct);

        return Result.Success();
    }

    public async Task<Result> AdminAdjustBalanceAsync(Guid accountId, decimal amount, bool credit, string reason, CancellationToken ct = default)
    {
        if (_currentUser.Role != UserRole.Administrator)
            return Result.Failure(DomainErrors.Auth.Forbidden);

        if (amount <= 0)
            return Result.Failure(DomainErrors.Transactions.InvalidAmount);

        var account = await _accounts.GetByIdAsync(accountId, ct);
        if (account is null)
            return Result.Failure(DomainErrors.Accounts.NotFound);

        if (credit)
        {
            account.Balance += amount;
        }
        else
        {
            if (account.Balance < amount)
                return Result.Failure(DomainErrors.Accounts.InsufficientFunds);
            account.Balance -= amount;
        }

        account.UpdatedAtUtc = _clock.UtcNow;

        var type = credit ? TransactionType.AdminCredit : TransactionType.AdminDebit;
        var description = string.IsNullOrWhiteSpace(reason) ? "Ajuste administrativo" : reason;
        var transaction = BuildTransaction(account, type, amount, description);

        _accounts.Update(account);
        await _transactions.AddAsync(transaction, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync(credit ? "AdminCredit" : "AdminDebit", "Administration", OperationResult.Success,
            $"{(credit ? "Crédito" : "Débito")} de {amount:C} na conta {account.Number}. Motivo: {description}",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success();
    }

    private Transaction BuildTransaction(Account account, TransactionType type, decimal amount, string description) => new()
    {
        AccountId = account.Id,
        Type = type,
        Amount = amount,
        BalanceAfter = account.Balance,
        Description = description,
        TimestampUtc = _clock.UtcNow
    };

    private static string Join(FluentValidation.Results.ValidationResult validation)
        => string.Join(" ", validation.Errors.Select(e => e.ErrorMessage));
}
