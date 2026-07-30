using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.DTOs.Loans;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Common;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using FluentValidation;

namespace BankingSystem.Application.Services.Loans;

public sealed class LoanService : ILoanService
{
    private readonly ILoanRepository _loans;
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;
    private readonly IDateTimeProvider _clock;
    private readonly IValidator<CreateLoanRequest> _validator;

    public LoanService(
        ILoanRepository loans,
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        IAuditService audit,
        IDateTimeProvider clock,
        IValidator<CreateLoanRequest> validator)
    {
        _loans = loans;
        _accounts = accounts;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _clock = clock;
        _validator = validator;
    }

    public async Task<Result<LoanDto>> ContractAsync(CreateLoanRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<LoanDto>(
                DomainErrors.Validation.Rule(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));

        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure<LoanDto>(DomainErrors.Accounts.NotFound);
        if (!account.IsActive)
            return Result.Failure<LoanDto>(DomainErrors.Accounts.Blocked);

        var now = _clock.UtcNow;
        var installment = FinancialCalculator.MonthlyInstallment(request.Principal, request.AnnualRatePercent, request.Installments);
        var total = FinancialCalculator.LoanTotal(request.Principal, request.AnnualRatePercent, request.Installments);

        account.Balance += request.Principal;
        account.UpdatedAtUtc = now;

        var loan = new Loan
        {
            AccountId = account.Id,
            Principal = request.Principal,
            AnnualRatePercent = request.AnnualRatePercent,
            Installments = request.Installments,
            PaidInstallments = 0,
            InstallmentAmount = installment,
            TotalAmount = total,
            ContractedAtUtc = now,
            Status = LoanStatus.Active
        };

        _accounts.Update(account);
        await _loans.AddAsync(loan, ct);
        await _transactions.AddAsync(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.LoanCredit,
            Amount = request.Principal,
            BalanceAfter = account.Balance,
            Description = $"Empréstimo em {request.Installments}x de {installment:C}",
            TimestampUtc = now
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("ContractLoan", "FinancialProducts", OperationResult.Success,
            $"Empréstimo de {request.Principal:C} em {request.Installments}x.",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success(ToDto(loan));
    }

    public async Task<Result> PayInstallmentAsync(Guid loanId, CancellationToken ct = default)
    {
        var loan = await _loans.GetByIdAsync(loanId, ct);
        if (loan is null)
            return Result.Failure(DomainErrors.Loans.NotFound);
        if (loan.Status == LoanStatus.Settled)
            return Result.Failure(DomainErrors.Loans.AlreadySettled);

        var account = await _accounts.GetByIdAsync(loan.AccountId, ct);
        if (account is null)
            return Result.Failure(DomainErrors.Accounts.NotFound);
        if (!account.IsActive)
            return Result.Failure(DomainErrors.Accounts.Blocked);
        if (account.Balance < loan.InstallmentAmount)
            return Result.Failure(DomainErrors.Accounts.InsufficientFunds);

        var now = _clock.UtcNow;

        account.Balance -= loan.InstallmentAmount;
        account.UpdatedAtUtc = now;

        loan.PaidInstallments++;
        if (loan.PaidInstallments >= loan.Installments)
            loan.Status = LoanStatus.Settled;
        loan.UpdatedAtUtc = now;

        _accounts.Update(account);
        _loans.Update(loan);
        await _transactions.AddAsync(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.LoanPayment,
            Amount = loan.InstallmentAmount,
            BalanceAfter = account.Balance,
            Description = $"Parcela {loan.PaidInstallments}/{loan.Installments} do empréstimo",
            TimestampUtc = now
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("PayLoan", "FinancialProducts", OperationResult.Success,
            $"Pagamento de parcela ({loan.PaidInstallments}/{loan.Installments}) de {loan.InstallmentAmount:C}.",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success();
    }

    public async Task<List<LoanDto>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var loans = await _loans.GetByUserIdAsync(userId, ct);
        return loans.Select(ToDto).ToList();
    }

    public LoanSimulationResult Simulate(LoanSimulationRequest request)
    {
        var installment = FinancialCalculator.MonthlyInstallment(request.Principal, request.AnnualRatePercent, request.Installments);
        var total = FinancialCalculator.LoanTotal(request.Principal, request.AnnualRatePercent, request.Installments);
        return new LoanSimulationResult
        {
            Principal = request.Principal,
            InstallmentAmount = installment,
            Total = total,
            TotalInterest = decimal.Round(total - request.Principal, 2)
        };
    }

    private static LoanDto ToDto(Loan l) => new()
    {
        Id = l.Id,
        Principal = l.Principal,
        AnnualRatePercent = l.AnnualRatePercent,
        Installments = l.Installments,
        PaidInstallments = l.PaidInstallments,
        InstallmentAmount = l.InstallmentAmount,
        TotalAmount = l.TotalAmount,
        Outstanding = l.Outstanding,
        ContractedAtUtc = l.ContractedAtUtc,
        StatusLabel = l.Status == LoanStatus.Active ? "Ativo" : "Quitado",
        IsActive = l.Status == LoanStatus.Active
    };
}
