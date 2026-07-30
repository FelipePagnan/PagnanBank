using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class Card : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public CardType Type { get; set; } = CardType.Virtual;
    public string HolderName { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;   // masked, e.g. **** **** **** 1234
    public DateTime ExpiryDateUtc { get; set; }

    public decimal Limit { get; set; }
    public decimal UsedAmount { get; set; }
    public CardStatus Status { get; set; } = CardStatus.Active;

    public decimal AvailableLimit => Limit - UsedAmount;
    public bool IsActive => Status == CardStatus.Active;
}
