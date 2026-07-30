using BankingSystem.Domain.Common;

namespace BankingSystem.Domain.Entities;

public class LoginHistory : BaseEntity
{
    public Guid? UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Machine { get; set; } = string.Empty;
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
