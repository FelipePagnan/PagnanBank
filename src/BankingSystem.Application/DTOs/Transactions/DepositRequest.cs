namespace BankingSystem.Application.DTOs.Transactions;

public sealed class DepositRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = "Depósito";
}
