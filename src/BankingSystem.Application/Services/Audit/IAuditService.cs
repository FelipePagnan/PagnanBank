using BankingSystem.Application.DTOs.Audit;
using BankingSystem.Domain.Enums;

namespace BankingSystem.Application.Services.Audit;

public interface IAuditService
{
    Task LogAsync(
        string operation,
        string module,
        OperationResult result,
        string details = "",
        Guid? userId = null,
        string userName = "",
        CancellationToken ct = default);

    Task<List<AuditLogDto>> GetRecentAsync(int take = 200, CancellationToken ct = default);
}
