namespace BankingSystem.Application.DTOs.Security;

public sealed class LoginHistoryDto
{
    public DateTime TimestampUtc { get; init; }
    public string Email { get; init; } = string.Empty;
    public string ResultLabel { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string Machine { get; init; } = string.Empty;
}
