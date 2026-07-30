using BankingSystem.Domain.Common;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Domain.Entities;

public class AuditLog : BaseEntity
{
    public Guid? UserId { get; set; }
    public string UserName { get; set; } = string.Empty;

    public string Operation { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;

    public OperationResult Result { get; set; }
    public string Details { get; set; } = string.Empty;

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}
