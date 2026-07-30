using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class Transaction : BaseEntity
{
    public Guid AccountId { get; set; }
    public Account? Account { get; set; }

    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }

    /// <summary>Account balance immediately after this movement was applied.</summary>
    public decimal BalanceAfter { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>For transfers/PIX: the account on the other side of the operation.</summary>
    public Guid? CounterpartAccountId { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
