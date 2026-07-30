using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.DTOs.Cards;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Common;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;
using FluentValidation;

namespace BankingSystem.Application.Services.Cards;

public sealed class CardService : ICardService
{
    private readonly ICardRepository _cards;
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;
    private readonly IDateTimeProvider _clock;
    private readonly IValidator<IssueCardRequest> _validator;

    public CardService(
        ICardRepository cards,
        IAccountRepository accounts,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        IAuditService audit,
        IDateTimeProvider clock,
        IValidator<IssueCardRequest> validator)
    {
        _cards = cards;
        _accounts = accounts;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _clock = clock;
        _validator = validator;
    }

    public async Task<Result<CardDto>> IssueAsync(IssueCardRequest request, CancellationToken ct = default)
    {
        var validation = await _validator.ValidateAsync(request, ct);
        if (!validation.IsValid)
            return Result.Failure<CardDto>(
                DomainErrors.Validation.Rule(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage))));

        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure<CardDto>(DomainErrors.Accounts.NotFound);

        var now = _clock.UtcNow;
        var card = new Card
        {
            AccountId = account.Id,
            Type = request.Type,
            HolderName = account.User?.FullName ?? "TITULAR",
            Number = GenerateMaskedNumber(),
            ExpiryDateUtc = now.AddYears(4),
            Limit = request.Limit,
            UsedAmount = 0m,
            Status = CardStatus.Active
        };

        await _cards.AddAsync(card, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("IssueCard", "Cards", OperationResult.Success,
            $"Cartão {request.Type} emitido (limite {request.Limit:C}).",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success(ToDto(card));
    }

    public Task<Result> BlockAsync(Guid cardId, CancellationToken ct = default)
        => SetStatusAsync(cardId, CardStatus.Blocked, "BlockCard", ct);

    public Task<Result> UnblockAsync(Guid cardId, CancellationToken ct = default)
        => SetStatusAsync(cardId, CardStatus.Active, "UnblockCard", ct);

    private async Task<Result> SetStatusAsync(Guid cardId, CardStatus status, string operation, CancellationToken ct)
    {
        var card = await _cards.GetByIdAsync(cardId, ct);
        if (card is null)
            return Result.Failure(DomainErrors.Cards.NotFound);

        card.Status = status;
        card.UpdatedAtUtc = _clock.UtcNow;
        _cards.Update(card);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync(operation, "Cards", OperationResult.Success,
            $"Status do cartão alterado para {status}.",
            card.Account?.UserId, card.Account?.User?.FullName ?? "", ct);

        return Result.Success();
    }

    public async Task<Result> SetLimitAsync(Guid cardId, decimal newLimit, CancellationToken ct = default)
    {
        if (newLimit < 0)
            return Result.Failure(DomainErrors.Validation.Rule("O limite não pode ser negativo."));

        var card = await _cards.GetByIdAsync(cardId, ct);
        if (card is null)
            return Result.Failure(DomainErrors.Cards.NotFound);
        if (newLimit < card.UsedAmount)
            return Result.Failure(DomainErrors.Cards.LimitBelowUsed);

        card.Limit = newLimit;
        card.UpdatedAtUtc = _clock.UtcNow;
        _cards.Update(card);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("SetCardLimit", "Cards", OperationResult.Success,
            $"Limite do cartão ajustado para {newLimit:C}.",
            card.Account?.UserId, card.Account?.User?.FullName ?? "", ct);

        return Result.Success();
    }

    public async Task<Result> PayInvoiceAsync(Guid cardId, CancellationToken ct = default)
    {
        var card = await _cards.GetByIdAsync(cardId, ct);
        if (card is null)
            return Result.Failure(DomainErrors.Cards.NotFound);
        if (card.UsedAmount <= 0)
            return Result.Failure(DomainErrors.Cards.NoInvoice);

        var account = await _accounts.GetByIdAsync(card.AccountId, ct);
        if (account is null)
            return Result.Failure(DomainErrors.Accounts.NotFound);
        if (account.Balance < card.UsedAmount)
            return Result.Failure(DomainErrors.Accounts.InsufficientFunds);

        var now = _clock.UtcNow;
        var invoice = card.UsedAmount;

        account.Balance -= invoice;
        account.UpdatedAtUtc = now;
        card.UsedAmount = 0m;
        card.UpdatedAtUtc = now;

        _accounts.Update(account);
        _cards.Update(card);
        await _transactions.AddAsync(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.CardInvoicePayment,
            Amount = invoice,
            BalanceAfter = account.Balance,
            Description = $"Pagamento de fatura do cartão {card.Number}",
            TimestampUtc = now
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("PayCardInvoice", "Cards", OperationResult.Success,
            $"Pagamento de fatura de {invoice:C}.",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success();
    }

    public async Task<List<CardDto>> GetByUserAsync(Guid userId, CancellationToken ct = default)
    {
        var cards = await _cards.GetByUserIdAsync(userId, ct);
        return cards.Select(ToDto).ToList();
    }

    private static string GenerateMaskedNumber()
        => $"**** **** **** {Random.Shared.Next(1000, 9999)}";

    private static CardDto ToDto(Card c) => new()
    {
        Id = c.Id,
        TypeLabel = c.Type == CardType.Virtual ? "Virtual" : "Físico",
        Number = c.Number,
        HolderName = c.HolderName,
        ExpiryLabel = c.ExpiryDateUtc.ToString("MM/yy"),
        Limit = c.Limit,
        UsedAmount = c.UsedAmount,
        AvailableLimit = c.AvailableLimit,
        StatusLabel = c.Status == CardStatus.Active ? "Ativo" : "Bloqueado",
        IsActive = c.IsActive
    };
}
