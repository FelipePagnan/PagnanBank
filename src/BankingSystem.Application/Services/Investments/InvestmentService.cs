using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.DTOs.Investments;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Common;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using FluentValidation;

namespace BankingSystem.Application.Services.Investments;

public sealed class InvestmentService : IInvestmentService
{
    private readonly IInvestmentRepository _investments;
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;
    private readonly IDateTimeProvider _clock;
    private readonly IValidator<CreateInvestmentRequest> _validator;

    public InvestmentService(
        IInvestmentRepository investments,
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        IAuditService audit,
        IDateTimeProvider clock,
        IValidator<CreateInvestmentRequest> validator)
    {
        _investments = investments;
        _accounts = accounts;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _clock = clock;
        _validator = validator;
    }

    public async Task<Result<InvestmentDto>> InvestAsync(CreateInvestmentRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<InvestmentDto>(
                DomainErrors.Validation.Rule(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));

        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure<InvestmentDto>(DomainErrors.Accounts.NotFound);
        if (!account.IsActive)
            return Result.Failure<InvestmentDto>(DomainErrors.Accounts.Blocked);
        if (account.Balance < request.Principal)
            return Result.Failure<InvestmentDto>(DomainErrors.Accounts.InsufficientFunds);

        var now = _clock.UtcNow;

        account.Balance -= request.Principal;
        account.UpdatedAtUtc = now;

        var investment = new Investment
        {
            AccountId = account.Id,
            ProductName = string.IsNullOrWhiteSpace(request.ProductName) ? "Investimento" : request.ProductName.Trim(),
            Principal = request.Principal,
            AnnualRatePercent = request.AnnualRatePercent,
            StartDateUtc = now,
            Status = InvestmentStatus.Active
        };

        _accounts.Update(account);
        await _investments.AddAsync(investment, ct);
        await _transactions.AddAsync(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.InvestmentBuy,
            Amount = request.Principal,
            BalanceAfter = account.Balance,
            Description = $"Aplicação em {investment.ProductName}",
            TimestampUtc = now
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("Invest", "FinancialProducts", OperationResult.Success,
            $"Aplicação de {request.Principal:C} em {investment.ProductName}.",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success(ToDto(investment, now));
    }

    public async Task<Result> RedeemAsync(Guid investmentId, CancellationToken ct = default)
    {
        var investment = await _investments.GetByIdAsync(investmentId, ct);
        if (investment is null)
            return Result.Failure(DomainErrors.Investments.NotFound);
        if (investment.Status == InvestmentStatus.Redeemed)
            return Result.Failure(DomainErrors.Investments.AlreadyRedeemed);

        var account = await _accounts.GetByIdAsync(investment.AccountId, ct);
        if (account is null)
            return Result.Failure(DomainErrors.Accounts.NotFound);

        var now = _clock.UtcNow;
        var months = FinancialCalculator.MonthsBetween(investment.StartDateUtc, now);
        var value = FinancialCalculator.FutureValue(investment.Principal, investment.AnnualRatePercent, months);

        account.Balance += value;
        account.UpdatedAtUtc = now;

        investment.Status = InvestmentStatus.Redeemed;
        investment.RedeemedAtUtc = now;
        investment.RedeemedAmount = value;
        investment.UpdatedAtUtc = now;

        _accounts.Update(account);
        _investments.Update(investment);
        await _transactions.AddAsync(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.InvestmentRedeem,
            Amount = value,
            BalanceAfter = account.Balance,
            Description = $"Resgate de {investment.ProductName}",
            TimestampUtc = now
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("Redeem", "FinancialProducts", OperationResult.Success,
            $"Resgate de {value:C} ({investment.ProductName}).",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success();
    }

    public async Task<List<InvestmentDto>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var now = _clock.UtcNow;
        var investments = await _investments.GetByUserIdAsync(userId, ct);
        return investments.Select(i => ToDto(i, now)).ToList();
    }

    public InvestmentSimulationResult Simulate(InvestmentSimulationRequest request)
    {
        var future = FinancialCalculator.FutureValue(request.Principal, request.AnnualRatePercent, request.Months);
        return new InvestmentSimulationResult
        {
            Principal = request.Principal,
            FutureValue = future,
            Yield = decimal.Round(future - request.Principal, 2)
        };
    }

    private static InvestmentDto ToDto(Investment i, DateTime now)
    {
        var estimated = i.Status == InvestmentStatus.Active
            ? FinancialCalculator.FutureValue(i.Principal, i.AnnualRatePercent, FinancialCalculator.MonthsBetween(i.StartDateUtc, now))
            : i.RedeemedAmount ?? i.Principal;

        return new InvestmentDto
        {
            Id = i.Id,
            ProductName = i.ProductName,
            Principal = i.Principal,
            AnnualRatePercent = i.AnnualRatePercent,
            StartDateUtc = i.StartDateUtc,
            EstimatedValue = estimated,
            StatusLabel = i.Status == InvestmentStatus.Active ? "Ativo" : "Resgatado",
            IsActive = i.Status == InvestmentStatus.Active
        };
    }
}
