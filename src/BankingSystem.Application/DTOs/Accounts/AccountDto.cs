using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.DTOs.Accounts;

public sealed class AccountDto
{
    public Guid Id { get; init; }
    public string Branch { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public AccountType Type { get; init; }
    public AccountStatus Status { get; init; }
    public decimal Balance { get; init; }
    public decimal DailyLimit { get; init; }
    public string OwnerName { get; init; } = string.Empty;
}
