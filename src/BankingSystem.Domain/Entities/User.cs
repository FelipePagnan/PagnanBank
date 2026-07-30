using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Client;
    public UserStatus Status { get; set; } = UserStatus.Active;

    public int FailedLoginAttempts { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    public bool IsAdministrator => Role == UserRole.Administrator;
}
