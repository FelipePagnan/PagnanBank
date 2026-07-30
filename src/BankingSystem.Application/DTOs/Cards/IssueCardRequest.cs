using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.DTOs.Cards;

public sealed class IssueCardRequest
{
    public Guid AccountId { get; set; }
    public CardType Type { get; set; } = CardType.Virtual;
    public decimal Limit { get; set; }
}
