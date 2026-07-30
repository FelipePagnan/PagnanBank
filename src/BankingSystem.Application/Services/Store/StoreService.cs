using BankingSystem.Application.Abstractions.Repositories;
using BankingSystem.Application.Common.Errors;
using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.DTOs.Store;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Common;
using BankingSystem.Domain.Entities;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.Services.Store;

public sealed class StoreService : IStoreService
{
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly IAccountRepository _accounts;
    private readonly ICardRepository _cards;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditService _audit;
    private readonly IDateTimeProvider _clock;

    public StoreService(
        IProductRepository products,
        IOrderRepository orders,
        IAccountRepository accounts,
        ICardRepository cards,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork,
        IAuditService audit,
        IDateTimeProvider clock)
    {
        _products = products;
        _orders = orders;
        _accounts = accounts;
        _cards = cards;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _clock = clock;
    }

    public async Task<List<ProductDto>> GetCatalogAsync(CancellationToken ct = default)
    {
        var products = await _products.GetActiveAsync(ct);
        return products.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Category = p.Category,
            Price = p.Price,
            CashbackPercent = p.CashbackPercent
        }).ToList();
    }

    public async Task<Result<OrderSummaryDto>> CheckoutAsync(CheckoutRequest request, CancellationToken ct = default)
    {
        var items = request.Items.Where(i => i.Quantity > 0).ToList();
        if (items.Count == 0)
            return Result.Failure<OrderSummaryDto>(DomainErrors.Store.EmptyCart);

        var account = await _accounts.GetByIdAsync(request.AccountId, ct);
        if (account is null)
            return Result.Failure<OrderSummaryDto>(DomainErrors.Accounts.NotFound);
        if (!account.IsActive)
            return Result.Failure<OrderSummaryDto>(DomainErrors.Accounts.Blocked);

        var products = await _products.GetByIdsAsync(items.Select(i => i.ProductId), ct);
        var productMap = products.ToDictionary(p => p.Id);

        decimal total = 0m;
        decimal cashback = 0m;
        var now = _clock.UtcNow;

        var order = new Order
        {
            AccountId = account.Id,
            CreatedAtUtc = now,
            PaymentMethod = request.PaymentMethod,
            Installments = request.PaymentMethod == PaymentMethod.Credit ? Math.Max(1, request.Installments) : 1,
            Status = OrderStatus.Confirmed
        };

        foreach (var item in items)
        {
            if (!productMap.TryGetValue(item.ProductId, out var product))
                return Result.Failure<OrderSummaryDto>(DomainErrors.Store.ProductNotFound);

            var lineTotal = product.Price * item.Quantity;
            total += lineTotal;
            cashback += decimal.Round(lineTotal * product.CashbackPercent / 100m, 2);

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = item.Quantity
            });
        }

        Card? card = null;

        if (request.PaymentMethod == PaymentMethod.Debit)
        {
            if (account.Balance < total)
                return Result.Failure<OrderSummaryDto>(DomainErrors.Accounts.InsufficientFunds);

            account.Balance -= total;
            account.UpdatedAtUtc = now;
            _accounts.Update(account);
            await _transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Purchase,
                Amount = total,
                BalanceAfter = account.Balance,
                Description = $"Compra na loja ({order.Items.Count} item(ns))",
                TimestampUtc = now
            }, ct);
        }
        else // Credit
        {
            if (request.CardId is null)
                return Result.Failure<OrderSummaryDto>(DomainErrors.Store.CardRequired);

            card = await _cards.GetByIdAsync(request.CardId.Value, ct);
            if (card is null)
                return Result.Failure<OrderSummaryDto>(DomainErrors.Cards.NotFound);
            if (!card.IsActive)
                return Result.Failure<OrderSummaryDto>(DomainErrors.Cards.Blocked);
            if (card.AvailableLimit < total)
                return Result.Failure<OrderSummaryDto>(DomainErrors.Cards.LimitExceeded);

            card.UsedAmount += total;
            card.UpdatedAtUtc = now;
            _cards.Update(card);
            order.CardId = card.Id;
        }

        // Cashback is always credited to the account balance.
        if (cashback > 0)
        {
            account.Balance += cashback;
            account.UpdatedAtUtc = now;
            _accounts.Update(account);
            await _transactions.AddAsync(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Cashback,
                Amount = cashback,
                BalanceAfter = account.Balance,
                Description = "Cashback de compra",
                TimestampUtc = now
            }, ct);
        }

        order.Total = total;
        order.CashbackAmount = cashback;

        await _orders.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.LogAsync("Checkout", "Store", OperationResult.Success,
            $"Compra de {total:C} ({(request.PaymentMethod == PaymentMethod.Debit ? "débito" : $"crédito em {order.Installments}x")}), cashback {cashback:C}.",
            account.UserId, account.User?.FullName ?? "", ct);

        return Result.Success(new OrderSummaryDto
        {
            OrderId = order.Id,
            Total = total,
            CashbackAmount = cashback,
            Installments = order.Installments,
            PaymentLabel = request.PaymentMethod == PaymentMethod.Debit ? "Débito" : $"Crédito em {order.Installments}x"
        });
    }
}
