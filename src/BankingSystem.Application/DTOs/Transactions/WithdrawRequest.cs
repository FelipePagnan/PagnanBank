namespace BankingSystem.Application.DTOs.Transactions;

public sealed class WithdrawRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "Saque";
}
