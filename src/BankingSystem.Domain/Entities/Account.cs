using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class Account : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Branch { get; set; } = "0001";   // Agência
    public string Number { get; set; } = string.Empty; // Número da conta

    public AccountType Type { get; set; } = AccountType.Checking;
    public AccountStatus Status { get; set; } = AccountStatus.Active;

    public decimal Balance { get; set; }
    public decimal DailyLimit { get; set; } = 5000m;

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public bool IsActive => Status == AccountStatus.Active;
}
