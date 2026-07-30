namespace BankingSystem.Application.DTOs.Transactions;

public sealed class TransferRequest
{
    public Guid SourceAccountId { get; set; }
    public string DestinationAccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>When true the movement is recorded as PIX instead of a regular transfer.</summary>
    public bool IsPix { get; set; }
}
