using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.DTOs.Store;

public sealed class CheckoutItem
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class CheckoutRequest
{
    public Guid AccountId { get; set; }
    public List<CheckoutItem> Items { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Debit;
    public int Installments { get; set; } = 1;
    public Guid? CardId { get; set; }
}
