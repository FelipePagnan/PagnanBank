namespace BankingSystem.Application.DTOs.Cards;

public sealed class CardDto
{
    public Guid Id { get; init; }
    public string TypeLabel { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string HolderName { get; init; } = string.Empty;
    public string ExpiryLabel { get; init; } = string.Empty;
    public decimal Limit { get; init; }
    public decimal UsedAmount { get; init; }
    public decimal AvailableLimit { get; init; }
    public string StatusLabel { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
