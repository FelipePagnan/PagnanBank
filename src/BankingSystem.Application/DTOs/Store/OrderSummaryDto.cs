namespace BankingSystem.Application.DTOs.Store;

public sealed class OrderSummaryDto
{
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
    public decimal CashbackAmount { get; init; }
    public int Installments { get; init; }
    public string PaymentLabel { get; init; } = string.Empty;
}
