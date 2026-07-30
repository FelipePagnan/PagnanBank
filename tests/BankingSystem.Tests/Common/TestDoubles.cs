using BankingSystem.Application.Common.Interfaces;
using BankingSystem.Application.DTOs.Audit;
using BankingSystem.Application.Services.Audit;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Tests.Common;

/// <summary>Deterministic clock for tests.</summary>
public sealed class FixedDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; set; } = new(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
}

/// <summary>Fast, non-cryptographic hasher used only in tests.</summary>
public sealed class FakePasswordHasher : IPasswordHasher
{
    public string Hash(string password) => "hashed:" + password;
    public bool Verify(string password, string hash) => hash == "hashed:" + password;
}

/// <summary>No-op audit service so tests focus on business behaviour.</summary>
public sealed class NullAuditService : IAuditService
{
    public Task LogAsync(
        string operation, string module, OperationResult result,
        string details = "", Guid? userId = null, string userName = "",
        CancellationToken ct = default) => Task.CompletedTask;

    public Task<List<AuditLogDto>> GetRecentAsync(int take = 200, CancellationToken ct = default)
        => Task.FromResult(new List<AuditLogDto>());
}

/// <summary>Configurable current-user stub (defaults to an administrator).</summary>
public sealed class TestCurrentUser : ICurrentUser
{
    public Guid? UserId { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = "Admin de Teste";
    public UserRole? Role { get; set; } = UserRole.Administrator;
    public bool IsAuthenticated => UserId.HasValue;
}
