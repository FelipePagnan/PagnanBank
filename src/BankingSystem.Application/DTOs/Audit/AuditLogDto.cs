namespace BankingSystem.Application.DTOs.Audit;

public sealed class AuditLogDto
{
    public DateTime TimestampUtc { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Operation { get; init; } = string.Empty;
    public string Module { get; init; } = string.Empty;
    public string ResultLabel { get; init; } = string.Empty;
    public string Details { get; init; } = string.Empty;
}
